using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Azure.WebPubSub.Common;
using Newtonsoft.Json;
using System;
using System.IO;
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
            [WebPubSubConnection(UserId = "{query.userid}", Hub = "%abc%")] WebPubSubConnection connection)
        {
            Console.WriteLine("login");
            return connection;
        }

        #region Work with WebPubSubTrigger
        [FunctionName("connect")]
        public static WebPubSubEventResponse Connect(
            [WebPubSubTrigger("triggerchat", WebPubSubEventType.System, "connect")] ConnectEventRequest request)
        {
            Console.WriteLine($"Received client connect with connectionId: {request.ConnectionContext.ConnectionId}");
            if (request.ConnectionContext.UserId == "attacker")
            {
                return request.CreateErrorResponse(WebPubSubErrorCode.Unauthorized, null);
            }
            //return request.CreateResponse(request.ConnectionContext.UserId, null, null, null);
            return request.CreateResponse(request.ConnectionContext.UserId, null, null, null);
        }

        // multi tasks sample
        [FunctionName("connected")]
        public static async Task Connected(
            [WebPubSubTrigger("%abc%",WebPubSubEventType.System, "connected")] WebPubSubConnectionContext connectionContext,
            [WebPubSub(Hub = "%abc%")] IAsyncCollector<WebPubSubAction> webpubsubOperation)
        {
            await webpubsubOperation.AddAsync(new SendToAllAction
            {
                Data = BinaryData.FromString(new ClientContent($"{connectionContext.UserId} connected.").ToString()),
                DataType = WebPubSubDataType.Json
            });

            await webpubsubOperation.AddAsync(WebPubSubAction.CreateAddUserToGroupAction(connectionContext.UserId, "group1"));
            await webpubsubOperation.AddAsync(new SendToUserAction
            {
                UserId = connectionContext.UserId,
                Data = BinaryData.FromString(new ClientContent($"{connectionContext.UserId} joined group: group1.").ToString()),
                DataType = WebPubSubDataType.Json
            });
        }

        // single message sample
        [FunctionName("broadcast")]
        public static async Task<WebPubSubEventResponse> Broadcast(
            [WebPubSubTrigger("%abc%", WebPubSubEventType.User, "message")]
            UserEventRequest request,
            WebPubSubConnectionContext connectionContext,
            BinaryData data,
            WebPubSubDataType dataType,
            [WebPubSub(Hub = "triggerchat")] IAsyncCollector<WebPubSubAction> operations)
        {
            await operations.AddAsync(WebPubSubAction.CreateSendToAllAction(request.Data, request.DataType));

            return request.CreateResponse(BinaryData.FromString(new ClientContent("ack").ToString()), WebPubSubDataType.Json);
        }

        [FunctionName("disconnect")]
        [return: WebPubSub(Hub = "%abc%")]
        public static WebPubSubAction Disconnect(
            [WebPubSubTrigger("%abc%", WebPubSubEventType.System, "disconnected")] WebPubSubConnectionContext connectionContext)
        {
            Console.WriteLine("Disconnect.");
            return new SendToAllAction
            {
                Data = BinaryData.FromString(new ClientContent($"{connectionContext.UserId} disconnect.").ToString()),
                DataType = WebPubSubDataType.Text
            };
        }

        #endregion

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
