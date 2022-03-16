using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static LibFoxyProxy.Http.HttpUtilities;
using FoxyProxyHttpProcessDelegate = System.Func<LibFoxyProxy.Http.HttpRequest, LibFoxyProxy.Http.HttpResponse, System.Threading.Tasks.Task<bool>>;

namespace LibFoxyProxy.Http;

public interface ICacheDb
{
    public T Get<T>(string key);

    public void Set<T>(string key, TimeSpan ttl, T value);
}

public class HttpProxy : Listener
{
    static readonly Dictionary<HttpStatusCode, string> ErrorPages = new()
    {
        { HttpStatusCode.NotFound, new StreamReader(typeof(HttpProxy).Assembly.GetManifestResourceStream("LibFoxyProxy.Http.www.errors.404.html")).ReadToEnd() },
        { HttpStatusCode.InternalServerError, new StreamReader(typeof(HttpProxy).Assembly.GetManifestResourceStream("LibFoxyProxy.Http.www.errors.500.html")).ReadToEnd() }
    };

    public static readonly string ApplicationVersion = typeof(HttpProxy).Assembly.GetName().Version?.ToString() ?? "NA";

    public readonly List<FoxyProxyHttpProcessDelegate> Handlers = new();

    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public ICacheDb CacheDb { get; set; }

    public HttpProxy(IPAddress listenAddress, int port) : base(listenAddress, port, SocketType.Stream, ProtocolType.Tcp) { }

    public HttpProxy Use(FoxyProxyHttpProcessDelegate delegateFunc)
    {
        Handlers.Add(delegateFunc);

        return this;
    }

    internal override async Task ProcessRequest(Socket connection, byte[] data, int read)
    {
        var httpRequest = HttpRequest.Parse(connection, Encoding, data[..read]);

        var httpResponse = new HttpResponse(httpRequest);

        var key = $"PC-{httpRequest.Uri}";

        var cachedResponse = CacheDb?.Get<string>(key);

        if (cachedResponse == null)
        {
            Console.WriteLine("Cache MISS: " + key);

            var handled = false;

            foreach (var handler in Handlers)
            {
                if (handled = await handler(httpRequest, httpResponse))
                {
                    break;
                }
            }

            if (!handled)
            {
                // TODO: Add Error Handling...
                handled = ProcessErrorResponse(httpRequest, httpResponse, HttpStatusCode.NotFound);
            }

            if (connection == null)
            {
                // Nothing to send too..
                return;
            }

            if (handled)
            {
                try
                {
                    var buffer = httpResponse.GetResponseEncodedData();

                    if (httpResponse.Cache)
                    {
                        CacheDb?.Set<string>(key, httpResponse.CacheTtl, Convert.ToBase64String(buffer));
                    }

                    await connection.SendAsync(buffer, SocketFlags.None);
                }
                catch (Exception) { }
            }
        }
        else
        {
            Console.WriteLine("Cache  HIT: " + key);

            try
            {
                var buffer = Convert.FromBase64String(cachedResponse);

                await connection.SendAsync(buffer, SocketFlags.None);
            }
            catch (Exception) { }
        }

        // TODO: Keep-Alive respect?
        connection.Close();
    }

    private static bool ProcessErrorResponse(HttpRequest httpRequest, HttpResponse httpResponse, HttpStatusCode statusCode)
    {
        httpResponse.SetStatusCode(statusCode);

        var date = DateTime.UtcNow.ToUniversalTime().ToString("R");

        httpResponse.Headers.Add(HttpHeaderName.Date, date);

        if (!ErrorPages.ContainsKey(statusCode))
        {
            var plainBody = $"{(int)statusCode} {statusCode}\n\n{httpRequest.Uri}\n\n{date}{string.Join("", Enumerable.Repeat("\n" + string.Join("", Enumerable.Repeat(" ", 80)), 20))}";

            httpResponse.SetBodyString(plainBody, HttpContentType.Text.Plain);

            return true;
        }

        var body = ErrorPages[statusCode];

        body = body.Replace("||REQUEST||", httpRequest.Uri?.ToString());
        body = body.Replace("||VERSION||", ApplicationVersion);
        body = body.Replace("||HOST||", httpRequest.Socket?.LocalEndPoint?.ToString());
        body = body.Replace("||DATE||", date);

        httpResponse.SetBodyString(body, HttpContentType.Text.Html);

        return true;
    }
}