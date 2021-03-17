using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace SimpleChat
{
    public static class Functions
    {
        [FunctionName("login")]
        public static WebPubSubConnection GetClientConnection(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [WebPubSubConnection(UserId = "{query.userid}", Hub = "simplechat")] WebPubSubConnection connection)
        {
            Console.WriteLine("login");
            return connection;
        }

        [FunctionName("connect")]
        public static ConnectResponse Connect(
            [WebPubSubTrigger("simplechat","connect")]ConnectionContext context,
            string[] subprotocols)
        {
            Console.WriteLine($"Received client connect with connectionId: {context.ConnectionId}");
            return new ConnectResponse
            {
                UserId = context.UserId,
                Subprotocol = subprotocols.FirstOrDefault()
            };
        }

        // multi tasks sample
        [FunctionName("connected")]
        public static async Task Connected(
            [WebPubSubTrigger("simplechat", "connected")] ConnectionContext context,
            [WebPubSub] IAsyncCollector<WebPubSubEvent> eventHandler)
        {
            await eventHandler.AddAsync(new WebPubSubEvent
            {
                Operation = WebPubSubOperation.SendToAll,
                Message = GetStream(new ClientContent($"{context.UserId} connected.").ToString()),
                DataType = MessageDataType.Json
            });

            await eventHandler.AddAsync(new WebPubSubEvent
            {
                Operation = WebPubSubOperation.AddUserToGroup,
                UserId = context.UserId,
                GroupId = "group1"
            });
            await eventHandler.AddAsync(new WebPubSubEvent
            {
                Operation = WebPubSubOperation.SendToUser,
                UserId = context.UserId,
                GroupId = "group1",
                Message = GetStream(new ClientContent($"{context.UserId} joined group: group1.").ToString()),
                DataType = MessageDataType.Json
            });
        }

        // single message sample
        [FunctionName("broadcast")]
        public static async Task<MessageResponse> Broadcast(
            [WebPubSubTrigger("simplechat", "message", "user")] ConnectionContext context,
            Stream message,
            [WebPubSub] IAsyncCollector<WebPubSubEvent> eventHandler)
        {
            await eventHandler.AddAsync(new WebPubSubEvent
            {
                Operation = WebPubSubOperation.SendToAll,
                Message = message,
                DataType = MessageDataType.Text
            });

            return new MessageResponse
            {
                Message = GetStream(new ClientContent($"ack").ToString()),
                DataType = MessageDataType.Json
            };
        }

        [FunctionName("disconnect")]
        [return: WebPubSub]
        public static WebPubSubEvent Disconnect(
            [WebPubSubTrigger("simplechat", "disconnected")] ConnectionContext context)
        {
            Console.WriteLine("Disconnect.");
            return new WebPubSubEvent
            {
                Operation = WebPubSubOperation.SendToAll,
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
