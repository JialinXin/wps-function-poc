namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class ConnectionCloseData
    {
        public string ConnectionId { get; set; }
        public string Reason { get; set; }
    }
}
