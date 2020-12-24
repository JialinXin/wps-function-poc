using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubCollectorBuilder<T> : IConverter<WebPubSubAttribute, IAsyncCollector<T>>
    {
        private readonly WebPubSubConfigProvider _configProvider;

        public WebPubSubCollectorBuilder(WebPubSubConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public IAsyncCollector<T> Convert(WebPubSubAttribute attribute)
        {
            var service = _configProvider.GetService(attribute);
            return new WebPubSubAsyncCollector<T>(service, attribute.HubName);
        }
    }
}
