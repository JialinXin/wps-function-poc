using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal static class Constants
    {
        public const string WebPubSubConnectionStringName = "WebPubSubConnectionString";

        public const string BinaryContentType = "application/octet-stream";
        public const string PlainTextContentType = "text/plain";

        public const string AsrsHeaderPrefix = "X-ASRS-";
        public const string AsrsConnectionIdHeader = AsrsHeaderPrefix + "Connection-Id";
        public const string AsrsUserClaims = AsrsHeaderPrefix + "User-Claims";
        public const string AsrsUserId = AsrsHeaderPrefix + "User-Id";
        public const string AsrsHubNameHeader = AsrsHeaderPrefix + "Hub";
        public const string AsrsCategory = AsrsHeaderPrefix + "Category";
        public const string AsrsEvent = AsrsHeaderPrefix + "Event";
        public const string AsrsClientQueryString = AsrsHeaderPrefix + "Client-Query";
        public const string AsrsSignature = AsrsHeaderPrefix + "Signature";

        public const char HeaderSeparator = ',';
        public const string ClaimsSeparator = ": ";

        public static class Categories
        {
            public const string Messages = "messages";
            public const string Connections = "connections";
        }

        public static class Events
        {
            public const string Connect = "connect";
            public const string Disconnect = "disconnect";
            public const string Message = "message";
        }
    }
}
