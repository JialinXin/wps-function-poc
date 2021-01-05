using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SimpleChat
{
    public static class Functions
    {
        [FunctionName("connect")]
        public static WebPubSubConnection Connect(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [WebPubSubConnection(HubName = "simplechat", UserId = "User aaa")] WebPubSubConnection connection,
            ILogger log)
        {
            return connection;
        }

        [FunctionName("chat")]
        public static Task Broadcast(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [WebPubSub(HubName = "simplechat")] IAsyncCollector<MessageData> messages)
        {
            var data = new JsonSerializer().Deserialize<ClientData>(new JsonTextReader(new StreamReader(req.Body)));
            var msg = new MessageData();
            msg.Message = data.Content;
            return messages.AddAsync(msg);
        }

        [FunctionName("disconnect")]
        public static void Disconnect(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            Console.WriteLine("Disconnect.");
        }

        private sealed class ClientData
        {
            public string From { get; set; }
            public string Content { get; set; }
        }
    }
}
