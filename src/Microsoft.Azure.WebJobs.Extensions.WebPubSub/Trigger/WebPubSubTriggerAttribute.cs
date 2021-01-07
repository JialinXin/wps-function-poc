using Microsoft.Azure.WebJobs.Description;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
    [Binding]
    class WebPubSubTriggerAttribute : Attribute
    {
        public WebPubSubTriggerAttribute()
        {
        }

        public WebPubSubTriggerAttribute(string hubName, string category, string @event) : this(hubName, category, @event, Array.Empty<string>())
        {
        }

        public WebPubSubTriggerAttribute(string hubName, string category, string @event, params string[] parameterNames)
        {
            HubName = hubName;
            Category = category;
            Event = @event;
            ParameterNames = parameterNames;
        }

        /// <summary>
        /// Connection string that connect to Web PubSub Service
        /// </summary>
        public string ConnectionStringSetting { get; set; } = Constants.WebPubSubConnectionStringName;

        /// <summary>
        /// The hub of request belongs to.
        /// </summary>
        [AutoResolve]
        public string HubName { get; }

        /// <summary>
        /// The event of the request.
        /// </summary>
        [AutoResolve]
        public string Event { get; }

        /// <summary>
        /// Two optional value: connections and messages
        /// </summary>
        [AutoResolve]
        public string Category { get; }

        /// <summary>
        /// Used for messages category. All the name defined in <see cref="ParameterNames"/> will map to
        /// Arguments in InvocationMessage by order. And the name can be used in parameters of method
        /// directly.
        /// </summary>
        public string[] ParameterNames { get; }
    }
}
