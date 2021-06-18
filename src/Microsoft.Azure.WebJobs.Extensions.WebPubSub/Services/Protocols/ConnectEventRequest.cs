// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class ConnectEventRequest : ServiceRequest
    {
        public IDictionary<string, string[]> Claims { get; set; }

        public IDictionary<string, string[]> Query { get; set; }

        public string[] Subprotocols { get; set; }

        public ClientCertificateInfo[] ClientCertificates { get; set; }
    }
}
