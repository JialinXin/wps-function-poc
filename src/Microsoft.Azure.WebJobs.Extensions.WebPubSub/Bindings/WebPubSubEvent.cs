using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class WebPubSubEvent
    {
        [JsonRequired]
        public WebPubSubOperation Operation { get; set; }

        public string GroupId { get; set; }

        public string UserId { get; set; }

        public string ConnectionId { get; set; }

        public string[] Excluded { get; set; }

        public string Reason { get; set; }

        public string Permission { get; set; }

        public WebPubSubMessage Message { get; set; }
    }
}
