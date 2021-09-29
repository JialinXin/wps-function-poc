// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    internal class ServiceRequestHandlerAdapter
    {
        private readonly WebPubSubValidationOptions _options;

        // <hubName, <Hub,path>>
        private readonly Dictionary<string, HubRegistry> _hubRegistry = new(StringComparer.OrdinalIgnoreCase);

        // for tests.
        internal ServiceRequestHandlerAdapter(WebPubSubValidationOptions options, WebPubSubHub hub, string path)
        {
            _options = options;
            _hubRegistry.Add(hub.GetType().Name, new HubRegistry(hub, path));
        }

        public ServiceRequestHandlerAdapter(WebPubSubValidationOptions options, Dictionary<string, HubRegistry> hubRegistry)
        {
            _options = options;
            _hubRegistry = hubRegistry;
        }

        public string GetPath(string hubName)
        {
            if (_hubRegistry.TryGetValue(hubName, out var registry))
            {
                return registry.Path;
            }
            return null;
        }

        public WebPubSubHub GetHub(string hubName)
        {
            if (_hubRegistry.TryGetValue(hubName, out var registry))
            {
                return registry.Hub;
            }
            return null;
        }

        public async Task HandleRequest(HttpContext context)
        {
            HttpRequest request = context.Request;

            if (context == null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("HttpContext is null").ConfigureAwait(false);
                return;
            }

            // Should check in middleware to skip not match calls.
            // And keep here for internal reference lib robustness amd return as 400BadRequest.
            #region WebPubSubRequest Check
            // Not Web PubSub request.
            if (!context.Request.Headers.ContainsKey(Constants.Headers.CloudEvents.WebPubSubVersion)
                || !context.Request.Headers.TryGetValue(Constants.Headers.CloudEvents.Hub, out var hubName))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Not Web PubSub request.").ConfigureAwait(false);
                return;
            }

            // Hub not registered or path not match will skip.
            var hub = GetHub(hubName);
            if (hub == null || !context.Request.Path.StartsWithSegments(GetPath(hubName)))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Hub is not registered.").ConfigureAwait(false);
                return;
            }
            #endregion

            try
            {
                var serviceRequest = await request.ParseServiceRequest(_options);

                switch (serviceRequest)
                {
                    case ValidationRequest validationRequest:
                        {
                            if (validationRequest.IsValid)
                            {
                                context.Response.Headers.Add(Constants.Headers.WebHookAllowedOrigin, "*");
                                return;
                            }
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            await context.Response.WriteAsync("Abuse Protection validation failed.").ConfigureAwait(false);
                            return;
                        }
                    case ConnectEventRequest connectEventRequest:
                        {
                            var response = await hub.OnConnectAsync(connectEventRequest).ConfigureAwait(false);
                            if (response is ErrorResponse error)
                            {
                                context.Response.StatusCode = ConvertToStatusCode(error.Code);
                                context.Response.ContentType = Constants.ContentTypes.PlainTextContentType;
                                await context.Response.WriteAsync(error.ErrorMessage).ConfigureAwait(false);
                                return;
                            }
                            else if (response is ConnectResponse connectResponse)
                            {
                                SetConnectionState(ref context, connectEventRequest.ConnectionContext, connectResponse.States);
                                await context.Response.WriteAsync(JsonSerializer.Serialize(connectResponse)).ConfigureAwait(false);
                                return;
                            }
                            // other response is invalid, igonre.
                            return;
                        }
                    case MessageEventRequest messageRequest:
                        {
                            var response = await hub.OnMessageAsync(messageRequest).ConfigureAwait(false);
                            if (response is ErrorResponse error)
                            {
                                context.Response.StatusCode = ConvertToStatusCode(error.Code);
                                context.Response.ContentType = Constants.ContentTypes.PlainTextContentType;
                                await context.Response.WriteAsync(error.ErrorMessage).ConfigureAwait(false);
                                return;
                            }
                            else if (response is MessageResponse msgResponse)
                            {
                                SetConnectionState(ref context, messageRequest.ConnectionContext, msgResponse.States);
                                context.Response.ContentType = ConvertToContentType(msgResponse.DataType);
                                var payload = msgResponse.Message.ToArray();
                                await context.Response.Body.WriteAsync(payload, 0, payload.Length).ConfigureAwait(false);
                                return;
                            }
                            // other response is invalid, igonre.
                            return;
                        }
                    case ConnectedEventRequest connectedEvent:
                        {
                            await hub.OnConnectedAsync(connectedEvent).ConfigureAwait(false);
                            return;
                        }
                    case DisconnectedEventRequest disconnectedEvent:
                        {
                            await hub.OnDisconnectedAsync(disconnectedEvent).ConfigureAwait(false);
                            return;
                        }
                    default:
                        return;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync(ex.Message).ConfigureAwait(false);
                return;
            }
        }

        private static void SetConnectionState(ref HttpContext context, ConnectionContext connectionContext, Dictionary<string, object> newStates)
        {
            var updatedStates = connectionContext.UpdateStates(newStates);
            if (updatedStates != null)
            {
                context.Response.Headers.Add(Constants.Headers.CloudEvents.State, updatedStates.EncodeConnectionStates());
            }
        }

        private static int ConvertToStatusCode(WebPubSubErrorCode errorCode) =>
            errorCode switch
            {
                WebPubSubErrorCode.UserError => StatusCodes.Status400BadRequest,
                WebPubSubErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
                // default and server error returns 500
                _ => StatusCodes.Status500InternalServerError
            };

        private static string ConvertToContentType(MessageDataType dataType) =>
            dataType switch
            {
                MessageDataType.Text => $"{Constants.ContentTypes.PlainTextContentType}; {Constants.ContentTypes.CharsetUTF8}",
                MessageDataType.Json => $"{Constants.ContentTypes.JsonContentType}; {Constants.ContentTypes.CharsetUTF8}",
                _ => Constants.ContentTypes.BinaryContentType
            };
    }
}
