using System.Threading.Tasks;

namespace Microsoft.Azure.WebPubSub.AspNetCore.Tests.Samples
{
    public class SampleHub : WebPubSubHub
    {
        public override Task<WebPubSubResponse> OnConnectAsync(ConnectEventRequest request)
        {
            var response = new ConnectResponse
            {
                UserId = request.ConnectionContext.UserId
            };
            return Task.FromResult<WebPubSubResponse>(response);
        }

        public override Task<WebPubSubResponse> OnMessageAsync(MessageEventRequest request)
        {
            var response = new MessageResponse("ack");
            return Task.FromResult<WebPubSubResponse>(response);
        }
    }
}
