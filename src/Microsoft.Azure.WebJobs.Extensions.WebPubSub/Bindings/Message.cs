// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    //[JsonConverter(typeof(MessageJsonConverter))]
    public class Message
    {
        /// <summary>
        /// Web PubSub data message
        /// </summary>
        public BinaryData Body { get; }

        public Message(Stream message)
        {
            Body = BinaryData.FromStream(message);
        }

        public Message(string message)
        {
            Body = BinaryData.FromString(message);
        }

        public Message(byte[] message)
        {
            Body = BinaryData.FromBytes(message);
        }

        ///// <summary>
        ///// DataType of the message.
        ///// </summary>
        //public MessageDataType DataType { get; }
        //
        ///// <summary>
        ///// Constructor for string/json typed message
        ///// </summary>
        //[JsonConstructor]
        //public Message(string message, MessageDataType dataType = MessageDataType.Text)
        //{
        //    Body = BinaryData.FromString(message);
        //    DataType = dataType;
        //}
        //
        ///// <summary>
        ///// Constructor for stream type message
        ///// </summary>
        //public Message(Stream message, MessageDataType dataType)
        //{
        //    Body = BinaryData.FromStream(message);
        //    DataType = dataType;
        //}
        //
        ///// <summary>
        ///// Constructor for binary type message
        ///// </summary>
        //public Message(byte[] message, MessageDataType dataType)
        //{
        //    Body = BinaryData.FromBytes(message);
        //    DataType = dataType;
        //}
    }

    internal static class MessageExtensions
    {
        public static object Convert(this Message message, Type targetType)
        {
            if (targetType.GetType() == typeof(JObject))
            {
                return JObject.FromObject(message.Body.ToArray());
            }

            if (targetType.GetType() == typeof(string))
            {
                return message.Body.ToString();
            }

            if (targetType.GetType() == typeof(Stream))
            {
                return message.Body.ToStream();
            }

            if (targetType.GetType() == typeof(byte[]))
            {
                return message.Body.ToArray();
            }

            return null;
        }
    }
}
