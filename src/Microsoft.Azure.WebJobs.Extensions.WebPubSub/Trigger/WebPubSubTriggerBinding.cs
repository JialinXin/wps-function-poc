using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubTriggerBinding : ITriggerBinding
    {
        private readonly ParameterInfo _parameterInfo;
        private readonly WebPubSubTriggerAttribute _attribute;
        private readonly IWebPubSubTriggerDispatcher _dispatcher;

        public WebPubSubTriggerBinding(ParameterInfo parameterInfo, WebPubSubTriggerAttribute attribute, IWebPubSubTriggerDispatcher dispatcher)
        {
            _parameterInfo = parameterInfo ?? throw new ArgumentNullException(nameof(parameterInfo));
            _attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            BindingDataContract = CreateBindingContract();
        }

        public Type TriggerValueType => typeof(WebPubSubTriggerEvent);

        public IReadOnlyDictionary<string, Type> BindingDataContract { get; }

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (value is WebPubSubTriggerEvent triggerEvent)
            {
                AddBindingData(bindingData, triggerEvent);

                return Task.FromResult<ITriggerData>(new TriggerData(new WebPubSubTriggerValueProvider(_parameterInfo, triggerEvent), bindingData)
                {
                    ReturnValueProvider = triggerEvent.TaskCompletionSource == null ? null : new TriggerReturnValueProvider(triggerEvent.TaskCompletionSource),
                });
            }

            return Task.FromResult<ITriggerData>(null);
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Get listener key from attributes.
            var attributeName = $"{_attribute.Hub}.{_attribute.EventType}.{_attribute.EventName}".ToLower();
            var listernerKey = attributeName;

            return Task.FromResult<IListener>(new WebPubSubListener(context.Executor,  listernerKey, _dispatcher));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor
            {
                Name = _parameterInfo.Name,
            };
        }

        private void AddBindingData(Dictionary<string, object> bindingData, WebPubSubTriggerEvent triggerEvent)
        {
            bindingData.Add("connectionContext", triggerEvent.ConnectionContext);
            bindingData.Add("message", triggerEvent.Message != null ? triggerEvent.Message : null);
            bindingData.Add("claims", triggerEvent.Claims);
            bindingData.Add("query", triggerEvent.Query);
            bindingData.Add("reason", triggerEvent.Reason);
            bindingData.Add("subprotocols", triggerEvent.Subprotocols);
            //var properties = Utilities.GetProperties(triggerEvent.GetType());
            //foreach (var property in properties)
            //{
            //    if (property.PropertyType == typeof(TaskCompletionSource<>))
            //    {
            //        continue;
            //    }
            //    bindingData.Add(property.Name, Utilities.GetProperty(triggerEvent.GetType(), property.Name).GetValue(triggerEvent));
            //}
        }

        /// <summary>
        /// Defined what other bindings can use and return value.
        /// </summary>
        private IReadOnlyDictionary<string, Type> CreateBindingContract()
        {
            var contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "connectionContext", typeof(ConnectionContext) },
                { "message", typeof(WebPubSubMessage) },
                { "claims", typeof(IDictionary<string, string[]>) },
                { "query", typeof(IDictionary<string, string[]>) },
                { "reason", typeof(string) },
                { "subprotocols", typeof(string[]) },
                { "$return", typeof(object).MakeByRefType() },
            };

            //var properties = Utilities.GetProperties(typeof(WebPubSubTriggerEvent));
            //foreach (var property in properties)
            //{
            //    if (property.PropertyType == typeof(TaskCompletionSource<>))
            //    {
            //        continue;
            //    }
            //    contract.Add(property.Name, property.PropertyType);
            //}

            return contract;
        }

        /// <summary>
        /// A provider that responsible for providing value in various type to be bond to function method parameter.
        /// </summary>
        private class WebPubSubTriggerValueProvider : IValueBinder
        {
            private readonly ParameterInfo _parameter;
            private readonly WebPubSubTriggerEvent _triggerEvent;

            public WebPubSubTriggerValueProvider(ParameterInfo parameter, WebPubSubTriggerEvent triggerEvent)
            {
                _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
                _triggerEvent = triggerEvent ?? throw new ArgumentNullException(nameof(triggerEvent));
            }

            public Task<object> GetValueAsync()
            {
                if (_parameter.ParameterType == typeof(object) ||
                         _parameter.ParameterType == typeof(JObject))
                {
                    return Task.FromResult<object>(JObject.FromObject(GetValueByName(_parameter.Name)));
                }
                else
                {
                    return Task.FromResult(GetValueByName(_parameter.Name));
                }
            }

            public string ToInvokeString()
            {
                return _triggerEvent.ToString();
            }

            public Type Type => _parameter.GetType();

            // No use here
            public Task SetValueAsync(object value, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            private object GetValueByName(string parameterName)
            {
                var property = Utilities.GetProperty(typeof(WebPubSubTriggerEvent), parameterName);
                if (property != null)
                {
                    return property.GetValue(_triggerEvent);
                }
                throw new ArgumentException($"Invalid parameter name: {parameterName}, supported names are: {Utilities.GetTriggerEventSupportedNames()}");
            }
        }

        /// <summary>
        /// A provider to handle return value.
        /// </summary>
        private class TriggerReturnValueProvider : IValueBinder
        {
            private readonly TaskCompletionSource<object> _tcs;

            public TriggerReturnValueProvider(TaskCompletionSource<object> tcs)
            {
                _tcs = tcs;
            }

            public Task<object> GetValueAsync()
            {
                // Useless for return value provider
                return null;
            }

            public string ToInvokeString()
            {
                // Useless for return value provider
                return string.Empty;
            }

            public Type Type => typeof(object).MakeByRefType();

            public Task SetValueAsync(object value, CancellationToken cancellationToken)
            {
                _tcs.TrySetResult(value);
                return Task.CompletedTask;
            }
        }
    }
}
