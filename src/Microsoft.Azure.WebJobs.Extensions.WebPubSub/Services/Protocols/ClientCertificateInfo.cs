using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal sealed class ClientCertificateInfo
    {
        [JsonProperty("thumbprint")]
        public string Thumbprint { get; }
    }
}
