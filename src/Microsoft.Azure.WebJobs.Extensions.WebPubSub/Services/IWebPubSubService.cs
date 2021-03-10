using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal interface IWebPubSubService
    {
        Task Send(MessageEvent messageData);

        Task AddToGroup(GroupEvent groupData);

        Task RemoveFromGroup(GroupEvent groupData);

        Task CheckExistence(ExistenceEvent existenceData);

        Task CloseConnection(ConnectionCloseData closeData);
    }
}
