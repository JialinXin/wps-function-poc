using Microsoft.Azure.WebPubSub.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// The <see cref="IApplicationBuilder"/> extensions for adding CORS middleware support.
    /// </summary>
    public static class WebPubSubMiddlewareExtensions
    {
        /// <summary>
        /// Adds a CORS middleware to your web application pipeline to allow cross domain requests.
        /// </summary>
        /// <param name="app">The IApplicationBuilder passed to your Configure method</param>
        /// <param name="configure">A callback to configure the <see cref="ServiceRequestBuilder"/></param>
        /// <param name="options">A callback to add the <see cref="WebPubSubValidationOptions"/></param>
        /// <returns>The original app parameter</returns>
        public static IApplicationBuilder UseWebPubSub(this IApplicationBuilder app, Action<ServiceRequestBuilder> configure, Action<WebPubSubValidationOptions> options = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (configure == null)
            {
                throw new ArgumentException(nameof(configure));
            }

            ServiceRequestBuilder builder = new ServiceRequestBuilder();
            configure(builder);

            if (options != null)
            {
                var validationOptions = new WebPubSubValidationOptions();
                options(validationOptions);
                builder.AddValidationOptions(validationOptions);
            }

            return app.UseMiddleware<WebPubSubMiddleware>(builder.Build());
        }

    }
}
