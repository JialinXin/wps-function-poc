using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChat
{
    public static class Functions
    {
        //private const string Hub = "simplechat";

        [FunctionName("login")]
        public static WebPubSubConnection GetClientConnection(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [WebPubSubConnection(UserId = "{query.userid}", Hub = "simplechat")] WebPubSubConnection connection)
        {
            Console.WriteLine("login");
            return connection;
        }

        [FunctionName("simplechat-connect")]
        public static void Connect(
            [WebPubSubTrigger]InvocationContext context)
        {
            Console.WriteLine($"Received client connect with connectionId: {context.ConnectionId}");
        }

        // multi tasks sample
        [FunctionName("simplechat-connected")]
        public static async Task Connected(
            [WebPubSubTrigger] InvocationContext context,
            [WebPubSub] IAsyncCollector<WebPubSubEvent> eventHandler)
        {

            await eventHandler.AddAsync(new MessageData
            {
                Message = new ClientContent($"{context.UserId} connected.").ToString()
            });

            await eventHandler.AddAsync(new GroupData
            {
                TargetType = TargetType.Users,
                TargetId = context.UserId,
                Action = GroupAction.Add,
                GroupId = "group1"
            });
            await eventHandler.AddAsync(new MessageData
            {
                Message = new ClientContent($"{context.UserId} joined group: group1.").ToString()
            });
        }

        // single message sample
        [FunctionName("simplechat-message")]
        [return: WebPubSub]
        public static MessageData Broadcast(
            [WebPubSubTrigger] InvocationContext context)
        {
            return new MessageData
            {
                Message = Encoding.UTF8.GetString(context.Payload.Span)
            };
        }

        [FunctionName("simplechat-disconnect")]
        [return: WebPubSub]
        public static MessageData Disconnect(
            [WebPubSubTrigger] InvocationContext context)
        {
            Console.WriteLine("Disconnect.");
            return new MessageData
            {
                Message = new ClientContent($"{context.UserId} disconnect.").ToString()
            };
        }

        [JsonObject]
        public sealed class ClientContent
        {
            [JsonProperty("from")]
            public string From { get; set; }
            [JsonProperty("content")]
            public string Content { get; set; }

            public ClientContent(string message)
            {
                From = "[System]";
                Content = message;
            }

            public ClientContent(string from, string message)
            {
                From = from;
                Content = message;
            }

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }
    }
}
