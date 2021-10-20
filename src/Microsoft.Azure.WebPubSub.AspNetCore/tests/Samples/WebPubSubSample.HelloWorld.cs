using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.WebPubSub.AspNetCore.Tests.Samples
{
    class WebPubSubSample
    {
        #region Snippet:WebPubSubDependencyInjection
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebPubSub(o =>
            {
                o.ValidationOptions.Add("<connection-string>");
            });
        }
        #endregion

        #region Snippet:WebPubSubMapHub
        public void Configure(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoint =>
            {
                endpoint.MapWebPubSubHub<SampleHub>("/eventhander");
            });
        }
        #endregion
    }
}
