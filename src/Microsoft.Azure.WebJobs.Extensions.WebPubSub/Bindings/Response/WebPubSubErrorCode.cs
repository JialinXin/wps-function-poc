﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WebPubSubErrorCode
    {
        [EnumMember(Value = "unauthorized")]
        Unauthorized,
        [EnumMember(Value = "userError")]
        UserError,
        [EnumMember(Value = "serverError")]
        ServerError
    }
}
