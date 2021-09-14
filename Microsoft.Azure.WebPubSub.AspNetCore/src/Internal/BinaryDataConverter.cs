﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    class BinaryDataConverter : JsonConverter<BinaryData>
    {
        public override BinaryData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return BinaryData.FromString(JsonSerializer.Deserialize<string>(ref reader));
            }

            if (TryLoadBinary(ref reader, out var bytes))
            {
                return BinaryData.FromBytes(bytes);
            }

            // string object
            return BinaryData.FromString(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, BinaryData value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }

        private static bool TryLoadBinary(ref Utf8JsonReader input, out byte[] output)
        {
            var doc = JsonDocument.ParseValue(ref input);
            if (doc.RootElement.TryGetProperty("type", out var value) && value.GetString().Equals("Buffer")
                && doc.RootElement.TryGetProperty("data", out var data))
            {
                output = JsonSerializer.Deserialize<List<byte>>(data.GetRawText()).ToArray();
                return true;
            }
            output = null;
            return false;
        }
    }
}
