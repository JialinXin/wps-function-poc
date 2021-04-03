﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal static class Constants
    {
        // WebPubSubOptions can be set by customers.
        public const string WebPubSubConnectionStringName = "WebPubSubConnectionString";
        public const string HubNameStringName = "WebPubSubHub";
        public const string AllowedHostsName = "AllowedHosts";

        public static class ContentTypes
        {
            public const string JsonContentType = "application/json";
            public const string BinaryContentType = "application/octet-stream";
            public const string PlainTextContentType = "text/plain";
        }

        public static class EventTypes
        {
            public const string User = "user";
            public const string System = "system";
        }

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

                public const string TypeSystemPrefix = "azure.webpubsub.sys.";
                public const string TypeUserPrefix = "azure.webpubsub.user.";
            }

            public const string WebSocketSubProtocols = "Sec-WebSocket-Protocol";

            public const string WebHookRequestOrigin = "WebHook-Request-Origin";
            public const string WebHookAllowedOrigin = "WebHook-Allowed-Origin";
        }
    }
}
