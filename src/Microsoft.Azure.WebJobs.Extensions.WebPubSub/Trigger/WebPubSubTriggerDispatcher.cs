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
using System.Security.Cryptography;
using System.Text;

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

        public async Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage req, 
            HashSet<string> allowedHosts, 
            HashSet<string> accessKeys,
            CancellationToken token = default)
        {
            // Handle service abuse check.
            if (RespondToServiceAbuseCheck(req, allowedHosts, out var abuseResponse))
            {
                return abuseResponse;
            }

            if (!TryParseRequest(req, out var context))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            if (!ValidateSignature(context.ConnectionId, context.Signature, accessKeys))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            var function = GetFunctionName(context);

            if (_listeners.TryGetValue(function, out var executor))
            {
                WebPubSubMessage message = null;
                IDictionary<string, string[]> claims = null;
                string[] subprotocols = null;
                string reason = null;

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
                    var dataType = Utilities.GetDataType(req.Content.Headers.ContentType.MediaType);
                    if (dataType == MessageDataType.NotSupported)
                    {
                        throw new ArgumentException($"Message only supports text,binary,json. Current value is {req.Content.Headers.ContentType.MediaType}");
                    }

                    var payload = await req.Content.ReadAsByteArrayAsync();
                    message = new WebPubSubMessage(payload, dataType);
                }

                var triggerEvent = new WebPubSubTriggerEvent
                {
                    ConnectionContext = context,
                    Message = message,
                    Claims = claims,
                    Reason = reason,
                    Subprotocols = subprotocols,
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
            context.Signature = request.Headers.GetValues(Constants.Headers.CloudEvents.Signature).FirstOrDefault();
            context.Headers = request.Headers.ToDictionary(x => x.Key, v => new StringValues(v.Value.FirstOrDefault()), StringComparer.OrdinalIgnoreCase);

            // UserId is optional.
            if (request.Headers.TryGetValues(Constants.Headers.CloudEvents.UserId, out var values))
            {
                context.UserId = values.FirstOrDefault();
            }

            return true;
        }

        private bool ValidateSignature(string connectionId, string signature, HashSet<string> accessKeys)
        {
            foreach (var accessKey in accessKeys)
            {
                var signatures = Utilities.GetSignatureList(signature);
                if (signatures == null)
                {
                    continue;
                }
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(accessKey)))
                {
                    var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(connectionId));
                    var hash = "sha256=" + BitConverter.ToString(hashBytes).Replace("-", "");
                    if (signatures.Contains(hash, StringComparer.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return false;
        }

        private static string GetFunctionName(ConnectionContext context)
        {
            var eventType = context.Type.StartsWith(Constants.CloudEventTypeSystemPrefix, StringComparison.OrdinalIgnoreCase) ? 
                Constants.EventTypes.System :
                Constants.EventTypes.User;
            return $"{context.Hub}.{eventType}.{context.Event}".ToLower();
        }

        private static bool RespondToServiceAbuseCheck(HttpRequestMessage req, HashSet<string> allowedHosts, out HttpResponseMessage response)
        {
            response = new HttpResponseMessage();
            if (req.Method == HttpMethod.Options || req.Method == HttpMethod.Get)
            {
                var hosts = req.Headers.GetValues(Constants.Headers.WebHookRequestOrigin);
                if (hosts != null && hosts.Count() > 0)
                {
                    foreach (var item in allowedHosts)
                    {
                        if (hosts.Contains(item))
                        {
                            response.Headers.Add(Constants.Headers.WebHookAllowedOrigin, hosts);
                            return true;
                        }
                    }
                    response.StatusCode = HttpStatusCode.BadRequest;
                }
            }
            return false;
        }
    }
}
