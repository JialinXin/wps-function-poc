using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    internal class WebPubSubMiddleware
    {
        /// <summary>
        /// Instantiates a new <see cref="CorsMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="corsService">An instance of <see cref="ICorsService"/>.</param>
        /// <param name="loggerFactory">An instance of <see cref="ILoggerFactory"/>.</param>
        public WebPubSubMiddleware(
            RequestDelegate next)
        {
        }
    }
}
