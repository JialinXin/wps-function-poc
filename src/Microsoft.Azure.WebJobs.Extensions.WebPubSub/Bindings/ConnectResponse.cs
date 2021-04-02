using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Description;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ConnectResponse
    {
        public Error Error { get; set; }

        public string UserId { get; set; }

        public string[] Groups { get; set; }

        public string Subprotocol { get; set; }

        public string[] Roles { get; set; }
    }
}
