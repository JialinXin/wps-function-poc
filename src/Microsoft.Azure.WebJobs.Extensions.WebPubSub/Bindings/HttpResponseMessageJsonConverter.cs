﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class HttpResponseMessageJsonConverter : JsonConverter<HttpResponseMessage>
    {
        public override HttpResponseMessage ReadJson(JsonReader reader, Type objectType, HttpResponseMessage existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<HttpResponseMessage>(reader);
        }

        public override void WriteJson(JsonWriter writer, HttpResponseMessage value, JsonSerializer serializer)
        {
            var simpleRes = SimpleResponse.FromHttpResponse(value);
            serializer.Serialize(writer, JObject.FromObject(simpleRes));
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        private sealed class SimpleResponse
        {
            public Stream Body { get; set; }

            public int Status { get; set; }

            public Dictionary<string, StringValues> Headers { get; set; }

            public static SimpleResponse FromHttpResponse(HttpResponseMessage response)
            {
                return new SimpleResponse
                {
                    Body = response.Content?.ReadAsStreamAsync().Result,
                    Status = (int)response.StatusCode,
                    Headers = response.Headers.ToDictionary(x => x.Key, v => new StringValues(v.Value.ToArray()), StringComparer.OrdinalIgnoreCase)
                };
            }
        }
    }
}
