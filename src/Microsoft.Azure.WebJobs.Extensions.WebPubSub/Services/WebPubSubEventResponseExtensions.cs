using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public static class WebPubSubEventResponseExtensions
    {
        public static HttpResponseMessage BuildResponse(this WebPubSubEventResponse eventResponse)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            if (eventResponse.Error != null)
            {
                response.StatusCode = eventResponse.Error.Code == ErrorCode.Unauthorized ? HttpStatusCode.Unauthorized : HttpStatusCode.BadRequest;
                response.Content = new StringContent(eventResponse.Error.Message);
            }
            else if (eventResponse is ConnectResponse connect)
            {
                var connectEvent = new ConnectEventResponse
                {
                    UserId = connect.UserId,
                    Groups = connect.Groups,
                    Subprotocol = connect.Subprotocol
                };
                response.Content = new StringContent(JsonConvert.SerializeObject(connectEvent));
            }
            else if (eventResponse is MessageResponse message && message.Message != null)
            {
                response.Content = new StreamContent(message.Message);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(Utilities.GetContentType(message.DataType));
            }
        
            return response;
        }
    }
}
