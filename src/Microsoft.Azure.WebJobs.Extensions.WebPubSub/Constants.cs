using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal static class Constants
    {
        public const string WebPubSubConnectionStringName = "WebPubSubConnectionString";
        public const string HubNameStringName = "HubName";
        public const string AllowedHostsName = "AllowedHosts";

        public static class ContentTypes
        {
            public const string JsonContentType = "application/json";
            public const string BinaryContentType = "application/octet-stream";
            public const string PlainTextContentType = "text/plain";
        }

        public const string DefaultHub = "_default";

        public const string CloudEventTypeSystemPrefix = "azure.webpubsub.sys.";
        public const string CloudEventTypeUserPrefix = "azure.webpubsub.user.";

        public static class EventTypes
        {
            public const string User = "user";
            public const string System = "system";
        }

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

        public static class Headers
        {
            public static class CloudEvents
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

            public const string WebSocketSubProtocols = "Sec-WebSocket-Protocol";

            public const string WebHookRequestOrigin = "WebHook-Request-Origin";
            public const string WebHookAllowedOrigin = "WebHook-Allowed-Origin";
        }

        public static class TriggerNames
        {
            public const string ConnectionContext = nameof(ConnectionContext);
            public const string Message = "message";
            public const string DataType = "datatype";
            public const string Subprotocols = "subprotocols";
            public const string Claims = "claims";
            public const string ClientCertificaties = "clientcertificates";
            public const string Reason = "reason";
        }
    }
}
