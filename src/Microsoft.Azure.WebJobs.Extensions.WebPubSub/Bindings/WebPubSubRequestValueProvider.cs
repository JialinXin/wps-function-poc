using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubRequestValueProvider : IValueProvider
    {
        private readonly WebPubSubRequest _request;
        private readonly string _invokeString;

        public Type Type { get; }

        public WebPubSubRequestValueProvider(WebPubSubRequest request, Type type, string invokeString)
        {
            _request = request;
            _invokeString = invokeString;
            Type = type;
        }

        public Task<object> GetValueAsync()
        {
            return Task.FromResult(GetRequest());
        }

        public string ToInvokeString()
        {
            return _invokeString;
        }

        private object GetRequest()
        {
            if (Type == typeof(JObject))
            {
                return JObject.FromObject(_request);
            }
            if (Type == typeof(string))
            {
                return JObject.FromObject(_request).ToString();
            }

            return _request;
        }
    }
}
