namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class ConnectionCloseData : WebPubSubEvent
    {
        public string ConnectionId { get; set; }
        public string Reason { get; set; }
    }
}
