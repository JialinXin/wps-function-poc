using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ConnectionContext
    {
        /// <summary>
        /// The type of the message.
        /// </summary>
        public string Type { get; internal set; }

        /// <summary>
        /// The event of the message.
        /// </summary>
        public string Event { get; internal set; }

        /// <summary>
        /// The hub which message belongs to.
        /// </summary>
        public string Hub { get; internal set; }

        /// <summary>
        /// The connection-id of the client which send the message.
        /// </summary>
        public string ConnectionId { get; internal set; }

        /// <summary>
        /// The user identity of the client which send the message.
        /// </summary>
        public string UserId { get; internal set; }

        /// <summary>
        /// The signature for validation
        /// </summary>
        public string Signature { get; internal set; }

        /// <summary>
        /// The headers of request.
        /// Headers with duplicated key will be joined by comma.
        /// </summary>
        public Dictionary<string, StringValues> Headers { get; internal set; }
    }
}
