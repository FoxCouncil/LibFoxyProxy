using System.Net.Sockets;
using System.Text;
using static LibFoxyProxy.Http.HttpUtilities;

namespace LibFoxyProxy.Http;

public sealed class HttpResponse
{
    public Socket? Socket { get; private set; }

    public Encoding Encoding { get; private set; } = Encoding.UTF8;

    public string Version { get; internal set; } = "HTTP/1.1";

    public HttpStatusCode StatusCode { get; internal set; } = HttpStatusCode.OK;

    public Dictionary<string, string> Headers { get; internal set; } = new();

    public byte[] Body { get; internal set; }

    public HttpResponse(HttpRequest request)
    {
        Encoding = request.Encoding ?? Encoding.UTF8;
        Socket = request.Socket;
        Version = request.Version;
    }

    public HttpResponse SetBodyString(string body, string type = HttpContentType.Text.Html)
    {
        SetBodyData(Encoding.GetBytes(body), type);

        return this;
    }

    public HttpResponse SetBodyData(byte[] body, string type = HttpContentType.Application.OctetStream)
    {
        Body = body ?? throw new ArgumentNullException(nameof(body));

        Headers.AddOrUpdate(HttpHeaderName.ContentLength, Body.Length.ToString());
        Headers.AddOrUpdate(HttpHeaderName.ContentType, type);

        return this;
    }

    public HttpResponse SetEncoding(Encoding encoding)
    {
        Encoding = encoding;

        return this;
    }

    public HttpResponse SetOk()
    {
        StatusCode = HttpStatusCode.OK;

        return this;
    }

    public byte[] GetResponseEncodedData()
    {
        var outputBuilder = new StringBuilder();

        outputBuilder.Append($"{Version} {(int)StatusCode} {StatusCode}{HttpSeperator}");

        foreach (var header in Headers)
        {
            outputBuilder.Append($"{header.Key}: {header.Value}{HttpSeperator}");
        }

        outputBuilder.Append($"{HttpHeaderName.Server}: LibFoxyProxy/{HttpProxy.ApplicationVersion}"); // Fuck them

        outputBuilder.Append(HttpBodySeperator);

        var headerData = Encoding.GetBytes(outputBuilder.ToString());

        return headerData.Concat(Body).ToArray();
    }
}