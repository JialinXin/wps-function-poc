using Azure.Messaging.WebPubSub;
using Microsoft.Azure.WebPubSub.AspNetCore;
using Microsoft.Azure.WebPubSub.Common;
using System.Threading;
using System.Threading.Tasks;

namespace chatapp
{
    public class SampleHub1 : WebPubSubHub
    {
        private readonly WebPubSubServiceClient _client;

        public SampleHub1(WebPubSubServiceClient client)
        {
            _client = client;
        }

        public override async ValueTask<ConnectEventResponse> OnConnectAsync(ConnectEventRequest request, CancellationToken cancellationToken)
        {
            await _client.SendToAllAsync($"user: {request.ConnectionContext.UserId} is connecting...");
            return request.CreateResponse(request.ConnectionContext.UserId, null, null, null);
        }

        public override async ValueTask<UserEventResponse> OnMessageReceivedAsync(UserEventRequest request, CancellationToken cancellationToken)
        {
            await _client.SendToAllAsync(request.Message.ToString());
            return request.CreateResponse("ack");
        }
    }
}
