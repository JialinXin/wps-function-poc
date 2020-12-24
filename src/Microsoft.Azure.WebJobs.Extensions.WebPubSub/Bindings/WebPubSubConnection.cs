using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubConnection
    {
        public string Url { get; set; }

        public string AccessToken { get; set; }
    }
}
