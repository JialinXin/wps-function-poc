using System;
//using Microsoft.AspNetCore.Connections;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{ 
    public class ClientPayloadResponseMessage
    {
        public Error Error { get; set; }
        public string ConnectionId { get; set; }
        //public TransferFormat Format { get; set; }
        public ReadOnlyMemory<byte> Payload { get; set; }
    }
}
