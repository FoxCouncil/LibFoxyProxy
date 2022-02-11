using System.Net.Sockets;
using System.Text;
using static LibFoxyProxy.Http.HttpUtilities;

namespace LibFoxyProxy.Http;

public sealed class HttpResponse
{
    public Socket? Socket { get; private set; }

    public Encoding Encoding { get; private set; } = Encoding.UTF8;

    public string Version { get; internal set; } = "HTTP/1.1";

    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    public Dictionary<string, string> Headers { get; internal set; } = new ();

    public string Body { get; set; } = "";

    public HttpResponse(HttpRequest request)
    {
        Encoding = request.Encoding ?? Encoding.UTF8;
        Socket = request.Socket;
        Version = request.Version;
    }

    public byte[] GetBytes()
    {
        var outputBuilder = new StringBuilder();

        outputBuilder.Append($"{Version} {(int)StatusCode} {StatusCode}{HttpSeperator}");

        foreach (var header in Headers)
        {
            outputBuilder.Append($"{header.Key}: {header.Value}{HttpSeperator}");
        }

        outputBuilder.Append($"{HttpHeaderName.Server}: FoxyProxy/{HttpProxy.ApplicationVersion}{HttpSeperator}"); // Fuck them

        outputBuilder.Append(HttpBodySeperator);

        outputBuilder.Append(Body);

        return Encoding.GetBytes(outputBuilder.ToString());
    }
}