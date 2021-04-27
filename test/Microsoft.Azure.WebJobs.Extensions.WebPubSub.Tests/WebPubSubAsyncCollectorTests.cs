using Azure.Core;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub.Tests
{
    public class WebPubSubAsyncCollectorTests
    {
        [TestCase]
        public async Task AddAsync_WebPubSubEvent_SendAll()
        {
            var serviceMock = new Mock<IWebPubSubService>();
            var collector = new WebPubSubAsyncCollector(serviceMock.Object);
        
            var message = "new message";
            await collector.AddAsync(new SendToAll
            {
                Message = BinaryData.FromString(message),
                DataType = MessageDataType.Text
            });
        
            //serviceMock.Verify(c => c.Client.SendToAllAsync(It.IsAny<RequestContent>()), Times.Once);
            serviceMock.VerifyNoOtherCalls();
        
            var actualData = (SendToAll)serviceMock.Invocations[0].Arguments[0];
            Assert.AreEqual(MessageDataType.Text, actualData.DataType);
            Assert.AreEqual(message, actualData.Message.ToString());
        }
    }
}
