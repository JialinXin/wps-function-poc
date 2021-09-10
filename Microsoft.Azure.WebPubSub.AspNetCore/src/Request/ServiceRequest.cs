// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    /// <summary>
    /// Web PubSub service request.
    /// </summary>
    public abstract class ServiceRequest
    {
        /// <summary>
        /// ConnectionContext.
        /// </summary>
        [JsonPropertyName("connectionContext")]
        public ConnectionContext ConnectionContext { get; internal set;}

        /// <summary>
        /// Request name.
        /// </summary>
        [JsonPropertyName("name")]
        public abstract string Name { get; }

        /// <summary>
        /// Create instance of ServiceRequest.
        /// </summary>
        /// <param name="context"></param>
        public ServiceRequest(ConnectionContext context)
        {
            ConnectionContext = context;
        }
    }
}
