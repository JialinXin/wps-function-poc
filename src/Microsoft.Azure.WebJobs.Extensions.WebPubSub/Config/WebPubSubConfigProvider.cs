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
using System.Xml;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [Extension("WebPubSub", "webpubsub")]
    internal class WebPubSubConfigProvider : IExtensionConfigProvider, IAsyncConverter<HttpRequestMessage, HttpResponseMessage>
    {
        private readonly IConfiguration _configuration;
        private readonly INameResolver _nameResolver;
        private readonly ILogger _logger;
        private readonly WebPubSubOptions _options;
        private readonly IWebPubSubTriggerDispatcher _dispatcher;

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
            _dispatcher = new WebPubSubTriggerDispatcher();
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
                AddSettings(_options.ConnectionString);
            }

            if (string.IsNullOrEmpty(_options.HubName))
            {
                _options.HubName = _nameResolver.Resolve(Constants.HubNameStringName);
            }

            if (_options.AllowedHosts == null && !string.IsNullOrEmpty(_nameResolver.Resolve(Constants.AllowedHostsName)))
            {
                _nameResolver.Resolve(Constants.AllowedHostsName).Split(',').Select(x => _options.AllowedHosts.Add(x));
            }

            var url = context.GetWebhookHandler();
            _logger.LogInformation($"Registered Web PubSub negotiate Endpoint = {url?.GetLeftPart(UriPartial.Path)}");

            // bindings
            context
                //.AddConverter<byte[], JObject>(JObject.FromObject)
                .AddConverter(new MessageToBinaryConverter())
                .AddConverter<WebPubSubConnection, JObject>(JObject.FromObject)
                .AddConverter<ConnectResponse, JObject>(JObject.FromObject)
                .AddConverter<MessageResponse, JObject>(JObject.FromObject)
                .AddOpenConverter<JObject, OpenType.Poco>(typeof(JObjectToPocoConverter<>))
                .AddOpenConverter<JObject, OpenType.Poco[]>(typeof(JObjectToPocoConverter<>));

            // Trigger binding
            context.AddBindingRule<WebPubSubTriggerAttribute>()
                .BindToTrigger<JObject>(new WebPubSubTriggerBindingProvider(_dispatcher));

            var webpubsubConnectionAttributeRule = context.AddBindingRule<WebPubSubConnectionAttribute>();
            webpubsubConnectionAttributeRule.AddValidator(ValidateWebPubSubConnectionAttributeBinding);
            webpubsubConnectionAttributeRule.BindToInput(GetClientConnection);

            var webPubSubAttributeRule = context.AddBindingRule<WebPubSubAttribute>();
            webPubSubAttributeRule.AddValidator(ValidateWebPubSubAttributeBinding);
            webPubSubAttributeRule.BindToCollector(CreateCollector);

            _logger.LogInformation("Azure Web PubSub binding initialized");
        }

        public Task<HttpResponseMessage> ConvertAsync(HttpRequestMessage input, CancellationToken cancellationToken)
        {
            return _dispatcher.ExecuteAsync(input, _options.AllowedHosts, _options.AccessKeys, cancellationToken);
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
            var hubName = FirstOrDefault(attribute.Hub, _options.HubName);
            return new WebPubSubService(connectionString, hubName);
        }

        private IAsyncCollector<WebPubSubEvent> CreateCollector(WebPubSubAttribute attribute)
        {
            return new WebPubSubAsyncCollector(GetService(attribute));
        }

        private WebPubSubConnection GetClientConnection(WebPubSubConnectionAttribute attribute)
        {
            var service = new WebPubSubService(attribute.ConnectionStringSetting, attribute.Hub);
            var claims = attribute.GetClaims();
            return service.GetClientConnection(claims);
        }

        private void ValidateConnectionString(string attributeConnectionString, string attributeConnectionStringName)
        {
            AddSettings(attributeConnectionString);
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

        private void AddSettings(string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                var item = Utilities.ParseConnectionString(connectionString);
                _options.AllowedHosts.Add(new Uri(item.EndPoint).Host);
                _options.AccessKeys.Add(item.AccessKey);
            }
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

        private sealed class MessageToStringConverter : IAsyncConverter<WebPubSubMessage, string>
        {
            public Task<string> ConvertAsync(WebPubSubMessage input, CancellationToken cancellationToken)
            {
                if (input == null)
                {
                    throw new ArgumentNullException();
                }

                if (input.Body == null)
                {
                    return null;
                }

                return Task.FromResult(input.Body.ToString());
            }
        }

        private sealed class MessageToBinaryConverter : IAsyncConverter<WebPubSubMessage, byte[]>
        {
            public Task<byte[]> ConvertAsync(WebPubSubMessage input, CancellationToken cancellationToken)
            {
                if (input == null)
                {
                    throw new ArgumentNullException();
                }

                if (input.Body == null)
                {
                    return null;
                }

                return Task.FromResult(input.Payload);
            }
        }

        private sealed class JObjectToPocoConverter<T> : IConverter<JObject, T>
        {
            public T Convert(JObject input)
            {
                return input.ToObject<T>();
            }
        }
    }
}
