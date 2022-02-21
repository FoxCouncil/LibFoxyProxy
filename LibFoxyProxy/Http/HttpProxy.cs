using System.Net;
using System.Net.Sockets;
using System.Text;
using static LibFoxyProxy.Http.HttpUtilities;

namespace LibFoxyProxy.Http;

public class HttpProxy : Listener
{
    public static readonly string ApplicationVersion = typeof(HttpProxy).Assembly.GetName().Version?.ToString() ?? "NA";

    static Dictionary<HttpStatusCode, string> ErrorPages = new()
    {
#pragma warning disable CS8604 // Possible null reference argument.
        { HttpStatusCode.NotFound, new StreamReader(typeof(HttpProxy).Assembly.GetManifestResourceStream("LibFoxyProxy.Http.www.errors.404.html")).ReadToEnd() },
        { HttpStatusCode.InternalServerError, new StreamReader(typeof(HttpProxy).Assembly.GetManifestResourceStream("LibFoxyProxy.Http.www.errors.500.html")).ReadToEnd() }
#pragma warning restore CS8604 // Possible null reference argument.
    };

    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public event Func<HttpRequest, HttpResponse?>? Request;

    public HttpProxy(IPAddress listenAddress, int port) : base(listenAddress, port, SocketType.Stream, ProtocolType.Tcp) { }

    internal override async void ProcessRequest(Socket? connection, byte[] data, int read)
    {
        var httpRequest = HttpRequest.Parse(connection, Encoding, data[..read]);

        var httpResponse = Request?.Invoke(httpRequest);

        if (httpResponse == null)
        {
            // TODO: Add Error Handling...
            httpResponse = ProcessErrorResponse(httpRequest, HttpStatusCode.NotFound);
        }

        if (connection == null)
        {
            // Nothing to send too..
            return;
        }

        if (httpResponse != null)
        {
            try
            {
                await connection.SendAsync(httpResponse.GetResponseEncodedData(), SocketFlags.None);
            }
            catch (SocketException)
            {
                // ignore.
            }
        }

        // TODO: Keep-Alive respect?
        connection.Close();
    }

    private static HttpResponse? ProcessErrorResponse(HttpRequest httpRequest, HttpStatusCode statusCode)
    {
        var httpResponse = new HttpResponse(httpRequest) { StatusCode = statusCode };

        var date = DateTime.UtcNow.ToUniversalTime().ToString("R");

        httpResponse.Headers.Add(HttpHeaderName.Date, date);

        if (!ErrorPages.ContainsKey(statusCode))
        {
            var plainBody = $"{(int)statusCode} {statusCode}\n\n{httpRequest.Uri}\n\n{date}{string.Join("", Enumerable.Repeat("\n" + string.Join("", Enumerable.Repeat(" ", 80)), 20))}";

            return httpResponse.SetBodyString(plainBody, HttpContentType.Text.Plain);
        }

        var body = ErrorPages[statusCode];

        body = body.Replace("||REQUEST||", httpRequest.Uri?.ToString());
        body = body.Replace("||VERSION||", ApplicationVersion);
        body = body.Replace("||HOST||", httpRequest.Socket?.LocalEndPoint?.ToString());
        body = body.Replace("||DATE||", date);

        return httpResponse.SetBodyString(body, HttpContentType.Text.Html);
    }
}