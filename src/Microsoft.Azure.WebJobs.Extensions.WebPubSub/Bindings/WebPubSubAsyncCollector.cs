using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubAsyncCollector<T>: IAsyncCollector<T>
    {
        private readonly IWebPubSubService _service;
        private readonly WebPubSubOutputConverter _converter;

        internal WebPubSubAsyncCollector(IWebPubSubService service, string hub)
        {
            _service = service;
            _converter = new WebPubSubOutputConverter();
        }

        public async Task AddAsync(T item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Binding Object.");
            }

            var convertItem = _converter.ConvertToWebPubSubOutput(item);

            if (convertItem.GetType() == typeof(MessageData))
            {
                MessageData message = convertItem as MessageData;
                await _service.Send(message).ConfigureAwait(false);
            }
            else if (convertItem.GetType() == typeof(GroupData))
            {
                var groupData = convertItem as GroupData;

                if (groupData.Action == GroupAction.Add)
                {
                    await _service.AddToGroup(groupData).ConfigureAwait(false);
                }
                else
                {
                    await _service.RemoveFromGroup(groupData).ConfigureAwait(false);
                }
            }
            else if (convertItem.GetType() == typeof(ExistenceData))
            {
                var existenceData = convertItem as ExistenceData;

                await _service.CheckExistence(existenceData).ConfigureAwait(false);
            }
            else if (convertItem.GetType() == typeof(ConnectionCloseData))
            {
                var closeData = convertItem as ConnectionCloseData;

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
