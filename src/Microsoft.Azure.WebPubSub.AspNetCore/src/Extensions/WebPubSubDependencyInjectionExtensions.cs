// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Azure.WebPubSub.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebPubSubDependencyInjectionExtensions
    {
        public static IServiceCollection AddWebPubSub(this IServiceCollection services, Action<WebPubSubOptions> configure)
        {
            services.Configure(configure);

            return services;
        }
    }
}
