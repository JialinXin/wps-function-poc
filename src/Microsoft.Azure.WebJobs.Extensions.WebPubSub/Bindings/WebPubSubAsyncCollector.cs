using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubAsyncCollector: IAsyncCollector<WebPubSubEvent>
    {
        private readonly IWebPubSubService _service;

        internal WebPubSubAsyncCollector(IWebPubSubService service, string hub)
        {
            _service = service;
        }

        public async Task AddAsync(WebPubSubEvent item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Binding Object.");
            }

            try
            {
                var method = typeof(IWebPubSubService).GetMethod(item.Operation.ToString(),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                var task = (Task)method.Invoke(_service, new object[] { item });

                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Not supported operation: {item.Operation}, exception: {ex}");
            }
            
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
