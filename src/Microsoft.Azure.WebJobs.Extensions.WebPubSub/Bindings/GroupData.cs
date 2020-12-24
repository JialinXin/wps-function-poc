using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class GroupData
    {
        public GroupAction Action { get; set; }

        public TargetType TargetType { get; set; }

        // userId or connectionId
        public string TargetId { get; set; }

        public string GroupId { get; set; }
    }
}
