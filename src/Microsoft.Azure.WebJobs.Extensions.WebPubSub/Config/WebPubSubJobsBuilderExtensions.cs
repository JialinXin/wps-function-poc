using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public static class WebPubSubJobsBuilderExtensions
    {
        public static IWebJobsBuilder AddWebPubSub(this IWebJobsBuilder builder)
        {

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<WebPubSubConfigProvider>()
                .ConfigureOptions<WebPubSubOptions>(ApplyConfiguration);
            return builder;
        }

        private static void ApplyConfiguration(IConfiguration config, WebPubSubOptions options)
        {
            if (config == null)
            {
                return;
            }

            config.Bind(options);
        }
    }
}
