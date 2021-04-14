using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using static Microsoft.Azure.WebJobs.Extensions.WebPubSub.WebPubSubTriggerBinding;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub.Tests
{
    public class JObjectTests
    {
        public static IEnumerable<object[]> MessageTestData =>
            new List<object[]>
            {
                new object[] {new Message("Hello", MessageDataType.Binary), "SGVsbG8=" },
                new object[] {new Message("Hello", MessageDataType.Text), "Hello" },
                new object[] {new Message("Hello", MessageDataType.Json), "Hello" },
                new object[] {new Message(Encoding.UTF8.GetBytes("Hello"), MessageDataType.Binary), "SGVsbG8=" },
                new object[] {new Message(Encoding.UTF8.GetBytes("Hello"), MessageDataType.Json), "Hello" },
                new object[] {new Message(Encoding.UTF8.GetBytes("Hello"), MessageDataType.Text), "Hello" },
                new object[] {new Message(new MemoryStream(Encoding.UTF8.GetBytes("Hello")), MessageDataType.Binary), "SGVsbG8=" },
                new object[] {new Message(new MemoryStream(Encoding.UTF8.GetBytes("Hello")), MessageDataType.Json), "Hello" },
                new object[] {new Message(new MemoryStream(Encoding.UTF8.GetBytes("Hello")), MessageDataType.Text), "Hello" }
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
            Assert.Equal(Operation.SendToUser, result.Operation);
            Assert.Equal("abc", result.UserId);
        }

        [Theory]
        [MemberData(nameof(MessageTestData))]
        public void TestConvertMessageToAndFromJObject(Message message, string expected)
        {
            var dataType = message.DataType;
            var wpsEvent = new WebPubSubEvent
            {
                Operation = Operation.SendToConnection,
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
            Assert.Equal(Operation.SendToConnection, result.Operation);
            Assert.Equal("abc", result.ConnectionId);
        }

        [Fact]
        public void ParseErrorResponse()
        {
            var test = @"{""code"":""unauthorized"",""errorMessage"":""not valid user.""}";
            var jObject = JObject.Parse(test);

            var result = TriggerReturnValueProvider.ConvertToResponseIfPossible(jObject);

            Assert.NotNull(result);
            Assert.Equal(typeof(ErrorResponse), result.GetType());

            var converted = (ErrorResponse)result;
            Assert.Equal(ErrorCode.Unauthorized, converted.Code);
            Assert.Equal("not valid user.", converted.ErrorMessage);
        }

        [Fact]
        public void ParseConnectResponse()
        {
            var test = @"{""userId"":""aaa""}";
            var jObject = JObject.Parse(test);

            var result = TriggerReturnValueProvider.ConvertToResponseIfPossible(jObject);

            Assert.NotNull(result);
            Assert.Equal(typeof(ConnectResponse), result.GetType());

            var converted = (ConnectResponse)result;
            Assert.Equal("aaa", converted.UserId);
        }
    }
}
