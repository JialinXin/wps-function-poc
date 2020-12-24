using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(WebPubSubWebJobsStartup))]
namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddWebPubSub();
        }
    }
}
