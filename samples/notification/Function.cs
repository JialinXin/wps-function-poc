using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Azure.WebPubSub.Common;
using Microsoft.Extensions.Logging;

namespace notifications
{
    public static class Function
    {
        [FunctionName("notification")]
        public static async Task Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log,
            [WebPubSub(Hub = "notification")] IAsyncCollector<WebPubSubAction> operations)
        {
           await operations.AddAsync(new SendToAllAction
            {
                Data = BinaryData.FromString($"[DateTime: {DateTime.Now}], MSFT stock price: {GetStockPrice()}"),
                DataType = WebPubSubDataType.Text
            });
        }

        private static double GetStockPrice()
        {
            var rng = new Random();
            return 270 + 1.0 / 100 * rng.Next(-500, 500);
        }
    }
}
