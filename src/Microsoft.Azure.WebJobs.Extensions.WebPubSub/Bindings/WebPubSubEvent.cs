using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class WebPubSubEvent
    {
        [Required]
        [JsonRequired, JsonConverter(typeof(StringEnumConverter))]
        public WebPubSubOperation Operation { get; set; }

        public string GroupId { get; set; }

        public string UserId { get; set; }

        public string ConnectionId { get; set; }

        public string[] Excluded { get; set; }

        public string Reason { get; set; }

        public string Permission { get; set; }

        [JsonConverter(typeof(WebPubSubMessageJsonConverter))]
        public WebPubSubMessage Message { get; set; }
    }
}
