﻿using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubRequestBindingProvider : IBindingProvider
    {
        private readonly WebPubSubOptions _options;
        private readonly INameResolver _nameResolver;
        private readonly IConfiguration _configuration;

        public WebPubSubRequestBindingProvider(WebPubSubOptions options, INameResolver nameResolver, IConfiguration configuration)
        {
            _options = options;
            _nameResolver = nameResolver;
            _configuration = configuration;
        }
    
        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
    
            ParameterInfo parameter = context.Parameter;
            WebPubSubRequestAttribute attribute = parameter.GetCustomAttribute<WebPubSubRequestAttribute>(inherit: false);
            if (attribute == null)
            {
                return Task.FromResult<IBinding>(null);
            }

            return Task.FromResult<IBinding>(new WebPubSubRequestBinding(context, _configuration, _nameResolver, _options));
        }
    }
}
