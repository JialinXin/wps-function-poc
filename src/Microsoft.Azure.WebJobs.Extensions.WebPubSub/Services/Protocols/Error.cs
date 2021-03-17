using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject]
    public class Error
    {
        [JsonProperty("code")]
        public ErrorCode Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        public Error(ErrorCode code)
        {
            Code = code;
        }

        public Error(ErrorCode code, string message)
        {
            Code = code;
            Message = message;
        }
    }

}
