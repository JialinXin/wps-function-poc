using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    //internal class WebPubSubCollectorBuilder : IConverter<WebPubSubAttribute, IAsyncCollector<WebPubSubEvent>>
    //{
    //    private readonly WebPubSubConfigProvider _configProvider;
    //
    //    public WebPubSubCollectorBuilder(WebPubSubConfigProvider configProvider)
    //    {
    //        _configProvider = configProvider;
    //    }
    //
    //    public IAsyncCollector<WebPubSubEvent> Convert(WebPubSubAttribute attribute)
    //    {
    //        var service = _configProvider.GetService(attribute);
    //        return new WebPubSubAsyncCollector(service);
    //    }
    //}
}
