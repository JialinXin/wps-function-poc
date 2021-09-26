using Microsoft.Azure.WebPubSub.AspNetCore;
using System;
using System.Threading.Tasks;

namespace chatapp
{
    public class TestHub : ServiceHub
    {
        public override Task<ServiceResponse> Connect(ConnectEventRequest request)
        {
            throw new NotImplementedException();
        }

        public override Task<ServiceResponse> Message(MessageEventRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
