using Microsoft.Azure.WebJobs.Description;
using System;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
    [Binding]
    public class WebPubSubTriggerAttribute : Attribute
    {

        /// <summary>
        /// Used to map to method name automatically
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="eventName"></param>
        /// <param name="eventType"></param>
        public WebPubSubTriggerAttribute(string hub, string eventName, string eventType = "system")
        {
            if (!eventType.ToLower().Equals(Constants.EventTypes.User) && 
                !eventType.ToLower().Equals(Constants.EventTypes.System))
            {
                throw new ArgumentException("Not supported event type");
            }

            Hub = hub;
            EventName = eventName;
            EventType = eventType;
        }

        public WebPubSubTriggerAttribute(string eventName, string eventType = "system")
            : this ("", eventName, eventType)
        {
        }

        /// <summary>
        /// The hub of request.
        /// </summary>
        [AutoResolve]
        public string Hub { get; }
        
        /// <summary>
        /// The event of the request
        /// </summary>
        [Required]
        [AutoResolve]
        public string EventName { get; }

        /// <summary>
        /// The event type, allowed value is system or user
        /// </summary>
        [AutoResolve]
        public string EventType { get; }

    }
}
