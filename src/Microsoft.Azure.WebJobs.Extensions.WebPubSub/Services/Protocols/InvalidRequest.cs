using System.Net;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class InvalidRequest : ServiceRequest
    {
        public override string Name => nameof(InvalidRequest);

        public InvalidRequest(HttpStatusCode statusCode, string message = null)
            : base(statusCode, message)
        {
        }
    }
}
