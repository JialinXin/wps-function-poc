﻿using System.Threading.Tasks;

namespace Microsoft.Azure.WebPubSub.AspNetCore.Tests.Samples
{
    public class SampleHub : WebPubSubHub
    {
        public override Task<ServiceResponse> OnConnectAsync(ConnectEventRequest request)
        {
            var response = new ConnectResponse
            {
                UserId = request.ConnectionContext.UserId
            };
            return Task.FromResult<ServiceResponse>(response);
        }

        public override Task<ServiceResponse> OnMessageAsync(MessageEventRequest request)
        {
            var response = new MessageResponse("ack");
            return Task.FromResult<ServiceResponse>(response);
        }
    }
}
