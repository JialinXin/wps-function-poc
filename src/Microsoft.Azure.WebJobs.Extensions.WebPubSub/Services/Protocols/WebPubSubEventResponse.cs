using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubEventResponse
    {
        public Error Error { get; set; }
    }

    public class ConnectResponse : WebPubSubEventResponse
    {
        public string UserId { get; set; }
        public string[] Groups { get; set; }
        public string Subprotocol { get; set; }
        public string[] Roles { get; set; }
    }

    public class MessageResponse : WebPubSubEventResponse
    {
        public Stream Message { get; set; }
        public MessageDataType DataType { get; set; }
    }
}
