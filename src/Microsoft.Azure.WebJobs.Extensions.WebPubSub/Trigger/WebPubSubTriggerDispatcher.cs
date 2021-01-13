﻿using Microsoft.Azure.WebJobs.Host.Executors;
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
        private Dictionary<string, WebPubSubKey> MethodsMap = new Dictionary<string, WebPubSubKey>();

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

            if (!TryGetConnectionContext(req, out var context))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            if (!ValidateContentType(context))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            var hubName = context.Hub;
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            // TODO: detect category of connections/messages.
            if (_listeners.TryGetValue(functionName, out var executor))
            {
                if (context.Category == Constants.Categories.Messages)
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
            // Connections don't have MediaType, check value if set.
            if (context.Category == Constants.Categories.Messages)
            {
                return context.MediaType == Constants.BinaryContentType || context.MediaType == Constants.PlainTextContentType;
            }
            return true;
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
            context.Category = request.Headers.GetValues(Constants.AsrsCategory).FirstOrDefault();
            context.Event = request.Headers.GetValues(Constants.AsrsEvent).FirstOrDefault();
            context.MediaType = request.Content.Headers.ContentType?.MediaType;
            context.Headers = request.Headers.ToDictionary(x => x.Key, v => v.Value.FirstOrDefault(), StringComparer.OrdinalIgnoreCase);
            
            if (request.Headers.Contains(Constants.AsrsClientQueryString))
            {
                var queries = HttpUtility.ParseQueryString(request.Headers.GetValues(Constants.AsrsClientQueryString).FirstOrDefault());
                context.Queries = queries.AllKeys.ToDictionary(x => x, v => queries[v]);
            }

            if (request.Headers.Contains(Constants.AsrsUserClaims))
            {
                var claim = request.Headers.GetValues(Constants.AsrsUserClaims).FirstOrDefault();
                context.Claims = GetClaimDictionary(claim);
            }

            if (request.Headers.Contains(Constants.AsrsUserId))
            {
                context.UserId = request.Headers.GetValues(Constants.AsrsUserId).FirstOrDefault();
            }
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

        private sealed class WebPubSubKey
        {
            public string HubName { get; set; }
            public string Category { get; set; }
            public string Event { get; set; }
        }
    }
}
