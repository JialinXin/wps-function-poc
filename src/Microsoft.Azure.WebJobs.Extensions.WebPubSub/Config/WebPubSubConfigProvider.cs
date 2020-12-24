﻿using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [Extension("WebPubSub")]
    internal class WebPubSubConfigProvider : IExtensionConfigProvider, IAsyncConverter<HttpRequestMessage, HttpResponseMessage>
    {
        private readonly IConfiguration _configuration;
        private readonly INameResolver _nameResolver;
        private readonly ILogger _logger;
        private readonly WebPubSubOptions _options;

        public WebPubSubConfigProvider(
            IOptions<WebPubSubOptions> options,
            INameResolver nameResolver,
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            _options = options.Value;
            _logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("WebPubSub"));
            _nameResolver = nameResolver;
            _configuration = configuration;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (string.IsNullOrEmpty(_options.ConnectionString))
            {
                _options.ConnectionString = _nameResolver.Resolve(Constants.WebPubSubConnectionStringName);
            }

            context.AddConverter<string, JObject>(JObject.FromObject)
                   .AddConverter<WebPubSubConnection, JObject>(JObject.FromObject)
                   .AddConverter<JObject, MessageData>(input => input.ToObject<MessageData>())
                   .AddConverter<JObject, GroupData>(input => input.ToObject<GroupData>())
                   .AddConverter<JObject, ExistenceData>(input => input.ToObject<ExistenceData>())
                   .AddConverter<JObject, ConnectionCloseData>(input => input.ToObject<ConnectionCloseData>());

            var webpubsubConnectionAttributeRule = context.AddBindingRule<WebPubSubConnectionAttribute>();
            webpubsubConnectionAttributeRule.AddValidator(ValidateWebPubSubConnectionAttributeBinding);
            webpubsubConnectionAttributeRule.BindToInput<WebPubSubConnection>(GetClientConnection);

            var signalRAttributeRule = context.AddBindingRule<WebPubSubAttribute>();
            signalRAttributeRule.AddValidator(ValidateWebPubSubAttributeBinding);
            signalRAttributeRule.BindToCollector<WebPubSubOpenType>(typeof(WebPubSubCollectorBuilder<>), this);

            _logger.LogInformation("SignalRService binding initialized");
        }

        public Task<HttpResponseMessage> ConvertAsync(HttpRequestMessage input, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private void ValidateWebPubSubConnectionAttributeBinding(WebPubSubConnectionAttribute attribute, Type type)
        {
            ValidateConnectionString(
                attribute.ConnectionStringSetting,
                $"{nameof(WebPubSubConnectionAttribute)}.{nameof(WebPubSubConnectionAttribute.ConnectionStringSetting)}");
        }

        private void ValidateWebPubSubAttributeBinding(WebPubSubAttribute attribute, Type type)
        {
            ValidateConnectionString(
                attribute.ConnectionStringSetting,
                $"{nameof(WebPubSubAttribute)}.{nameof(WebPubSubAttribute.ConnectionStringSetting)}");
        }
        internal WebPubSubService GetService(WebPubSubAttribute attribute)
        {
            var connectionString = FirstOrDefault(attribute.ConnectionStringSetting, _options.ConnectionString);
            var hubName = FirstOrDefault(attribute.HubName, _options.HubName);
            return new WebPubSubService(connectionString, hubName);
        }

        private WebPubSubConnection GetClientConnection(WebPubSubConnectionAttribute attribute)
        {
            var service = new WebPubSubService(attribute.ConnectionStringSetting);
            var claims = attribute.GetClaims();
            return service.GetClientConnection(attribute.HubName, claims);
        }

        private void ValidateConnectionString(string attributeConnectionString, string attributeConnectionStringName)
        {
            var connectionString = FirstOrDefault(attributeConnectionString, _options.ConnectionString);

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(string.Format($"The Service connection string must be set either via an '{Constants.WebPubSubConnectionStringName}' app setting, via an '{Constants.WebPubSubConnectionStringName}' environment variable, or directly in code via {nameof(WebPubSubOptions)}.{nameof(WebPubSubOptions.ConnectionString)} or {{0}}.", 
                    attributeConnectionStringName));
            }
        }
        
        private string FirstOrDefault(params string[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrEmpty(v));
        }

        private sealed class WebPubSubOpenType : OpenType.Poco
        {
            public override bool IsMatch(Type type, OpenTypeMatchContext context)
            {
                if (type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return false;
                }

                if (type.FullName == "System.Object")
                {
                    return true;
                }

                return base.IsMatch(type, context);
            }
        }
    }
}
