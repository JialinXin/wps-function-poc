// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public enum MessageDataType
    {
        [EnumMember(Value = "binary")]
        Binary,
        [EnumMember(Value = "json")]
        Json,
        [EnumMember(Value = "text")]
        Text
    }
}
