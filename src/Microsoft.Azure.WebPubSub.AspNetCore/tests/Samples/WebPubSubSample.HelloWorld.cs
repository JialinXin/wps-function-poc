using Microsoft.AspNetCore.Builder;

namespace Microsoft.Azure.WebPubSub.AspNetCore.Tests.Samples
{
    class WebPubSubSample
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoint =>
            {
                endpoint.MapWebPubSubHub<SampleHub>("/eventhander");
            });
        }
    }
}
