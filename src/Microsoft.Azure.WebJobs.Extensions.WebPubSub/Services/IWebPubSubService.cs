using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal interface IWebPubSubService
    {
        Task Send(MessageData messageData);

        Task AddToGroup(GroupData groupData);

        Task RemoveFromGroup(GroupData groupData);

        Task CheckExistence(ExistenceData existenceData);

        Task CloseConnection(ConnectionCloseData closeData);
    }
}
