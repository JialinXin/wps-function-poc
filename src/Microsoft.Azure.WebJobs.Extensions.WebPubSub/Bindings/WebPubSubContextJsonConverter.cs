// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubContextJsonConverter : JsonConverter<WebPubSubContext>
    {
        public override WebPubSubContext ReadJson(JsonReader reader, Type objectType, WebPubSubContext existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, WebPubSubContext value, JsonSerializer serializer)
        {
            serializer.Converters.Add(new HttpResponseMessageJsonConverter());
            // Request is using System.Json, use string as bridge to convert.
            var request = System.Text.Json.JsonSerializer.Serialize(value.Request);
            JObject jobj = new JObject();
            jobj.Add(new JProperty("request", JObject.Parse(request)));
            jobj.Add(new JProperty("response", JObject.FromObject(value.Response, serializer)));
            jobj.Add("errorMessage", value.ErrorMessage);
            jobj.Add("errorCode", value.ErrorCode);
            jobj.Add("isValidationRequest", value.IsValidationRequest);
            jobj.WriteTo(writer);
        }
    }
}
