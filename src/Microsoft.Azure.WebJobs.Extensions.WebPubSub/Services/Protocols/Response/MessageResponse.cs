using System.IO;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject]
    [Binding]
    public class MessageResponse
    {
        [JsonProperty("error")]
        public Error Error { get; set; }
        [JsonProperty("message")]
        public WebPubSubMessage Message { get; set; }
        [JsonProperty("datatype")]
        public MessageDataType DataType { get; set; }
    }
}
