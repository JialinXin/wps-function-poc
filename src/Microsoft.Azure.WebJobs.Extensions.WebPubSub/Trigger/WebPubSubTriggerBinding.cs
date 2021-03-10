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
                //var bindingContext = triggerEvent.Context;

                // attribute settings valids for connect/disconnect only.
                //if (_attribute.EventName != Constants.Events.Connect && _attribute.EventName != Constants.Events.Disconnect)
                //{
                //    // TODO: warns 
                //    Console.WriteLine("ignored settings.");
                //}

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
            // var functionName = _parameterInfo.Member.GetCustomAttribute<FunctionNameAttribute>(false);
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

        /// <summary>
        /// Defined what other bindings can use and return value.
        /// </summary>
        private IReadOnlyDictionary<string, Type> CreateBindingContract()
        {
            var contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                // Functions can bind to parameter name "hubName" directly
                //{ "hubName", typeof(string) },
                { "message", typeof(Stream) },
                { "messageResponse", typeof(MessageResponse) },
                { "connectResponse", typeof(ConnectResponse) },
                { "response", typeof(WebPubSubEventResponse) },
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
            private readonly InvocationContext _context;
            private readonly ParameterInfo _parameter;
            // optional parameters
            private readonly Stream _message;
            private readonly WebPubSubEventResponse _response;

            public WebPubSubTriggerValueProvider(ParameterInfo parameter, WebPubSubTriggerEvent triggerEvent)
            {
                _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
                _context = triggerEvent.Context ?? throw new ArgumentNullException(nameof(triggerEvent.Context));
                _message = triggerEvent.Message;
                _response = triggerEvent.Response;
            }

            public Task<object> GetValueAsync()
            {
                if (_parameter.ParameterType == typeof(InvocationContext))
                {
                    return Task.FromResult<object>(_context);
                }
                else if (_parameter.ParameterType == typeof(Stream))
                {
                    return Task.FromResult<object>(_message);
                }
                else if (_parameter.ParameterType == typeof(WebPubSubEventResponse))
                {
                    return Task.FromResult<object>(_response);
                }
                else if (_parameter.ParameterType == typeof(object) ||
                         _parameter.ParameterType == typeof(JObject))
                {
                    return Task.FromResult<object>(JObject.FromObject(_context));
                }

                return Task.FromResult<object>(null);
            }

            public string ToInvokeString()
            {
                return _context.ToString();
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
