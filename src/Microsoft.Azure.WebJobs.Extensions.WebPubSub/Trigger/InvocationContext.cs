using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class InvocationContext
    {
        /// <summary>
        /// The arguments of invocation message.
        /// </summary>
        //public object[] Arguments { get; set; }
        public ReadOnlyMemory<byte> Payload { get; set; }

        /// <summary>
        /// The error message of the event.
        /// </summary>
        public string Error { get; set; }

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
        public string UserId { get; set; }

        /// <summary>
        /// The headers of request.
        /// Headers with duplicated key will be joined by comma.
        /// </summary>
        public Dictionary<string, StringValues> Headers { get; set; }

        /// <summary>
        /// The query of the request when client connect to the service.
        /// Queries with duplicated key will be joined by comma.
        /// </summary>
        public IDictionary<string, string> Queries { get; internal set; }

        /// <summary>
        /// The claims of the client.
        /// If you multiple claims have the same key, only the first one will be reserved.
        /// </summary>
        public IDictionary<string, string> Claims { get; set; }

        /// <summary>
        /// The media type of the message.
        /// </summary>
        public string MediaType { get; internal set; }

        /// <summary>
        /// Function name of the trigger as the key to bind the function
        /// </summary>
        internal string Function { get; set; }

        /// <summary>
        /// ResponsiveEvent Properties where server can manage response send to service.
        /// </summary>
        #region ResponsiveEvent Properties

        public string[] Roles { get; set; }

        public string Subprotocol { get; set; }

        public string[] Groups { get; set; }

        public HttpStatusCode StatusCode { get; set; }
        #endregion
    }
}
