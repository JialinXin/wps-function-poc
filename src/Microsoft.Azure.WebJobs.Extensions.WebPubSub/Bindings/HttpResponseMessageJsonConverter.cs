// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.Extensions.Primitives;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class HttpResponseMessageJsonConverter : JsonConverter<HttpResponseMessage>
    {
        public override HttpResponseMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, HttpResponseMessage value, JsonSerializerOptions options)
        {
#pragma warning disable AZC0102 // Do not use GetAwaiter().GetResult().
            var simpleRes = SimpleResponse.FromHttpResponse(value).GetAwaiter().GetResult();
#pragma warning restore AZC0102 // Do not use GetAwaiter().GetResult().
            JsonSerializer.Serialize(writer, simpleRes, options);
        }

        // js accecpts simple HttpResponse object.
        private sealed class SimpleResponse
        {
            [JsonPropertyName("body")]
            public Stream Body { get; set; }

            [JsonPropertyName("status")]
            public int Status { get; set; }

            [JsonPropertyName("headers")]
            public Dictionary<string, StringValues> Headers { get; set; }

            public static async Task<SimpleResponse> FromHttpResponse(HttpResponseMessage response)
            {
                Stream body = null;
                if (response.Content != null)
                {
                    body = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                }
                return new SimpleResponse
                {
                    Body = body,
                    Status = (int)response.StatusCode,
                    Headers = response.Headers.ToDictionary(x => x.Key, v => new StringValues(v.Value.ToArray()), StringComparer.OrdinalIgnoreCase)
                };
            }
        }
    }
}
