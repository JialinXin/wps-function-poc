using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChat
{
    public static class Functions
    {
        [FunctionName("login")]
        public static WebPubSubConnection GetClientConnection(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [WebPubSubConnection(UserId = "{query.userid}", Hub = "simplechat", CustomClaims = "{headers.x-claims}")] WebPubSubConnection connection)
        {
            Console.WriteLine("login");
            return connection;
        }

        [FunctionName("connect")]
        public static void Connect(
            [WebPubSubTrigger("simplechat","connect")]InvocationContext context)
        {
            Console.WriteLine($"Received client connect with connectionId: {context.ConnectionId}");
        }

        // multi tasks sample
        [FunctionName("connected")]
        public static async Task Connected(
            [WebPubSubTrigger("simplechat", "connected")] InvocationContext context,
            [WebPubSub] IAsyncCollector<WebPubSubEvent> eventHandler)
        {
            await eventHandler.AddAsync(new MessageEvent
            {
                Message = GetStream(new ClientContent($"{context.UserId} connected.").ToString()),
                DataType = MessageDataType.Json
            });

            await eventHandler.AddAsync(new GroupEvent
            {
                TargetType = TargetType.Users,
                TargetId = context.UserId,
                Action = GroupAction.Join,
                GroupId = "group1"
            });
            await eventHandler.AddAsync(new MessageEvent
            {
                Message = GetStream(new ClientContent($"{context.UserId} joined group: group1.").ToString()),
                DataType = MessageDataType.Json
            });
        }

        // single message sample
        [FunctionName("broadcast")]
        [return: WebPubSub]
        public static MessageEvent Broadcast(
            [WebPubSubTrigger("simplechat", "message", "user")] InvocationContext context,
            Stream message,
            MessageResponse messageResponse)
        {
            messageResponse.Message = new MemoryStream(Encoding.UTF8.GetBytes("received"));
            messageResponse.DataType = MessageDataType.Text;
            return new MessageEvent
            {
                Message = message,
                DataType = MessageDataType.Json
            };
        }

        [FunctionName("disconnect")]
        [return: WebPubSub]
        public static MessageEvent Disconnect(
            [WebPubSubTrigger("simplechat", "disconnected")] InvocationContext context)
        {
            Console.WriteLine("Disconnect.");
            return new MessageEvent
            {
                Message = GetStream(new ClientContent($"{context.UserId} disconnect.").ToString()),
                DataType = MessageDataType.Json
            };
        }

        private static Stream GetStream(string s)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(s));
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
