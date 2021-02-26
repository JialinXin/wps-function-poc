﻿using System;
using System.Collections.Generic;
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
                var bindingContext = triggerEvent.Context;

                // attribute settings valids for connect/disconnect only.
                //if (_attribute.EventName != Constants.Events.Connect && _attribute.EventName != Constants.Events.Disconnect)
                //{
                //    // TODO: warns 
                //    Console.WriteLine("ignored settings.");
                //}

                return Task.FromResult<ITriggerData>(new TriggerData(new WebPubSubTriggerValueProvider(_parameterInfo, bindingContext), bindingData)
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

            // Get listener key from function name which ensures unique
            // Format: hub-eventName or eventName(for default hub)
            // var attributeName = $"{_attribute.Hub}-{_attribute.EventName}".ToLower();
            var functionName = _parameterInfo.Member.GetCustomAttribute<FunctionNameAttribute>(false);
            var listernerKey = functionName.Name.ToLower();

            return Task.FromResult<IListener>(new WebPubSubListener(context.Executor,  listernerKey, _dispatcher));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor
            {
                Name = _parameterInfo.Name,
            };
        }

        /// <summary>
        /// Defined what other bindings can use and return value.
        /// </summary>
        private IReadOnlyDictionary<string, Type> CreateBindingContract()
        {
            var contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                // Functions can bind to parameter name "hubName" directly
                { "hubName", typeof(string) },
                { "$return", typeof(object).MakeByRefType() },
            };

            return contract;
        }

        // TODO: Add more supported type
        /// <summary>
        /// A provider that responsible for providing value in various type to be bond to function method parameter.
        /// </summary>
        private class WebPubSubTriggerValueProvider : IValueBinder
        {
            private readonly InvocationContext _value;
            private readonly ParameterInfo _parameter;

            public WebPubSubTriggerValueProvider(ParameterInfo parameter, InvocationContext value)
            {
                _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
                _value = value ?? throw new ArgumentNullException(nameof(value));
            }

            public Task<object> GetValueAsync()
            {
                if (_parameter.ParameterType == typeof(InvocationContext))
                {
                    return Task.FromResult<object>(_value);
                }
                else if (_parameter.ParameterType == typeof(object) ||
                         _parameter.ParameterType == typeof(JObject))
                {
                    return Task.FromResult<object>(JObject.FromObject(_value));
                }

                return Task.FromResult<object>(null);
            }

            public string ToInvokeString()
            {
                return _value.ToString();
            }

            public Type Type => _parameter.GetType();

            // No use here
            public Task SetValueAsync(object value, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
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
