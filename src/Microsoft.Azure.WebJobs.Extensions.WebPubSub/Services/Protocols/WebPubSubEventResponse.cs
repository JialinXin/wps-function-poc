using System.IO;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class ConnectResponse
    {
        public Error Error { get; set; }
        public string UserId { get; set; }
        public string[] Groups { get; set; }
        public string Subprotocol { get; set; }
        public string[] Roles { get; set; }
    }

    public class MessageResponse
    {
        public Error Error { get; set; }
        public Stream Message { get; set; }
        public MessageDataType DataType { get; set; }
    }
}
