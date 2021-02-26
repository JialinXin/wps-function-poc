using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public static class InvocationContextExtensions
    {
        public static HttpResponseMessage BuildConnectResponse(this InvocationContext context)
        {
            HttpResponseMessage response = new HttpResponseMessage(context.StatusCode);
            //var headers = new Dictionary<string, StringValues>();
            //if (context.Headers.Count > 0)
            //{
            //    foreach (var header in context.Headers)
            //    {
            //        headers.Add(header.Key, new StringValues(header.Value));
            //    }
            //}
            var connectEvent = new ConnectEventResponse 
            {
                UserId = context.UserId,
                Headers = context.Headers,
                Groups = context.Groups,
                Roles = context.Roles,
            };

            response.Content = new StringContent(JsonConvert.SerializeObject(connectEvent));

            return response;
        }
    }
}
