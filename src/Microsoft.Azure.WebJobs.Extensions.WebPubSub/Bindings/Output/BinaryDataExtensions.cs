// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal static class BinaryDataExtensions
    {
        private static readonly UTF8Encoding encoding = new UTF8Encoding(false, true);

        public static object Convert(this BinaryData message, Type targetType)
        {
            if (targetType == typeof(JObject))
            {
                return JObject.FromObject(message.ToArray());
            }

            if (targetType == typeof(Stream))
            {
                return message.ToStream();
            }

            if (targetType == typeof(byte[]))
            {
                return message.ToArray();
            }

            if (targetType == typeof(string))
            {
                return message.ToString();
            }

            if (targetType == typeof(BinaryData))
            {
                return message;
            }

            return null;
        }

        public static string ToValidUTF8String(this BinaryData binaryData)
        {
            if (MemoryMarshal.TryGetArray(binaryData.ToMemory(), out ArraySegment<byte> data))
            {
                return encoding.GetString(data.Array, data.Offset, data.Count);
            }
            return encoding.GetString(binaryData.ToArray());
        }
    }
}
