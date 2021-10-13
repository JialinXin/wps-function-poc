﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebPubSub.Common;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubContextBinding : BindingBase<WebPubSubContextAttribute>
    {
        private const string HttpRequestName = "$request";
        private readonly Type _userType;
        private readonly WebPubSubFunctionsOptions _options;

        public WebPubSubContextBinding(
            BindingProviderContext context,
            IConfiguration configuration,
            INameResolver nameResolver,
            WebPubSubFunctionsOptions options) : base(context, configuration, nameResolver)
        {
            _userType = context.Parameter.ParameterType;
            _options = options;
        }

        protected async override Task<IValueProvider> BuildAsync(WebPubSubContextAttribute attrResolved, IReadOnlyDictionary<string, object> bindingData)
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
                return new WebPubSubContextValueProvider(new WebPubSubContext($"HttpContext is null.", WebPubSubErrorCode.UserError), _userType);
            }

            try
            {
                // Attribute options will overwrite global settings.
                var validationOptions = _options.ValidationOptions;
                if (attrResolved.ValidationOptions != null)
                {
                    validationOptions = new WebPubSubValidationOptions(attrResolved.ValidationOptions);
                }
                var serviceRequest = await request.ReadWebPubSubRequestAsync(validationOptions).ConfigureAwait(false);

                switch (serviceRequest)
                {
                    case ValidationRequest validationRequest:
                        {
                            var response = new HttpResponseMessage();
                            if (validationRequest.IsValid)
                            {
                                response.Headers.Add(Constants.Headers.WebHookAllowedOrigin, Constants.AllowedAllOrigins);
                            }
                            else
                            {
                                response.StatusCode = HttpStatusCode.BadRequest;
                            }
                            var abuseRequest = new WebPubSubContext(validationRequest, response);
                            return new WebPubSubContextValueProvider(abuseRequest, _userType);
                        }
                    case ConnectEventRequest:
                    case UserEventRequest:
                    case ConnectedEventRequest:
                    case DisconnectedEventRequest:
                    default:
                        return new WebPubSubContextValueProvider(new WebPubSubContext(serviceRequest), _userType);
                }
            }
            catch (UnauthorizedAccessException unauthorized)
            {
                return new WebPubSubContextValueProvider(new WebPubSubContext(unauthorized.Message, WebPubSubErrorCode.Unauthorized), _userType);
            }
            catch (Exception ex)
            {
                return new WebPubSubContextValueProvider(new WebPubSubContext(ex.Message, WebPubSubErrorCode.UserError), _userType);
            }
        }
    }
}