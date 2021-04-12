using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub.Tests
{
    public class JObjectTests
    {
        public static IEnumerable<object[]> MessageTestData =>
            new List<object[]>
            {
                new object[] {new WebPubSubMessage("Hello", MessageDataType.Binary), "SGVsbG8=" },
                new object[] {new WebPubSubMessage("Hello", MessageDataType.Text), "Hello" },
                new object[] {new WebPubSubMessage("Hello", MessageDataType.Json), "Hello" },
                new object[] {new WebPubSubMessage(Encoding.UTF8.GetBytes("Hello"), MessageDataType.Binary), "SGVsbG8=" },
                new object[] {new WebPubSubMessage(Encoding.UTF8.GetBytes("Hello"), MessageDataType.Json), "Hello" },
                new object[] {new WebPubSubMessage(Encoding.UTF8.GetBytes("Hello"), MessageDataType.Text), "Hello" },
                new object[] {new WebPubSubMessage(new MemoryStream(Encoding.UTF8.GetBytes("Hello")), MessageDataType.Binary), "SGVsbG8=" },
                new object[] {new WebPubSubMessage(new MemoryStream(Encoding.UTF8.GetBytes("Hello")), MessageDataType.Json), "Hello" },
                new object[] {new WebPubSubMessage(new MemoryStream(Encoding.UTF8.GetBytes("Hello")), MessageDataType.Text), "Hello" }
            };

        [Fact]
        public void TestConvertFromJObject()
        {
            var wpsEvent = @"{
                ""operation"":""sendToUser"",
                ""userId"": ""abc"",
                ""message"": {
                    ""body"": ""test"",
                    ""dataType"": ""text""
                }}";
            
            var jsevent = JObject.Parse(wpsEvent);
            
            var result = jsevent.ToObject<WebPubSubEvent>();

            Assert.Equal("test", result.Message.Body.ToString());
            Assert.Equal(MessageDataType.Text, result.Message.DataType);
            Assert.Equal(WebPubSubOperation.SendToUser, result.Operation);
            Assert.Equal("abc", result.UserId);
        }

        [Theory]
        [MemberData(nameof(MessageTestData))]
        public void TestConvertMessageToAndFromJObject(WebPubSubMessage message, string expected)
        {
            var dataType = message.DataType;
            var wpsEvent = new WebPubSubEvent
            {
                Operation = WebPubSubOperation.SendToConnection,
                ConnectionId = "abc",
                Message = message
            };

            var jsObject = JObject.FromObject(wpsEvent);

            Assert.Equal("sendToConnection", jsObject["operation"].ToString());
            Assert.Equal("abc", jsObject["connectionId"].ToString());
            Assert.Equal(expected, jsObject["message"]["body"].ToString());

            var result = jsObject.ToObject<WebPubSubEvent>();

            Assert.Equal(expected, result.Message.Body.ToString());
            Assert.Equal(dataType, result.Message.DataType);
            Assert.Equal(WebPubSubOperation.SendToConnection, result.Operation);
            Assert.Equal("abc", result.ConnectionId);
        }

        [Fact]
        public void TestBinaryData()
        {
            var test = BinaryData.FromString("test");

            var aaa = test.ToString();
        }
    }
}
