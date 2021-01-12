using Microsoft.Azure.WebJobs.Host.Executors;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubTriggerDispatcher : IWebPubSubTriggerDispatcher
    {
        private Dictionary<string, WebPubSubListener> _listeners = new Dictionary<string, WebPubSubListener>();

        public void AddListener(string key, WebPubSubListener listener)
        {
            _listeners.Add(key, listener);
        }

        public async Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage req, CancellationToken token = default)
        {
            var functionName = HttpUtility.ParseQueryString(req.RequestUri.Query)["functionName"];
            if (string.IsNullOrEmpty(functionName) || !_listeners.ContainsKey(functionName))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent($"cannot find function: '{functionName}'") };
            }

            var contentType = req.Content.Headers.ContentType.MediaType;
            if (!ValidateContentType(contentType))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            if (!TryGetConnectionContext(req, out var context))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            var hubName = context.Hub;
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            // TODO: detect category of connections/messages.
            if (_listeners.TryGetValue(hubName, out var executor))
            {
                var payload = new ReadOnlySequence<byte>(await req.Content.ReadAsByteArrayAsync());
                var triggerEvent = new WebPubSubTriggerEvent
                {
                    Context = context,
                    TaskCompletionSource = tcs
                };
                await executor.Executor.TryExecuteAsync(new TriggeredFunctionData
                {
                    TriggerValue = triggerEvent
                }, token);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            // No target hub in functions
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        private bool ValidateContentType(string contentType)
        {
            return contentType == Constants.BinaryContentType || contentType == Constants.PlainTextContentType;
        }

        private bool TryGetConnectionContext(HttpRequestMessage request, out InvocationContext context)
        {
            if (!request.Headers.Contains(Constants.AsrsHubNameHeader) || !request.Headers.Contains(Constants.AsrsConnectionIdHeader))
            {
                context = null;
                return false;
            }

            context = new InvocationContext();
            context.ConnectionId = request.Headers.GetValues(Constants.AsrsConnectionIdHeader).FirstOrDefault();
            context.Hub = request.Headers.GetValues(Constants.AsrsHubNameHeader).FirstOrDefault();
            if (request.Headers.Contains(Constants.AsrsUserId))
            {
                context.UserId = request.Headers.GetValues(Constants.AsrsUserId).FirstOrDefault();
            }
            return true;
        }
    }
}
