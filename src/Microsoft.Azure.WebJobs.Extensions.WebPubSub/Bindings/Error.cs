﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Error
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorCode Code { get; set; }

        public string Message { get; set; } = string.Empty;

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
