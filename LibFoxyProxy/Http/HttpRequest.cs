using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static LibFoxyProxy.Http.HttpUtilities;

namespace LibFoxyProxy.Http;

public sealed class HttpRequest
{
    public bool IsValid { get; private set; }

    public string Type { get; private set; } = "";

    public Uri? Uri { get; private set; }

    public string Version { get; private set; } = "";

    public IReadOnlyDictionary<string, string>? Headers { get; private set; }

    public string Body { get; private set; } = "";

    public Socket? Socket { get; private set; }

    public Encoding? Encoding { get; private set; }

    internal static HttpRequest Parse(Socket? socket, Encoding encoding, byte[] rawBytes)
    {
        if (socket == null || rawBytes == null)
        {
            return Invalid;
        }

        var rawRequest = encoding.GetString(rawBytes);

        if (!rawRequest.Contains("\r\n") || !rawRequest.Contains("\r\n\r\n"))
        {
            return Invalid;
        }

        var rawHeaders = rawRequest[..rawRequest.IndexOf(HttpBodySeperator)];

        var rawBody = rawRequest[(rawRequest.IndexOf(HttpBodySeperator) + HttpBodySeperator.Length)..].Trim().Replace("\0", string.Empty);

        var parsedRequestArray = rawHeaders.Trim().Split("\r\n");

        var httpRequestLine = parsedRequestArray[0].Split(" ");

        if (httpRequestLine.Length != 3 || !HttpVerbs.Contains(httpRequestLine[0]) || !HttpVersions.Contains(httpRequestLine[2]))
        {
            return Invalid;
        }

        var headers = new Dictionary<string, string>();

        foreach (var header in parsedRequestArray.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                continue;
            }    

            var splitHeaderKV = header.Split(": ", 2);

            headers.Add(splitHeaderKV[0], splitHeaderKV[1]);
        }

        var newRequest = new HttpRequest
        {
            Type = httpRequestLine[0],
            Uri = new Uri(httpRequestLine[1]),
            Version = httpRequestLine[2],
            Headers = headers,
            Body = rawBody,
            Socket = socket,
            Encoding = encoding
        };

        return newRequest;
    }

    public static readonly HttpRequest Invalid = new() { IsValid = false };
}
