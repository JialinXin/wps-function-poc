// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class MessageJsonConverter : JsonConverter<Message>
    {
        public override Message ReadJson(JsonReader reader, Type objectType, Message existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var message = jObject["body"].ToString();

            if (jObject["body"] != null)
            {
                return new Message(message);
            }
            return new Message(jObject.ToString());
        }
    
        public override void WriteJson(JsonWriter writer, Message value, JsonSerializer serializer)
        {;
            serializer.Serialize(writer, value);
        }
    
        //[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        //private sealed class ConvertedMessage
        //{
        //    public string Body { get; }
        //    public string DataType { get; }
        //
        //    public ConvertedMessage(Message message)
        //    {
        //        DataType = message.DataType.ToString();
        //        Body = message.DataType == MessageDataType.Binary ?
        //            Convert.ToBase64String(message.Body.ToArray()) :
        //            message.Body.ToString();
        //    }
        //
        //    public Message ToMessage()
        //    {
        //        if (DataType.Equals("binary", StringComparison.OrdinalIgnoreCase))
        //        {
        //            return new Message(Convert.FromBase64String(Body), MessageDataType.Binary);
        //        }
        //        var dataType = (MessageDataType)Enum.Parse(typeof(MessageDataType), DataType, true);
        //        return new Message(Body, dataType);
        //    }
        //}
    }
}
