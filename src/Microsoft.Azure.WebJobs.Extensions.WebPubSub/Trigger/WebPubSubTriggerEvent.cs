using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubTriggerEvent
    {
        /// <summary>
        /// Web PubSub Context that gets from HTTP request and pass the Function parameters
        /// </summary>
        public InvocationContext Context { get; set; }

        /// <summary>
        /// A TaskCompletionSource will set result when the function invocation has finished.
        /// </summary>
        public TaskCompletionSource<object> TaskCompletionSource { get; set; }
    }
}
