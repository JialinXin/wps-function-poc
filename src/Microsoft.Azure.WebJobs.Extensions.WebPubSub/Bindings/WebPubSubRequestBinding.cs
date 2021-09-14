// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebPubSub.AspNetCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubRequestBinding : BindingBase<WebPubSubRequestAttribute>
    {
        private const string HttpRequestName = "$request";
        private readonly Type _userType;
        private readonly WebPubSubOptions _options;

        public WebPubSubRequestBinding(
            BindingProviderContext context,
            IConfiguration configuration,
            INameResolver nameResolver,
            WebPubSubOptions options) : base(context, configuration, nameResolver)
        {
            _userType = context.Parameter.ParameterType;
            _options = options;
        }

        protected async override Task<IValueProvider> BuildAsync(WebPubSubRequestAttribute attrResolved, IReadOnlyDictionary<string, object> bindingData)
        {
            if (bindingData == null)
            {
                throw new ArgumentNullException(nameof(bindingData));
            }
            bindingData.TryGetValue(HttpRequestName, out var httpRequest);
            var request = httpRequest as HttpRequest;

            var httpContext = request?.HttpContext;

            if (httpContext == null)
            {
                return new WebPubSubRequestValueProvider(new WebPubSubRequest(new InvalidRequest($"HttpContext is null."), HttpStatusCode.BadRequest), _userType);
            }

            // Build abuse response
            if (httpContext.Request.IsValidationRequest(out var requestHosts))
            {
                var abuseResponse = Utilities.RespondToServiceAbuseCheck(requestHosts, attrResolved.ValidationOptions);
                var abuseRequest = new WebPubSubRequest(new ValidationRequest(abuseResponse.StatusCode == HttpStatusCode.OK, requestHosts), abuseResponse);
                return new WebPubSubRequestValueProvider(abuseRequest, _userType);
            }

            // Build service request context
            if (!request.TryParseCloudEvents(out var connectionContext))
            {
                // Not valid WebPubSubRequest
                return new WebPubSubRequestValueProvider(new WebPubSubRequest(new InvalidRequest(Constants.ErrorMessages.NotValidWebPubSubRequest), HttpStatusCode.BadRequest), _userType);
            }

            // Signature check
            if (!connectionContext.IsValidSignature(attrResolved.ValidationOptions))
            {
                return new WebPubSubRequestValueProvider(new WebPubSubRequest(new InvalidRequest(Constants.ErrorMessages.SignatureValidationFailed), HttpStatusCode.Unauthorized), _userType);
            }

            WebPubSubRequest wpsRequest;
            var requestType = Utilities.GetRequestType(connectionContext.EventType, connectionContext.EventName);

            switch (requestType)
            {
                case RequestType.Connect:
                    {
                        var content = await ReadString(request.Body).ConfigureAwait(false);
                        var eventRequest = JsonConvert.DeserializeObject<ConnectEventRequest>(content);
                        eventRequest.ConnectionContext = connectionContext;
                        wpsRequest = new WebPubSubRequest(eventRequest);
                    }
                    break;
                case RequestType.Connected:
                    {
                        wpsRequest = new WebPubSubRequest(new ConnectedEventRequest(connectionContext));
                    }
                    break;
                case RequestType.Disconnected:
                    {
                        var content = await ReadString(request.Body).ConfigureAwait(false);
                        var eventRequest = JsonConvert.DeserializeObject<DisconnectedEventRequest>(content);
                        eventRequest.ConnectionContext = connectionContext;
                        wpsRequest = new WebPubSubRequest(eventRequest);
                    }
                    break;
                case RequestType.User:
                    {
                        var contentType = MediaTypeHeaderValue.Parse(request.ContentType);
                        if (!Utilities.ValidateMediaType(contentType.MediaType, out var dataType))
                        {
                            var invalidRequest = new InvalidRequest($"{Constants.ErrorMessages.NotSupportedDataType}{request.ContentType}");
                            return new WebPubSubRequestValueProvider(new WebPubSubRequest(invalidRequest, HttpStatusCode.BadRequest), _userType);
                        }
                        var payload = ReadBytes(request.Body);
                        var eventRequest = new MessageEventRequest(connectionContext, BinaryData.FromBytes(payload), dataType);
                        wpsRequest = new WebPubSubRequest(eventRequest);
                    }
                    break;
                default:
                    wpsRequest = new WebPubSubRequest(new InvalidRequest("Unknown request."));
                    break;
            }

            return new WebPubSubRequestValueProvider(wpsRequest, _userType);
        }

        private static async Task<string> ReadString(Stream body)
        {
            string payload;
            using var ms = new MemoryStream();
            await body.CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;
            body.Position = 0;
            using var reader = new StreamReader(ms);
            payload = await reader.ReadToEndAsync().ConfigureAwait(false);
            return payload;
        }

        private static byte[] ReadBytes(Stream body)
        {
            using var ms = new MemoryStream();
            body.CopyTo(ms);
            return ms.ToArray();
        }

        private static string GetHeaderValueOrDefault(IHeaderDictionary header, string key)
        {
            return header.TryGetValue(key, out var value) ? value[0] : null;
        }
    }
}
