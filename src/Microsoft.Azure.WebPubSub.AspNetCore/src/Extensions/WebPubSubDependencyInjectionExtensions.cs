// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#if NETCOREAPP3_0_OR_GREATER
using System;
using Microsoft.Azure.WebPubSub.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebPubSubDependencyInjectionExtensions
    {
        public static IServiceCollection AddWebPubSub(this IServiceCollection services, Action<WebPubSubOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            //var options = new WebPubSubOptions();
            //configure.Invoke(options);
            services.Configure(configure);

            services.AddWebPubSub();

            return services;
        }

        public static IServiceCollection AddWebPubSub(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddSingleton<ServiceRequestHandlerAdapter>()
                .AddSingleton<WebPubSubMarkerService>();
        }
    }
}
#endif
