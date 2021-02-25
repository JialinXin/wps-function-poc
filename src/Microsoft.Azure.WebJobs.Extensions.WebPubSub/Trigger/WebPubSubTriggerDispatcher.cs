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

using Newtonsoft.Json;

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
            if (!TryGetConnectionContext(req, out var context))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            if (!ValidateContentType(context))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(context.Function) || !_listeners.ContainsKey(context.Function))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent($"cannot find function: '{context.Function}'") };
            }

            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (_listeners.TryGetValue(context.Function, out var executor))
            {
                if (context.Type.StartsWith(Constants.CloudEventTypeUserPrefix))
                {
                    context.Payload = new ReadOnlyMemory<byte>(await req.Content.ReadAsByteArrayAsync());
                }

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

        private bool ValidateContentType(InvocationContext context)
        {
            // Check user message content type
            if (context.Type.StartsWith(Constants.CloudEventTypeUserPrefix))
            {
                return context.MediaType == Constants.ContentTypes.BinaryContentType || 
                    context.MediaType == Constants.ContentTypes.JsonContentType ||
                    context.MediaType == Constants.ContentTypes.PlainTextContentType;
            }
            return true;
        }

        private bool TryGetConnectionContext(HttpRequestMessage request, out InvocationContext context)
        {
            if (!request.Headers.Contains(Constants.CloudEvents.Headers.Hub) || !request.Headers.Contains(Constants.CloudEvents.Headers.ConnectionId))
            {
                context = null;
                return false;
            }

            context = new InvocationContext();
            context.ConnectionId = request.Headers.GetValues(Constants.CloudEvents.Headers.ConnectionId).FirstOrDefault();
            context.Hub = request.Headers.GetValues(Constants.CloudEvents.Headers.Hub).FirstOrDefault();
            context.Type = request.Headers.GetValues(Constants.CloudEvents.Headers.Type).FirstOrDefault();
            //context.Category = request.Headers.GetValues(Constants.CloudEvents.Headers).FirstOrDefault();
            context.Event = request.Headers.GetValues(Constants.CloudEvents.Headers.EventName).FirstOrDefault();
            context.MediaType = request.Content.Headers.ContentType?.MediaType;
            context.Headers = request.Headers.ToDictionary(x => x.Key, v => v.Value.FirstOrDefault(), StringComparer.OrdinalIgnoreCase);
            
            //if (request.Headers.Contains(Constants.AsrsClientQueryString))
            //{
            //    var queries = HttpUtility.ParseQueryString(request.Headers.GetValues(Constants.AsrsClientQueryString).FirstOrDefault());
            //    context.Queries = queries.AllKeys.ToDictionary(x => x, v => queries[v]);
            //}

            //if (request.Headers.Contains(Constants.AsrsUserClaims))
            //{
            //    var claim = request.Headers.GetValues(Constants.AsrsUserClaims).FirstOrDefault();
            //    context.Claims = GetClaimDictionary(claim);
            //}

            if (request.Headers.Contains(Constants.CloudEvents.Headers.UserId))
            {
                context.UserId = request.Headers.GetValues(Constants.CloudEvents.Headers.UserId).FirstOrDefault();
            }

            //var content = request.Content.ReadAsStringAsync().Result;
            //var body = JsonConvert.DeserializeObject<RequestContent>(content);
            // TODO: exlude _defaulthub
            context.Function =  context.Hub == Constants.DefaultHub ? $"{context.Event}".ToLower() : $"{context.Hub}-{context.Event}".ToLower();
            return true;
        }

        private static IDictionary<string, string> GetClaimDictionary(string claims)
        {
            if (string.IsNullOrEmpty(claims))
            {
                return default;
            }

            // The claim string looks like "a: v, b: v"
            return claims.Split(new char[] { Constants.HeaderSeparator }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split(new string[] { Constants.ClaimsSeparator }, StringSplitOptions.RemoveEmptyEntries)).Where(l => l.Length == 2)
                .ToDictionary(p => p[0].Trim(), p => p[1].Trim());
        }
    }
}
