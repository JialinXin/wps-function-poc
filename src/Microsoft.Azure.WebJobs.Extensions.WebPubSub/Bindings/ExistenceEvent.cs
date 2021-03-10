namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class ExistenceEvent : WebPubSubEvent
    {
        public TargetType TargetType { get; set; }
        public string TargetId { get; set; }
    }
}
