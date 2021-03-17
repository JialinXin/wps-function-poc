using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WebPubSubOperation
    {
        [EnumMember(Value = "sendToAll")]
        SendToAll,
        [EnumMember(Value = "closeClientConnection")]
        CloseClientConnection,
        [EnumMember(Value = "sendToConnection")]
        SendToConnection,
        [EnumMember(Value = "sendToGroup")]
        SendToGroup,
        [EnumMember(Value = "addConnectionToGroup")]
        AddConnectionToGroup,
        [EnumMember(Value = "removeConnectionFromGroup")]
        RemoveConnectionFromGroup,
        [EnumMember(Value = "sendToUser")]
        SendToUser,
        [EnumMember(Value = "addToGroup")]
        AddUserToGroup,
        [EnumMember(Value = "removeUserFromGroup")]
        RemoveUserFromGroup,
        [EnumMember(Value = "removeUserFromAllGroups")]
        RemoveUserFromAllGroups,
        [EnumMember(Value = "grandGroupPermission")]
        GrantGroupPermission,
        [EnumMember(Value = "revokeGroupPermission")]
        RevokeGroupPermission
    }
}
