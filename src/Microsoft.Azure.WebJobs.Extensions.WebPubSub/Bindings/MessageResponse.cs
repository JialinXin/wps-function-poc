using System.IO;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Description;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class MessageResponse
    {
        public Error Error { get; set; }

        public WebPubSubMessage Message { get; set; }
    }
}
