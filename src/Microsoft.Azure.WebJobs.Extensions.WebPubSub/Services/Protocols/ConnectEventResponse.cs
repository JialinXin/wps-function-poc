using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class ConnectEventResponse
    {
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
