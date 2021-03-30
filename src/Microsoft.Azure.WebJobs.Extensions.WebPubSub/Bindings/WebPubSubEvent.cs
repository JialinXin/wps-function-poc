using System.IO;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject]
    public class WebPubSubEvent
    {
        [JsonProperty("operation"), JsonRequired]
        public WebPubSubOperation Operation { get; set; }

        [JsonProperty("groupId")]
        public string GroupId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("connectionId")]
        public string ConnectionId { get; set; }

        [JsonProperty("excluded")]
        public string[] Excluded { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("permission")]
        public string Permission { get; set; }

        [JsonProperty("message")]
        public WebPubSubMessage Message { get; set; }
    }
}
