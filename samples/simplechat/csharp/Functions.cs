using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace SimpleChat
{
    public static class Functions
    {
        [FunctionName("login")]
        public static WebPubSubConnection GetClientConnection(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [WebPubSubConnection(UserId = "{query.userid}")] WebPubSubConnection connection)
        {
            Console.WriteLine("login");
            return connection;
        }

        [FunctionName("connect")]
        public static ServiceResponse Connect(
            [WebPubSubTrigger("simplechat", EventType.System, "connect")] ConnectionContext connectionContext)
        {
            Console.WriteLine($"Received client connect with connectionId: {connectionContext.ConnectionId}");
            if (connectionContext.UserId == "attacker")
            {
                return new ErrorResponse(ErrorCode.Unauthorized);
            }
            return new ConnectResponse
            {
                UserId = connectionContext.UserId
            };
        }

        // multi tasks sample
        [FunctionName("connected")]
        public static async Task Connected(
            [WebPubSubTrigger(EventType.System, "connected")] ConnectionContext connectionContext,
            [WebPubSub] IAsyncCollector<WebPubSubEvent> webpubsubEvent)
        {
            await webpubsubEvent.AddAsync(new WebPubSubEvent
            {
                Message = new Message(new ClientContent($"{connectionContext.UserId} connected.").ToString()),
            });

            await webpubsubEvent.AddAsync(new WebPubSubEvent
            {
                Operation = Operation.AddUserToGroup,
                UserId = connectionContext.UserId,
                Group = "group1"
            });
            await webpubsubEvent.AddAsync(new WebPubSubEvent
            {
                Operation = Operation.SendToUser,
                UserId = connectionContext.UserId,
                Group = "group1",
                Message = new Message(new ClientContent($"{connectionContext.UserId} joined group: group1.").ToString()),
            });
        }

        // single message sample
        [FunctionName("broadcast")]
        public static async Task<MessageResponse> Broadcast(
            [WebPubSubTrigger(EventType.User, "message")] Message message,
            [WebPubSub(Hub = "simplechat")] IAsyncCollector<WebPubSubEvent> webpubsubEvent)
        {
            await webpubsubEvent.AddAsync(new WebPubSubEvent
            {
                Operation = Operation.SendToAll,
                Message = message,
            });

            return new MessageResponse
            {
                Message = new Message("ack"),
            };
        }

        [FunctionName("disconnect")]
        [return: WebPubSub]
        public static WebPubSubEvent Disconnect(
            [WebPubSubTrigger(EventType.System, "disconnected")] ConnectionContext connectionContext)
        {
            Console.WriteLine("Disconnect.");
            return new WebPubSubEvent
            {
                Operation = Operation.SendToAll,
                Message = new Message(new ClientContent($"{connectionContext.UserId} disconnect.").ToString())
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
