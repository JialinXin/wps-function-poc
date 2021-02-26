using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class ConnectEventResponse
    {
        [JsonProperty("headers")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
        public Dictionary<string, StringValues> Headers { get; set; }

        [JsonProperty("subprotocol")]
        public string Subprotocol { get; set; }

        [JsonProperty("roles")]
        public string[] Roles { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("groups")]
        public string[] Groups { get; set; }
    }
}
