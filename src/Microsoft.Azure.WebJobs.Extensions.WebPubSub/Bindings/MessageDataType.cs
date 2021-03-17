using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageDataType
    {
        [EnumMember(Value = "binary")]
        Binary,
        [EnumMember(Value = "text")]
        Text,
        [EnumMember(Value = "json")]
        Json,
        [EnumMember(Value = "")]
        NotSupported
    }
}
