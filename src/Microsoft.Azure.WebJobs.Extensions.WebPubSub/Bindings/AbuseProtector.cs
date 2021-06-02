using System.Net.Http;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class AbuseProtector
    {
        public string RequestHosts { get; internal set; }

        public bool IsValid { get; }

        public HttpResponseMessage Response { get; }

        public AbuseProtector(WebPubSubOptions options, string requestHost)
        {
            IsValid = Utilities.RespondToServiceAbuseCheck(requestHost, options.AllowedHosts, out var response);
            Response = response;
        }
    }
}
