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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubRequestBinding : BindingBase<WebPubSubRequestAttribute>
    {
        private const string HttpRequestName = "$request";
        private readonly Type _userType;
        private readonly INameResolver _nameResolver;
        private readonly WebPubSubOptions _options;

        public WebPubSubRequestBinding(
            BindingProviderContext context,
            IConfiguration configuration,
            INameResolver nameResolver,
            WebPubSubOptions options) : base (context, configuration, nameResolver)
        {
            _userType = context.Parameter.ParameterType;
            _options = options;
            _nameResolver = nameResolver;
        }

        protected async override Task<IValueProvider> BuildAsync(WebPubSubRequestAttribute attrResolved, IReadOnlyDictionary<string, object> bindingData)
        {
            if (bindingData == null)
            {
                throw new ArgumentNullException(nameof(bindingData));
            }
            bindingData.TryGetValue(HttpRequestName, out var requestObj);
            var request = requestObj as HttpRequest;

            var httpContext = request?.HttpContext;

            // Build abuse response
            if (httpContext.Request.Headers.TryGetValue(Constants.Headers.WebHookRequestOrigin, out var requestHosts) &&
                Utilities.RespondToServiceAbuseCheck(httpContext.Request.Method, requestHosts, _options.AllowedHosts, out var abuseResponse))
            {
                var abuseRequest = new WebPubSubRequest(WebPubSubRequestStatus.RequestValid, abuseResponse);
                abuseRequest.IsAbuseRequest = true;
                return new WebPubSubRequestValueProvider(abuseRequest, _userType, string.Empty);
            }

            // Build service reuest context
            if (!TryParseRequest(request, out var connectionContext))
            {
                return new WebPubSubRequestValueProvider(new WebPubSubRequest(WebPubSubRequestStatus.FormatInvalid, HttpStatusCode.BadRequest), _userType, string.Empty);
            }

            // Signature check
            if (!Utilities.ValidateSignature(connectionContext.ConnectionId, connectionContext.Signature, _options.AccessKeys))
            {
                return new WebPubSubRequestValueProvider(new WebPubSubRequest(WebPubSubRequestStatus.SignatureInvalid, HttpStatusCode.Unauthorized), _userType, string.Empty);
            }

            var wpsRequest = new WebPubSubRequest(WebPubSubRequestStatus.RequestValid);
            var requestType = Utilities.GetRequestType(connectionContext.EventType, connectionContext.EventName);
            switch (requestType)
            {
                case RequestType.Connect:
                    using (var sr = new StreamReader(request.Body))
                    {
                        var content = await sr.ReadToEndAsync().ConfigureAwait(false);
                        wpsRequest.Request = JsonConvert.DeserializeObject<ConnectEventRequest>(content);
                        sr.Close();
                    }
                    break;
                case RequestType.Disconnect:
                    using (var sr = new StreamReader(request.Body))
                    {
                        var content = await sr.ReadToEndAsync().ConfigureAwait(false);
                        wpsRequest.Request = JsonConvert.DeserializeObject<DisconnectEventRequest>(content);
                    }
                    break;
                case RequestType.User:
                    var contentType = MediaTypeHeaderValue.Parse(request.ContentType);
                    if (!Utilities.ValidateContentType(contentType.MediaType, out var dataType))
                    {
                        return new WebPubSubRequestValueProvider(new WebPubSubRequest(WebPubSubRequestStatus.ContentTypeInvalid, HttpStatusCode.BadRequest), _userType, string.Empty);
                    }
                    wpsRequest.Request = new MessageEventRequest
                    {
                        Message = BinaryData.FromStream(request.Body),
                        DataType = dataType
                    };
                    break;
                default:
                    break;
            }
            return new WebPubSubRequestValueProvider(wpsRequest, _userType, string.Empty);
        }

        private static bool TryParseRequest(HttpRequest request, out ConnectionContext context)
        {
            // ConnectionId is required in upstream request, and method is POST.
            if (!request.Headers.ContainsKey(Constants.Headers.CloudEvents.ConnectionId)
                || !request.Method.Equals("post", StringComparison.OrdinalIgnoreCase))
            {
                context = null;
                return false;
            }

            context = new ConnectionContext();
            try
            {
                context.ConnectionId = GetHeaderValueOrDefault(request.Headers, Constants.Headers.CloudEvents.ConnectionId);
                context.Hub = GetHeaderValueOrDefault(request.Headers, Constants.Headers.CloudEvents.Hub);
                context.EventType = Utilities.GetEventType(GetHeaderValueOrDefault(request.Headers, Constants.Headers.CloudEvents.Type));
                context.EventName = GetHeaderValueOrDefault(request.Headers, Constants.Headers.CloudEvents.EventName);
                context.Signature = GetHeaderValueOrDefault(request.Headers, Constants.Headers.CloudEvents.Signature);
                context.Headers = new Dictionary<string, StringValues>();
                foreach (var item in request.Headers)
                {
                    context.Headers.Add(item.Key, item.Value);
                }

                // UserId is optional, e.g. connect
                if (request.Headers.ContainsKey(Constants.Headers.CloudEvents.UserId))
                {
                    context.UserId = GetHeaderValueOrDefault(request.Headers, Constants.Headers.CloudEvents.UserId);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static string GetHeaderValueOrDefault(IHeaderDictionary header, string key)
        {
            header.TryGetValue(key, out var value);
            return value.Count > 0 ? value[0] : string.Empty;
        }
    }

}
