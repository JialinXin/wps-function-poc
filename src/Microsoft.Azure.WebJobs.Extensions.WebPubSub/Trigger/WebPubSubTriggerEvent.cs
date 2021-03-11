using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubTriggerEvent
    {
        /// <summary>
        /// Web PubSub Context that gets from HTTP request and pass the Function parameters
        /// </summary>
        public ConnectionContext Context { get; set; }

        public byte[] Payload { get; set; }

        public MessageDataType DataType { get; set; }

        public string[] Subprotocols { get; set; }

        public IDictionary<string, string[]> Claims { get; set; }

        public string Reason { get; set; }

        /// <summary>
        /// A TaskCompletionSource will set result when the function invocation has finished.
        /// </summary>
        public TaskCompletionSource<object> TaskCompletionSource { get; set; }
    }
}
