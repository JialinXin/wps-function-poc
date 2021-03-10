using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class MessageEvent : WebPubSubEvent
    {
        public TargetType TargetType { get; set; } = TargetType.All;

        public string TargetId { get; set; }

        public string[] Excludes { get; set; }

        public Stream Message { get; set; }

        public MessageDataType DataType { get; set; }
    }
}
