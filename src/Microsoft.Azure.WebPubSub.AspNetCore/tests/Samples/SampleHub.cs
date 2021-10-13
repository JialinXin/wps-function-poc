﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebPubSub.Common;

namespace Microsoft.Azure.WebPubSub.AspNetCore.Tests.Samples
{
    public class SampleHub : WebPubSubHub
    {
        public override ValueTask<WebPubSubEventResponse> OnConnectAsync(ConnectEventRequest request, CancellationToken cancellationToken)
        {
            var response = new ConnectEventResponse
            {
                UserId = request.ConnectionContext.UserId
            };
            return new ValueTask<WebPubSubEventResponse>(response);
        }

        public override ValueTask<WebPubSubEventResponse> OnMessageReceivedAsync(UserEventRequest request, CancellationToken cancellationToken)
        {
            var response = new UserEventResponse("ack");
            return new ValueTask<WebPubSubEventResponse>(response);
        }
    }
}