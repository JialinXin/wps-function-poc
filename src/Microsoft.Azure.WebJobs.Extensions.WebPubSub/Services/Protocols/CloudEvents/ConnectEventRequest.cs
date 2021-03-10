using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal sealed class ConnectEventRequest
    {
        [JsonProperty("claims")]
        public IDictionary<string, string[]> Claims { get; set; }

        [JsonProperty("query")]
        public IDictionary<string, string[]> Query { get; set; }

        [JsonProperty("subprotocols")]
        public string[] Subprotocols { get; set; }

        [JsonProperty("clientCertificates")]
        public ClientCertificateInfo[] ClientCertificates { get; set; }
    }
}
