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
            bindingData.Add("connection", triggerEvent.Context);
            bindingData.Add("message", triggerEvent.Payload != null ? new MemoryStream(triggerEvent.Payload) : null);
            bindingData.Add("dataType", triggerEvent.DataType);
            bindingData.Add("claims", triggerEvent.Claims);
            bindingData.Add("reason", triggerEvent.Reason);
            bindingData.Add("subprotocols", triggerEvent.Subprotocols);
        }

        /// <summary>
        /// Defined what other bindings can use and return value.
        /// </summary>
        private IReadOnlyDictionary<string, Type> CreateBindingContract()
        {
            var contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "connection", typeof(ConnectionContext) },
                { "message", typeof(Stream) },
                { "dataType", typeof(MessageDataType) },
                { "claims", typeof(IDictionary<string, string[]>) },
                { "reason", typeof(string) },
                { "subprotocols", typeof(string[]) },
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
            //private readonly ConnectionContext _context;
            private readonly ParameterInfo _parameter;
            // optional parameters
            private readonly WebPubSubTriggerEvent _triggerEvent;

            public WebPubSubTriggerValueProvider(ParameterInfo parameter, WebPubSubTriggerEvent triggerEvent)
            {
                _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
                _triggerEvent = triggerEvent ?? throw new ArgumentNullException(nameof(triggerEvent));
            }

            public Task<object> GetValueAsync()
            {
                if (_parameter.ParameterType == typeof(ConnectionContext))
                {
                    return Task.FromResult<object>(_triggerEvent.Context);
                }
                else if (_parameter.ParameterType == typeof(Stream))
                {
                    return Task.FromResult<object>(new MemoryStream(_triggerEvent.Payload));
                }
                else if (_parameter.ParameterType == typeof(string))
                {
                    return Task.FromResult<object>(_triggerEvent.Reason);
                }
                else if (_parameter.ParameterType == typeof(MessageDataType))
                {
                    return Task.FromResult<object>(_triggerEvent.DataType);
                }
                else if (_parameter.ParameterType == typeof(string[]))
                {
                    return Task.FromResult<object>(_triggerEvent.Subprotocols);
                }
                else if (_parameter.ParameterType == typeof(IDictionary<string, string[]>))
                {
                    return Task.FromResult<object>(_triggerEvent.Claims);
                }
                else if (_parameter.ParameterType == typeof(object) ||
                         _parameter.ParameterType == typeof(JObject))
                {
                    return Task.FromResult<object>(JObject.FromObject(_triggerEvent));
                }

                return Task.FromResult<object>(null);
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
