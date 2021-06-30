namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class PreflightRequest : ServiceRequest
    {
        public override string Name => nameof(PreflightRequest);

        public PreflightRequest(bool valid)
            :base(true, valid)
        {
        }
    }
}
