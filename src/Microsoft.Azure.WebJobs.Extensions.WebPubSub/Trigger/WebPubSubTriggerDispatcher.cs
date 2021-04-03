﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

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
                IDictionary<string, string[]> query = null;
                string[] subprotocols = null;
                ClientCertificateInfo[] certificates = null;
                string reason = null;

                if (Utilities.IsSystemConnect(context.Type))
                {
                    var content = await req.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var request = JsonConvert.DeserializeObject<ConnectEventRequest>(content);
                    claims = request.Claims;
                    subprotocols = request.Subprotocols;
                    query = request.Query;
                    certificates = request.ClientCertificates;
                }
                else if (Utilities.IsSystemDisconnected(context.Type))
                {
                    var content = await req.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var request = JsonConvert.DeserializeObject<DisconnectEventRequest>(content);
                    reason = request.Reason;
                }
                else if (Utilities.IsUserEvent(context.Type))
                {
                    if (!ValidateContentType(req.Content.Headers.ContentType.MediaType, out var dataType))
                    {
                        return new HttpResponseMessage(HttpStatusCode.BadRequest)
                        {
                            Content = new StringContent($"Message only supports text,binary,json. Current value is {req.Content.Headers.ContentType.MediaType}")
                        };
                    }

                    var payload = await req.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    message = new WebPubSubMessage(payload, dataType);
                }

                var triggerEvent = new WebPubSubTriggerEvent
                {
                    ConnectionContext = context,
                    Message = message,
                    Claims = claims,
                    Query = query,
                    ClientCertificaties = certificates,
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
                        using (token.Register(() => tcs.TrySetCanceled()))
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
                    }
                    catch (Exception ex)
                    {
                        var error = new Error(ErrorCode.ServerError, ex.Message);
                        return Utilities.BuildErrorResponse(error);
                    }
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            // No function map to current request
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        private static bool TryParseRequest(HttpRequestMessage request, out ConnectionContext context)
        {
            // ConnectionId is required in upstream request, and method is POST.
            if (!request.Headers.Contains(Constants.Headers.CloudEvents.ConnectionId)
                || request.Method != HttpMethod.Post)
            {
                context = null;
                return false;
            }

            context = new ConnectionContext();
            try
            {
                context.ConnectionId = request.Headers.GetValues(Constants.Headers.CloudEvents.ConnectionId).FirstOrDefault();
                context.Hub = request.Headers.GetValues(Constants.Headers.CloudEvents.Hub).FirstOrDefault();
                context.Type = request.Headers.GetValues(Constants.Headers.CloudEvents.Type).FirstOrDefault();
                context.Event = request.Headers.GetValues(Constants.Headers.CloudEvents.EventName).FirstOrDefault();
                context.Signature = request.Headers.GetValues(Constants.Headers.CloudEvents.Signature).FirstOrDefault();
                context.Headers = request.Headers.ToDictionary(x => x.Key, v => new StringValues(v.Value.ToArray()), StringComparer.OrdinalIgnoreCase);

                // UserId is optional, e.g. connect
                if (request.Headers.TryGetValues(Constants.Headers.CloudEvents.UserId, out var values))
                {
                    context.UserId = values.FirstOrDefault();
                }
            }
            catch (Exception)
            {
                return false;
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

        private static bool ValidateContentType(string mediaType, out MessageDataType dataType)
        {
            dataType = Utilities.GetDataType(mediaType);
            return dataType != MessageDataType.NotSupported;
        }

        private static string GetFunctionName(ConnectionContext context)
        {
            var eventType = context.Type.StartsWith(Constants.Headers.CloudEvents.TypeSystemPrefix, StringComparison.OrdinalIgnoreCase) ? 
                Constants.EventTypes.System :
                Constants.EventTypes.User;
            return $"{context.Hub}.{eventType}.{context.Event}".ToLower();
        }

        private static bool RespondToServiceAbuseCheck(HttpRequestMessage req, HashSet<string> allowedHosts, out HttpResponseMessage response)
        {
            response = new HttpResponseMessage();
            // TODO: remove Get when function core is fully supported and AWPS service is updated.
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
                return true;
            }
            return false;
        }
    }
}
