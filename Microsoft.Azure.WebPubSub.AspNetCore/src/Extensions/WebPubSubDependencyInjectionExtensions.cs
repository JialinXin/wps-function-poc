// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    public static class WebPubSubDependencyInjectionExtensions
    {
        public static IServiceCollection AddWebPubSub(this IServiceCollection services, Action<WebPubSubValidationOptions> configure)
        {
            services.Configure(configure);

            return services;
        }
    }
}
