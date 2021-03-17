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
using Microsoft.Extensions.Primitives;
using System.IO;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubTriggerDispatcher : IWebPubSubTriggerDispatcher
    {
        private Dictionary<string, WebPubSubListener> _listeners = new Dictionary<string, WebPubSubListener>();

        public void AddListener(string key, WebPubSubListener listener)
        {
            if (_listeners.ContainsKey(key))
            {
                throw new ArgumentException($"Duplicated binding attribute find: {string.Join(",", key.Split('.'))}");
            }
            _listeners.Add(key, listener);
        }

        public async Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage req, string serviceHost, CancellationToken token = default)
        {
            // Handle service abuse check.
            if (RespondToServiceAbuseCheck(req, serviceHost, out var abuseResponse))
            {
                return abuseResponse;
            }

            if (!TryParseRequest(req, out var context))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            var function = GetFunctionName(context);

            if (_listeners.TryGetValue(function, out var executor))
            {
                byte[] payload = null;
                IDictionary<string, string[]> claims = null;
                string[] subprotocols = null;
                string reason = null;
                MessageDataType dataType = MessageDataType.Binary;

                if (Utilities.IsSystemConnect(context.Type))
                {
                    var content = await req.Content.ReadAsStringAsync();
                    var request = JsonConvert.DeserializeObject<ConnectEventRequest>(content);
                    claims = request.Claims;
                    subprotocols = request.Subprotocols;
                }
                else if (Utilities.IsSystemDisconnected(context.Type))
                {
                    var content = await req.Content.ReadAsStringAsync();
                    var request = JsonConvert.DeserializeObject<DisconnectEventRequest>(content);
                    reason = request.Reason;
                }
                else if (Utilities.IsUserEvent(context.Type))
                {
                    payload = await req.Content.ReadAsByteArrayAsync();
                    dataType = Utilities.GetDataType(req.Content.Headers.ContentType.MediaType);
                }

                var triggerEvent = new WebPubSubTriggerEvent
                {
                    ConnectionContext = context,
                    Payload = payload,
                    Claims = claims,
                    Reason = reason,
                    Subprotocols = subprotocols,
                    DataType = dataType,
                    TaskCompletionSource = tcs
                };
                await executor.Executor.TryExecuteAsync(new TriggeredFunctionData
                {
                    TriggerValue = triggerEvent
                }, token);

                // After function processed, return on-hold event reponses.
                if (Utilities.IsSyncMethod(context.Type))
                {
                    try
                    {
                        var response = await tcs.Task.ConfigureAwait(false);
                        if (response is MessageResponse msgResponse)
                        {
                            return Utilities.BuildResponse(msgResponse);
                        }
                        else if (response is ConnectResponse connectResponse)
                        {
                            return Utilities.BuildResponse(connectResponse);
                        }
                    }
                    catch (Exception ex)
                    {
                        var error = new Error(ErrorCode.ServerError, ex.Message);
                        return Utilities.BuildErrorResponse(error);
                    }
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            // No target hub in functions
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        private static bool TryParseRequest(HttpRequestMessage request, out ConnectionContext context)
        {
            if (!request.Headers.Contains(Constants.Headers.CloudEvents.ConnectionId))
            {
                context = null;
                return false;
            }

            context = new ConnectionContext();
            context.ConnectionId = request.Headers.GetValues(Constants.Headers.CloudEvents.ConnectionId).FirstOrDefault();
            context.Hub = request.Headers.GetValues(Constants.Headers.CloudEvents.Hub).FirstOrDefault();
            context.Type = request.Headers.GetValues(Constants.Headers.CloudEvents.Type).FirstOrDefault();
            context.Event = request.Headers.GetValues(Constants.Headers.CloudEvents.EventName).FirstOrDefault();
            context.Headers = request.Headers.ToDictionary(x => x.Key, v => new StringValues(v.Value.FirstOrDefault()), StringComparer.OrdinalIgnoreCase);
            context.UserId = request.Headers.GetValues(Constants.Headers.CloudEvents.UserId).FirstOrDefault();

            return true;
        }

        private static string GetFunctionName(ConnectionContext context)
        {
            var eventType = context.Type.StartsWith(Constants.CloudEventTypeSystemPrefix, StringComparison.OrdinalIgnoreCase) ? 
                Constants.EventTypes.System :
                Constants.EventTypes.User;
            return $"{context.Hub}.{eventType}.{context.Event}".ToLower();
        }

        private static bool RespondToServiceAbuseCheck(HttpRequestMessage req, string serviceHost, out HttpResponseMessage response)
        {
            response = new HttpResponseMessage();
            // TODO: Should be OPTIONS and use GET before function extensions update to supported version.
            if (req.Method == HttpMethod.Get)
            {
                var hosts = req.Headers.GetValues(Constants.Headers.WebHookRequestOrigin);
                if (req.RequestUri.AbsoluteUri.Contains(serviceHost))
                {
                    response.Headers.Add(Constants.Headers.WebHookAllowedOrigin, hosts);
                    return true;
                }
            }
            return false;
        }
    }
}
