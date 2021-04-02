using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal sealed class DisconnectEventRequest
    {
        [JsonProperty("reason")]
        public string Reason { get; }
    }
}
