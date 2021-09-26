using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    internal class WebPubSubMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ServiceRequestHandlerAdapter _handler;

        /// <summary>
        /// Instantiates a new <see cref="WebPubSubMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="corsService">An instance of <see cref="ICorsService"/>.</param>
        /// <param name="loggerFactory">An instance of <see cref="ILoggerFactory"/>.</param>
        public WebPubSubMiddleware(
            RequestDelegate next,
            ServiceRequestHandlerAdapter handler)
        {
            _next = next;
            _handler = handler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey(Constants.Headers.CloudEvents.WebPubSubVersion))
            {
                await _next(context);
                return;
            }
            
            if (!context.Request.Path.StartsWithSegments(_handler.Path))
            {
                await _next(context);
                return;
            }

            await _handler.HandleRequest(context);
        }
    }
}
