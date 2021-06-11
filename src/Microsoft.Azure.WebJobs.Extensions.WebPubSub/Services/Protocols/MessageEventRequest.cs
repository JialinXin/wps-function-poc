using System;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public sealed class MessageEventRequest : ServiceRequest
    {
        public BinaryData Message { get; set; }
        public MessageDataType DataType { get; set; }
    }
}
