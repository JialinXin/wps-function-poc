using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebPubSub.Common;

namespace Microsoft.Azure.WebPubSub.AspNetCore.Tests.Samples
{
    public class SampleHub : WebPubSubHub
    {
        #region Snippet:WebPubSubConnectMethods
        public override ValueTask<ConnectEventResponse> OnConnectAsync(ConnectEventRequest request, CancellationToken cancellationToken)
        {
            var response = new ConnectEventResponse
            {
                UserId = request.ConnectionContext.UserId
            };
            return new ValueTask<ConnectEventResponse>(response);
        }
        #endregion

        public override ValueTask<UserEventResponse> OnMessageReceivedAsync(UserEventRequest request, CancellationToken cancellationToken)
        {
            return new ValueTask<UserEventResponse>(request.CreateResponse("ack"));
        }
    }
}
