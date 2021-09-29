using Azure.Messaging.WebPubSub;
using Microsoft.Azure.WebPubSub.AspNetCore;
using System;
using System.Threading.Tasks;

namespace chatapp
{
    public class TestHub1 : WebPubSubHub
    {
        private readonly WebPubSubServiceClient _client;

        public TestHub1(WebPubSubServiceClient client)
        {
            _client = client;
        }

        public override async Task<WebPubSubResponse> OnConnectAsync(ConnectEventRequest request)
        {
            await _client.SendToAllAsync($"user: {request.ConnectionContext.UserId} is connecting...");
            return new ConnectResponse
            {
                UserId = request.ConnectionContext.UserId,
            };
        }

        public override async Task<WebPubSubResponse> OnMessageAsync(MessageEventRequest request)
        {
            await _client.SendToAllAsync(request.Message.ToString());
            return new MessageResponse
            {
                Message = BinaryData.FromString("ack"),
                DataType = MessageDataType.Text
            };
        }
    }
}
