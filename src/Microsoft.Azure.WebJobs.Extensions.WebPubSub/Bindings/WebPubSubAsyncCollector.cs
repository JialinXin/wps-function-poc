using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubAsyncCollector: IAsyncCollector<WebPubSubEvent>
    {
        private readonly IWebPubSubService _service;
        private readonly WebPubSubOutputConverter _converter;

        internal WebPubSubAsyncCollector(IWebPubSubService service, string hub)
        {
            _service = service;
            _converter = new WebPubSubOutputConverter();
        }

        public async Task AddAsync(WebPubSubEvent item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Binding Object.");
            }

            // var convertItem = _converter.ConvertToWebPubSubData(item);

            if (item is MessageData message)
            {
                await _service.Send(message).ConfigureAwait(false);
            }
            else if (item is GroupData groupData)
            {
                if (groupData.Action == GroupAction.Add)
                {
                    await _service.AddToGroup(groupData).ConfigureAwait(false);
                }
                else
                {
                    await _service.RemoveFromGroup(groupData).ConfigureAwait(false);
                }
            }
            else if (item is ExistenceData existenceData)
            {
                await _service.CheckExistence(existenceData).ConfigureAwait(false);
            }
            else if (item is ConnectionCloseData closeData)
            {
                await _service.CloseConnection(closeData).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException("Unsupport Binding Type.");
            }
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
