using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject]
    public class ConnectResponse
    {
        [JsonProperty("error")]
        public Error Error { get; set; }
        [JsonProperty("userId")]
        public string UserId { get; set; }
        [JsonProperty("groups")]
        public string[] Groups { get; set; }
        [JsonProperty("subprotocol")]
        public string Subprotocol { get; set; }
        [JsonProperty("roles")]
        public string[] Roles { get; set; }
    }
}
