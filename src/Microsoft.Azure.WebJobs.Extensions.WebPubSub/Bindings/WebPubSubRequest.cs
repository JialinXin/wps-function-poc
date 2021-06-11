using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubRequest
    {
        public ConnectionContext ConnectionContext { get; internal set; }

        public SystemRequest Request { get; internal set; }

        public bool IsValid { get; internal set; }

        public bool IsAbuseRequest { get; internal set; } = false;

        public HttpResponseMessage Response { get; set; }

        internal WebPubSubRequest(ConnectionContext context, HashSet<string> accessKeys)
        {
            ConnectionContext = context;
            IsValid = Utilities.ValidateSignature(context.ConnectionId, context.Signature, accessKeys);
            Response = IsValid ? new HttpResponseMessage() : new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        }

        internal WebPubSubRequest(bool isAbuse, bool isValid, HttpResponseMessage response)
        {
            IsAbuseRequest = isAbuse;
            IsValid = isValid;
            Response = response;
        }
    }
}
