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
            Hub = hub;
            EventName = eventName;
            EventType = eventType;
        }

        /// <summary>
        /// The hub of request.
        /// </summary>
        [AutoResolve]
        [Required]
        public string Hub { get; set; }
        
        /// <summary>
        /// The event of the request
        /// </summary>
        [AutoResolve]
        [Required]
        public string EventName { get; set; }

        /// <summary>
        /// The event type, allowed value in system or user
        /// </summary>
        [AutoResolve]
        [RegularExpression("(?i)(user|system)", ErrorMessage = "EventType must be either 'system' or 'user'.")]
        public string EventType { get; set; } = "system";
    }
}
