using Microsoft.Azure.WebJobs.Description;
using System;
using System.IO;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
    [Binding]
    public class WebPubSubRequestAttribute : Attribute
    {
        [AutoResolve(Default = null)]
        public string UserId { get; internal set; } = "{headers.ce-userId}";

        [AutoResolve(Default = null)]
        public string ConnectionId { get; internal set; } = "{headers.ce-connectionId}";

        [AutoResolve(Default = null)]
        public string Hub { get; internal set; } = "{headers.ce-hub}";

        [AutoResolve(Default = null)]
        public string EventType { get; internal set; } = "{headers.ce-type}";

        [AutoResolve(Default = null)]
        public string EventName { get; internal set; } = "{headers.ce-eventName}";

        [AutoResolve(Default = null)]
        public string Signature { get; internal set; } = "{headers.ce-signature}";

        [AutoResolve(Default = null)]
        public string WebRequestOrigin { get; internal set; } = "{headers.WebHook-Request-Origin}";
    }
}
