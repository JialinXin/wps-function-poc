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
        [FunctionName("login")]
        public static WebPubSubConnection GetClientConnection(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [WebPubSubConnection(HubName = "simplechat", UserId = "{query.userid}")] WebPubSubConnection connection,
            ILogger log)
        {
            Console.WriteLine("login");
            return connection;
        }

        [FunctionName("connect")]
        public static void Connect(
            [WebPubSubTrigger("simplechat", "connections", "connect")]InvocationContext context)
        {
            Console.WriteLine($"{context.ConnectionId}");
            Console.WriteLine("Connect.");
        }

        //[FunctionName("connect")]
        //public static void Connect(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        //    ILogger log)
        //{
        //    Console.WriteLine("Connect.");
        //}

        [FunctionName("chat")]
        public static Task Broadcast(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [WebPubSub(HubName = "simplechat")] IAsyncCollector<MessageData> messages)
        {
            var data = new StreamReader(req.Body);
            var msg = new MessageData();
            msg.Message = data.ReadToEnd();
            return messages.AddAsync(msg);
        }

        [FunctionName("disconnect")]
        public static void Disconnect(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            Console.WriteLine("Disconnect.");
        }
    }
}
