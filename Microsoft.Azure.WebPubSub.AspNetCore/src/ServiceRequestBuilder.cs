// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    /// <summary>
    /// Service request builder.
    /// </summary>
    public class ServiceRequestBuilder
    {
        private readonly IServiceProvider _provider;
        private readonly Dictionary<WebPubSubHub, string> _hubRegistry = new Dictionary<WebPubSubHub, string>();

        private WebPubSubValidationOptions _options;
        private WebPubSubHub _hub;
        private string _path;

        internal ServiceRequestBuilder(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Maps incoming requests with the specified path to the specified <see cref="WebPubSubHub"/> type.
        /// </summary>
        /// <typeparam name="THub">User defined <see cref="WebPubSubHub"/></typeparam>
        /// <param name="path">Target request path.</param>
        public void MapHub<THub>(PathString path) where THub: WebPubSubHub
        {
            _path = path;
            _hub = Create<THub>();
            _hubRegistry[_hub] = _path;
        }

        internal void AddValidationOptions(WebPubSubValidationOptions options)
        {
            _options = options;
        }

        internal ServiceRequestHandlerAdapter Build()
        {
            return new ServiceRequestHandlerAdapter(_options, _hub, _path);
        }

        private THub Create<THub>() where THub: WebPubSubHub
        {
            var hub = _provider.GetService<THub>();
            if (hub == null)
            {
                hub = ActivatorUtilities.CreateInstance<THub>(_provider);
            }

            if (_hubRegistry.TryGetValue(hub, out _))
            {
                Debug.Assert(true, $"{typeof(THub)} must not be reused.");
            }
            return hub;
        }
    }
}
