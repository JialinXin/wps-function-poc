using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject]
    public class ConnectionContext
    {
        /// <summary>
        /// The type of the message.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; internal set; }

        /// <summary>
        /// The event of the message.
        /// </summary>
        [JsonProperty("event")]
        public string Event { get; internal set; }

        /// <summary>
        /// The hub which message belongs to.
        /// </summary>
        [JsonProperty("hub")]
        public string Hub { get; internal set; }

        /// <summary>
        /// The connection-id of the client which send the message.
        /// </summary>
        [JsonProperty("connectionId")]
        public string ConnectionId { get; internal set; }

        /// <summary>
        /// The user identity of the client which send the message.
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; internal set; }

        /// <summary>
        /// The headers of request.
        /// Headers with duplicated key will be joined by comma.
        /// </summary>
        [JsonProperty("headers")]
        public Dictionary<string, StringValues> Headers { get; internal set; }
    }
}
