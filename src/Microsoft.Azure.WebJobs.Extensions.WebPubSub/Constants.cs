using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal static class Constants
    {
        public const string WebPubSubConnectionStringName = "WebPubSubConnectionString";

        public static class ContentTypes
        {
            public const string JsonContentType = "application/json";
            public const string BinaryContentType = "application/octet-stream";
            public const string PlainTextContentType = "text/plain";
        }

        public const string DefaultHub = "_default";

        public const string CloudEventTypeSystemPrefix = "azure.webpubsub.sys.";
        public const string CloudEventTypeUserPrefix = "azure.webpubsub.user.";

        //public const string AsrsHeaderPrefix = "X-ASRS-";
        //public const string AsrsConnectionIdHeader = AsrsHeaderPrefix + "Connection-Id";
        //public const string AsrsUserClaims = AsrsHeaderPrefix + "User-Claims";
        //public const string AsrsUserId = AsrsHeaderPrefix + "User-Id";
        //public const string AsrsHubNameHeader = AsrsHeaderPrefix + "Hub";
        //public const string AsrsCategory = AsrsHeaderPrefix + "Category";
        //public const string AsrsEvent = AsrsHeaderPrefix + "Event";
        //public const string AsrsClientQueryString = AsrsHeaderPrefix + "Client-Query";
        //public const string AsrsSignature = AsrsHeaderPrefix + "Signature";

        public const char HeaderSeparator = ',';
        public const string ClaimsSeparator = ": ";

        public static class Categories
        {
            public const string Messages = "messages";
            public const string Connections = "connections";
        }

        public static class Events
        {
            public const string ConnectEvent = "connect";
            public const string ConnectedEvent = "connected";
            public const string MessageEvent = "message";
            public const string DisconnectedEvent = "disconnected";
        }

        public static class CloudEvents
        {
            public static class Headers
            {
                private const string Prefix = "ce-";
                public const string Signature = Prefix + "signature";
                public const string Hub = Prefix + "hub";
                public const string ConnectionId = Prefix + "connectionId";
                public const string Id = Prefix + "id";
                public const string Time = Prefix + "time";
                public const string SpecVersion = Prefix + "specversion";
                public const string Type = Prefix + "type";
                public const string Source = Prefix + "source";
                public const string EventName = Prefix + "eventName";
                public const string UserId = Prefix + "userId";
            }
        }
    }
}
