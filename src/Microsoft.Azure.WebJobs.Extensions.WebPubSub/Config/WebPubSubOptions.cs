using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubOptions
    {
        public string ConnectionString { get; set; }

        public string Hub { get; set; }

        /// <summary>
        /// Allowed Hosts for Abuse Protection. All service connection strings will be added.
        /// User can add customized values from function settings using comma to separate multiple values.
        /// </summary>
        public HashSet<string> AllowedHosts { get; set; } = new HashSet<string>();

        internal HashSet<string> AccessKeys { get; set; } = new HashSet<string>();
    }
}
