﻿using System;
using System.Collections.Generic;
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
        /// The error message of close connection event.
        /// Only close connection message can have this property, and it can be empty if connections close with no error.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// The type of the message.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The event of the message.
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// The hub which message belongs to.
        /// </summary>
        public string Hub { get; set; }

        /// <summary>
        /// The connection-id of the client which send the message.
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// The user identity of the client which send the message.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The headers of request.
        /// Headers with duplicated key will be joined by comma.
        /// </summary>
        public IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// The query of the request when client connect to the service.
        /// Queries with duplicated key will be joined by comma.
        /// </summary>
        public IDictionary<string, string> Queries { get; set; }

        /// <summary>
        /// The claims of the client.
        /// If you multiple claims have the same key, only the first one will be reserved.
        /// </summary>
        public IDictionary<string, string> Claims { get; set; }

        /// <summary>
        /// The media type of the message.
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Function name of the trigger as the key to bind the function
        /// </summary>
        public string Function { get; set; }
    }
}
