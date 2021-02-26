using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class MessageData : WebPubSubEvent
    {
        public TargetType TargetType { get; set; } = TargetType.All;

        public string TargetId { get; set; }

        public string[] Excludes { get; set; }

        public string Message { get; set; }
    }
}
