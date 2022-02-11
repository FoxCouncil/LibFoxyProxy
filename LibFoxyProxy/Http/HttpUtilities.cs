﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibFoxyProxy.Http
{
    public static class HttpUtilities
    {
        public static readonly string HttpSeperator = "\r\n";

        public static readonly string HttpBodySeperator = "\r\n\r\n";

        public static readonly IReadOnlyList<string> HttpVerbs = new List<string> { "GET", "POST" }; // TODO: Extend to all!

        public static readonly IReadOnlyList<string> HttpVersions = new List<string> { "HTTP/1.0", "HTTP/1.1" };

        public static class HttpHeaderName
        {
            public const string ContentType = "Content-Type";

            public const string ContentLength = "Content-Length";

            public const string Date = "Date";

            public const string Server = "Server";
        }

        public static class HttpContentType
        {
            public static class Application
            {
                public const string Json = "application/json";
            }

            public static class Text
            {
                public const string Html = "text/html";

                public const string Plain = "text/plain";
            }
        }
    }
}