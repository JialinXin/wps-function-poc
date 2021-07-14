using System;
using System.Threading.Tasks;
using Azure.Messaging.WebPubSub;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Extensions.Logging;

namespace notifications
{
    public static class notification
    {
        [FunctionName("notification")]
        public static async Task Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log,
            [WebPubSub(Hub = "notification")] IAsyncCollector<WebPubSubOperation> operations)
        {
           await operations.AddAsync(new SendToAll
            {
                Message = BinaryData.FromString($"[DateTime: {DateTime.Now}], MSFT stock price: {GetStockPrice()}"),
                DataType = MessageDataType.Text
            });
        }

        private static double GetStockPrice()
        {
            var rng = new Random();
            return 270 + 1.0 / 100 * rng.Next(-500, 500);
        }
    }
}
