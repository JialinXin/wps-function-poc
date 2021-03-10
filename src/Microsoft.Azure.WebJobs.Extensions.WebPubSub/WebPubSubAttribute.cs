using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public class WebPubSubAttribute : Attribute
    {
        [ConnectionString]
        public string ConnectionStringSetting { get; set; }

        [AutoResolve]
        public string Hub { get; set; }
    }
}
