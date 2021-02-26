using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleChat
{
    public static class Functions
    {
        [FunctionName("login")]
        public static WebPubSubConnection GetClientConnection(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [WebPubSubConnection(UserId = "{query.userid}")] WebPubSubConnection connection,
            ILogger log)
        {
            Console.WriteLine("login");
            return connection;
        }

        [FunctionName("Connect")]
        public static void Connect(
            [WebPubSubTrigger]InvocationContext context)
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

        [FunctionName("message")]
        [return: WebPubSub()]
        public static MessageData Broadcast(
            [WebPubSubTrigger] InvocationContext context)
        {
            return new MessageData
            {
                Message = System.Text.Encoding.UTF8.GetString(context.Payload.Span)
            };
        }

        [FunctionName("disconnect")]
        public static void Disconnect(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            Console.WriteLine("Disconnect.");
        }
    }
}
