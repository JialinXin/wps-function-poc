using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    internal class WebPubSubMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ServiceRequestHandlerAdapter _handler;

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
            
            if (!context.Request.Headers.TryGetValue(Constants.Headers.CloudEvents.Hub, out var hub)
                && !hub.SingleOrDefault().Equals(nameof(_handler.Hub), System.StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            await _handler.HandleRequest(context);
        }
    }
}
