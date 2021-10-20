#if NETCOREAPP3_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebPubSub.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class WebPubSubEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapWebPubSubHub<THub>(
            this IEndpointRouteBuilder endpoints,
            string path) where THub: WebPubSubHub
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            var marker = endpoints.ServiceProvider.GetService<WebPubSubMarkerService>();

            if (marker == null)
            {
                throw new InvalidOperationException(
                    "Unable to find the required services. Please add all the required services by calling " +
                    "'IServiceCollection.AddWebPubSub' inside the call to 'ConfigureServices(...)' in the application startup code.");
            }

            var adaptor = endpoints.ServiceProvider.GetService<ServiceRequestHandlerAdapter>();
            adaptor.RegisterHub<THub>();

            var app = endpoints.CreateApplicationBuilder();
            app.UseMiddleware<WebPubSubMiddleware>();

            return endpoints.Map(path, app.Build());
        }
    }
}
#endif