#if NETCOREAPP3_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebPubSub.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{
    public static class WebPubSubEndpointRouteBuilderExtensions
    {
        public static ComponentEndpointConventionBuilder MapWebPubSubHub<THub>(
            this IEndpointRouteBuilder endpoints,
            string path) where THub: WebPubSubHub
        {
            throw new NotImplementedException();
        }
    }
}
#endif