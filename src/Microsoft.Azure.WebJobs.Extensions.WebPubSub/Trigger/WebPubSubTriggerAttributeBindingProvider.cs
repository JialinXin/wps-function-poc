using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    class WebPubSubTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            throw new NotImplementedException();
        }
    }
}
