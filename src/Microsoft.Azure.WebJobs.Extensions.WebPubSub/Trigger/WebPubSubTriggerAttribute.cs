// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebPubSub.Common;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [AttributeUsage(AttributeTargets.Parameter)]
#pragma warning disable CS0618 // Type or member is obsolete
    [Binding(TriggerHandlesReturnValue = true)]
#pragma warning restore CS0618 // Type or member is obsolete
    public class WebPubSubTriggerAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="eventType"></param>
        /// <param name="eventName"></param>
        /// <param name="connectionStrings"></param>
        public WebPubSubTriggerAttribute(string hub, WebPubSubEventType eventType, string eventName, params string[] connections)
        {
            Hub = hub;
            EventName = eventName;
            EventType = eventType;
            Connections = connections;
        }

        /// <summary>
        /// Used to map to method name automatically
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="eventName"></param>
        /// <param name="eventType"></param>
        public WebPubSubTriggerAttribute(string hub, WebPubSubEventType eventType, string eventName)
            : this(hub, eventType, eventName, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="eventName"></param>
        public WebPubSubTriggerAttribute(WebPubSubEventType eventType, string eventName, params string[] connections)
            : this("", eventType, eventName, connections)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="eventName"></param>
        public WebPubSubTriggerAttribute(WebPubSubEventType eventType, string eventName)
            : this ("", eventType, eventName)
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
        public string EventName { get; }

        /// <summary>
        /// The event type, allowed value is system or user
        /// </summary>
        [Required]
        public WebPubSubEventType EventType { get; }

        /// <summary>
        /// Allowed service upstream ConnectionString for Signature checks.
        /// </summary>
        public string[] Connections { get; }
    }
}
