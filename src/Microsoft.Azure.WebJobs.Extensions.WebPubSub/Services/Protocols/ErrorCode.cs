using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ErrorCode
    {
        [EnumMember(Value = "unauthorized")]
        Unauthorized,
        [EnumMember(Value = "userError")]
        UserError,
        [EnumMember(Value = "serverError")]
        ServerError
    }
}
