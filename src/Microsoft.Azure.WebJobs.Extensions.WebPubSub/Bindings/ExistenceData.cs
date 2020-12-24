using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class ExistenceData
    {
        public TargetType TargetType { get; set; }
        public string TargetId { get; set; }
    }
}
