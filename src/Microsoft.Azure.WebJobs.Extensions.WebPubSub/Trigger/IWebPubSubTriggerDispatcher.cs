using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal interface IWebPubSubTriggerDispatcher
    {
        void AddListener(string key, WebPubSubListener listener);

        Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage req, HashSet<string> allowedHosts, CancellationToken token = default);
    }
}
