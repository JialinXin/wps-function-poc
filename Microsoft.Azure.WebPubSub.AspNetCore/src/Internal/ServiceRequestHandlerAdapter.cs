﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    internal class ServiceRequestHandlerAdapter
    {
        private readonly WebPubSubValidationOptions _options;
        private readonly WebPubSubHub _hub;
        private readonly string _path;
        // <WebPubSubHub, Path>
        //private readonly Dictionary<WebPubSubHub, string> _hubRegistry;

        //public Dictionary<WebPubSubHub, string> HubRegistry => _hubRegistry;

        public WebPubSubHub Hub => _hub;
        public string Path => _path;

        public ServiceRequestHandlerAdapter(WebPubSubValidationOptions options, WebPubSubHub hub, string path)
        {
            _options = options;
            _hub = hub;
            _path = path;
            //_hubRegistry = hubRegistry;
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
                            var response = await _hub.OnConnectAsync(connectEventRequest).ConfigureAwait(false);
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
                            var response = await _hub.OnMessageAsync(messageRequest).ConfigureAwait(false);
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
                            await _hub.OnConnectedAsync(connectedEvent).ConfigureAwait(false);
                            return;
                        }
                    case DisconnectedEventRequest disconnectedEvent:
                        {
                            await _hub.OnDisconnectedAsync(disconnectedEvent).ConfigureAwait(false);
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
