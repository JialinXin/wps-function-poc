using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class ClientConnectResponseMessage
    {
        public string ConnectionId { get; set; }
        public string[] Groups { get; set; }
        public string UserId { get; set; }
        public int Code { get; set; }
        public Error Error { get; set; }
        public string Subprotocol { get; set; }
    }
}
