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
            // Not Web PubSub requests.
            if (!context.Request.Headers.ContainsKey(Constants.Headers.CloudEvents.WebPubSubVersion)
                || !context.Request.Headers.TryGetValue(Constants.Headers.CloudEvents.Hub, out var hub))
            {
                await _next(context);
                return;
            }

            // Hub not registered or path not match will skip.
            if (_handler.GetHub(hub) == null || !context.Request.Path.StartsWithSegments(_handler.GetPath(hub)))
            {
                await _next(context);
                return;
            }

            await _handler.HandleRequest(context);
        }
    }
}
