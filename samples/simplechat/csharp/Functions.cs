using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleChat
{
    public static class Functions
    {
        [FunctionName("index")]
        public static IActionResult Home([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req)
        {
            return new ContentResult
            {
                Content = File.ReadAllText("index.html"),
                ContentType = "text/html",
            };
        }

        [FunctionName("login")]
        public static WebPubSubConnection GetClientConnection(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [WebPubSubConnection(UserId = "{query.userid}")] WebPubSubConnection connection)
        {
            Console.WriteLine("login");
            return connection;
        }

        #region Work with HttpTrigger

        [FunctionName("abuse-connect")]
        public static HttpResponseMessage AbuseProtectionForConnect(
            [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "connect")] HttpRequest req,
            [WebPubSubRequest] AbuseProtector protector)
        {
            return protector.Response;
        }

        [FunctionName("connect")]
        public static ConnectResponse ConnectV2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [WebPubSubRequest] ConnectionContext context)
        {
            var response = new ConnectResponse
            {
                UserId = context.UserId
            };
            return response;
        }

        [FunctionName("abuse-broadcast")]
        public static HttpResponseMessage AbuseProtectionForBroadcast(
            [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "broadcast")] HttpRequest req)
        {
            req.Headers.TryGetValue("WebHook-Request-Origin", out var hosts);
            var response = new HttpResponseMessage();
            response.Headers.Add("WebHook-Allowed-Origin", hosts.ToString());
            return response;
        }

        // Http Trigger Message
        [FunctionName("broadcast")]
        public static async Task<ClientContent> Broadcast(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [WebPubSubRequest] ConnectionContext context,
            [WebPubSub(Hub = "simplechat")] IAsyncCollector<WebPubSubOperation> operations)
        {
            await operations.AddAsync(new SendToAll
            {
                Message = BinaryData.FromStream(req.Body),
                DataType = MessageDataType.Json
            });

            return new ClientContent("ack");
        }
        #endregion

        #region Work with WebPubSubTrigger
        //public static ServiceResponse Connect(
        //    [WebPubSubTrigger("simplechat", WebPubSubEventType.System, "connect")] ConnectionContext connectionContext)
        //{
        //    Console.WriteLine($"Received client connect with connectionId: {connectionContext.ConnectionId}");
        //    if (connectionContext.UserId == "attacker")
        //    {
        //        return new ErrorResponse(WebPubSubErrorCode.Unauthorized);
        //    }
        //    return new ConnectResponse
        //    {
        //        UserId = connectionContext.UserId
        //    };
        //}

        // multi tasks sample
        [FunctionName("connected")]
        public static async Task Connected(
            [WebPubSubTrigger(WebPubSubEventType.System, "connected")] ConnectionContext connectionContext,
            [WebPubSub] IAsyncCollector<WebPubSubOperation> webpubsubOperation)
        {
            await webpubsubOperation.AddAsync(new SendToAll
            {
                Message = BinaryData.FromString(new ClientContent($"{connectionContext.UserId} connected.").ToString()),
                DataType = MessageDataType.Json
            });

            await webpubsubOperation.AddAsync(new AddUserToGroup
            {
                UserId = connectionContext.UserId,
                Group = "group1"
            });
            await webpubsubOperation.AddAsync(new SendToUser
            {
                UserId = connectionContext.UserId,
                Message = BinaryData.FromString(new ClientContent($"{connectionContext.UserId} joined group: group1.").ToString()),
                DataType = MessageDataType.Json
            });
        }

        #endregion


        

        // single message sample
        //[FunctionName("broadcast")]
        //public static async Task<MessageResponse> Broadcast(
        //    [WebPubSubTrigger(WebPubSubEventType.User, "message")] ConnectionContext context,
        //    BinaryData message,
        //    MessageDataType dataType,
        //    [WebPubSub(Hub = "simplechat")] IAsyncCollector<WebPubSubOperation> operations)
        //{
        //    await operations.AddAsync(new SendToAll
        //    {
        //        Message = message,
        //        DataType = dataType
        //    });
        //
        //    return new MessageResponse
        //    {
        //        Message = BinaryData.FromString(new ClientContent("ack").ToString()),
        //        DataType = MessageDataType.Json
        //    };
        //}

        [FunctionName("disconnect")]
        [return: WebPubSub]
        public static WebPubSubOperation Disconnect(
            [WebPubSubTrigger(WebPubSubEventType.System, "disconnected")] ConnectionContext connectionContext)
        {
            Console.WriteLine("Disconnect.");
            return new SendToAll
            {
                Message = BinaryData.FromString(new ClientContent($"{connectionContext.UserId} disconnect.").ToString()),
                DataType = MessageDataType.Text
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
