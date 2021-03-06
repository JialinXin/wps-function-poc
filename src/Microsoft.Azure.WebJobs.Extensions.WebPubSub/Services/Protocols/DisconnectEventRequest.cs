﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Azure.Messaging.WebPubSub
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class DisconnectEventRequest : ServiceRequest
    {
        public string Reason { get; }

        public override string Name => nameof(DisconnectEventRequest);

        public DisconnectEventRequest(string reason)
            : base(false, true)
        {
            Reason = reason;
        }
    }
}
