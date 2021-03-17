using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject]
    public class WebPubSubConnection
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
    }
}
