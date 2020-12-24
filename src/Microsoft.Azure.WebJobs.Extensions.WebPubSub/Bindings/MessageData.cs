using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class MessageData
    {
        public TargetType TargetType { get; set; } = TargetType.All;

        public string TargetId { get; set; }

        public string[] Excludes { get; set; }

        public object Message { get; set; }
    }
}
