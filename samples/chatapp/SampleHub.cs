using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.WebPubSub;
using Microsoft.Azure.WebPubSub.AspNetCore;
using Microsoft.Azure.WebPubSub.Common;

namespace chatapp
{
    public class SampleHub : WebPubSubHub
    {
        private readonly WebPubSubServiceClient _client;

        public SampleHub(WebPubSubServiceClient client)
        {
            _client = client;
        }

        //public override async ValueTask<WebPubSubEventResponse> OnConnectAsync(ConnectEventRequest request, CancellationToken cancellationToken)
        //{
        //    await _client.SendToAllAsync($"user: {request.ConnectionContext.UserId} is connecting...");
        //    return request.CreateResponse(request.ConnectionContext.UserId, new List<string> { "group1" }, null, new List<string> { "editor" });
        //}

        public override async Task OnConnectedAsync(ConnectedEventRequest request)
        {
            await _client.SendToAllAsync($"user: {request.ConnectionContext.UserId} is connected");
        }

        public override async ValueTask<UserEventResponse> OnMessageReceivedAsync(UserEventRequest request, CancellationToken cancellationToken)
        {
            await _client.SendToAllAsync(request.Data.ToString());
            return request.CreateResponse("ack");
        }
    }
}
