﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

using Azure.Messaging.WebPubSub;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class WebPubSubEvent
    {
        [Required]
        [JsonRequired]
        public Operation Operation { get; set; }

        public string Group { get; set; }

        public string UserId { get; set; }

        public string ConnectionId { get; set; }

        public string[] Excluded { get; set; }

        public string Reason { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WebPubSubPermission Permission { get; set; }

        public Message Message { get; set; }

        public MessageDataType DataType { get; set; } = MessageDataType.Text;
    }
}
