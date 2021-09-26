using Microsoft.Azure.WebPubSub.AspNetCore;
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
        /// <returns>The original app parameter</returns>
        public static IApplicationBuilder UseWebPubSub(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<WebPubSubMiddleware>();
        }

        /// <summary>
        /// Adds a CORS middleware to your web application pipeline to allow cross domain requests.
        /// </summary>
        /// <param name="app">The IApplicationBuilder passed to your Configure method</param>
        /// <returns>The original app parameter</returns>
        public static IApplicationBuilder UseWebPubSub(this IApplicationBuilder app, Action<WebPubSubValidationOptions> options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var builder = new WebPubSubRequestHandlerBuilder();

            if (options != null)
            {
                builder.AddValidationOptions(options.Invoke());
            }

            return app.UseMiddleware<WebPubSubMiddleware>();
        }

    }
}
