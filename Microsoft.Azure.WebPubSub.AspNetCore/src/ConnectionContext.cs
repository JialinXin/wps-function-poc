﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    /// <summary>
    /// Request context from headers following CloudEvents.
    /// </summary>
    public class ConnectionContext
    {
        private static ReadOnlyDictionary<string, object> _states;
        private static ReadOnlyDictionary<string, StringValues> _headers;
        /// <summary>
        /// The type of the message.
        /// </summary>
        [JsonPropertyName("eventType")]
        public WebPubSubEventType EventType { get; internal set; }

        /// <summary>
        /// The event name of the message.
        /// </summary>
        [JsonPropertyName("eventName")]
        public string EventName { get; internal set; }

        /// <summary>
        /// The hub which the connection belongs to.
        /// </summary>
        [JsonPropertyName("hub")]
        public string Hub { get; internal set; }

        /// <summary>
        /// The connection-id of the client.
        /// </summary>
        [JsonPropertyName("connectionId")]
        public string ConnectionId { get; internal set; }

        /// <summary>
        /// The user identity of the client.
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; internal set; }

        /// <summary>
        /// The signature for validation.
        /// </summary>
        [JsonPropertyName("signature")]
        public string Signature { get; internal set; }

        /// <summary>
        /// Upstream origin.
        /// </summary>
        [JsonPropertyName("origin")]
        public string Origin { get; internal set; }

        /// <summary>
        /// The connection states.
        /// </summary>
        [JsonPropertyName("states")]
        public ReadOnlyDictionary<string, object> States => _states;

        /// <summary>
        /// The headers of request.
        /// </summary>
        [JsonPropertyName("headers")]
        public ReadOnlyDictionary<string, StringValues> Headers => _headers;

        internal void InitStates(Dictionary<string, object> states)
        {
            _states = new ReadOnlyDictionary<string, object>(states);
        }
        internal void InitHeaders(Dictionary<string, StringValues> headers)
        {
            _headers = new ReadOnlyDictionary<string, StringValues>(headers);
        }
    }
}
