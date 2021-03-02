using Microsoft.Azure.WebJobs.Description;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
    [Binding]
    public class WebPubSubTriggerAttribute : Attribute
    {
        public WebPubSubTriggerAttribute()
        {
        }

        /// <summary>
        /// Used for Connect/Disconnect event which can't be mapped to method name automatically
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="eventName"></param>
        //public WebPubSubTriggerAttribute(string hub, string eventName)
        //{
        //    Hub = hub;
        //    EventName = eventName;
        //}

        /// <summary>
        /// Connection string that connect to Web PubSub Service
        /// </summary>
        public string ConnectionStringSetting { get; set; } = Constants.WebPubSubConnectionStringName;

        /// <summary>
        /// The hub of request belongs to.
        /// </summary>
        //[AutoResolve]
        //public string Hub { get; set; }
        //
        ///// <summary>
        ///// The event of the request. Required for Connect/Disconnect
        ///// </summary>
        //[AutoResolve]
        //public string EventName { get; }
    }
}
