using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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

                // If ParameterNames are set, bind them in order.
                // To reduce undefined situation, number of arguments should keep consist with that of ParameterNames
                if (_attribute.ParameterNames != null && _attribute.ParameterNames.Length != 0)
                {
                    //if (bindingContext.Payload == null ||
                    //    bindingContext.Arguments.Length != _attribute.ParameterNames.Length)
                    //{
                    //    throw new ArgumentException(nameof(value));
                    //    //throw new SignalRTriggerParametersNotMatchException(_attribute.ParameterNames.Length, bindingContext.Arguments?.Length ?? 0);
                    //}
                    //
                    //AddParameterNamesBindingData(bindingData, _attribute.ParameterNames, bindingContext.Arguments);
                }

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

            var functionNameAttribute = _parameterInfo.Member.GetCustomAttribute<FunctionNameAttribute>(false);
            var methodName = functionNameAttribute.Name;

            return Task.FromResult<IListener>(new WebPubSubListener(context.Executor,  methodName, _dispatcher));
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

        private void AddParameterNamesBindingData(Dictionary<string, object> bindingData, string[] parameterNames, object[] arguments)
        {
            var length = parameterNames.Length;
            for (var i = 0; i < length; i++)
            {
                if (BindingDataContract.TryGetValue(parameterNames[i], out var type))
                {
                    bindingData.Add(parameterNames[i], ConvertValueIfNecessary(arguments[i], type));
                }
                else
                {
                    bindingData.Add(parameterNames[i], arguments[i]);
                }
            }
        }

        private object ConvertValueIfNecessary(object value, Type targetType)
        {
            if (value != null && !targetType.IsAssignableFrom(value.GetType()))
            {
                var underlyingTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                var jObject = value as JObject;
                if (jObject != null)
                {
                    value = jObject.ToObject(targetType);
                }
                else if (underlyingTargetType == typeof(Guid) && value.GetType() == typeof(string))
                {
                    // Guids need to be converted by their own logic
                    // Intentionally throw here on error to standardize behavior
                    value = Guid.Parse((string)value);
                }
                else
                {
                    // if the type is nullable, we only need to convert to the
                    // correct underlying type
                    value = Convert.ChangeType(value, underlyingTargetType);
                }
            }

            return value;
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
