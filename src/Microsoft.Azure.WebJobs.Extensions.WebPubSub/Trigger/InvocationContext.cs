using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class InvocationContext
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
        /// The headers of request.
        /// Headers with duplicated key will be joined by comma.
        /// </summary>
        public Dictionary<string, StringValues> Headers { get; internal set; }

        /// <summary>
        /// The query of the request when client connect to the service.
        /// Queries with duplicated key will be joined by comma.
        /// </summary>
        public IDictionary<string, string> Queries { get; internal set; }

        /// <summary>
        /// The claims of the client.
        /// If you multiple claims have the same key, only the first one will be reserved.
        /// </summary>
        public IDictionary<string, string> Claims { get; internal set; }

        /// <summary>
        /// The media type of the message.
        /// </summary>
        public string MediaType { get; internal set; }
    }
}
