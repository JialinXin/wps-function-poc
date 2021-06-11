using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubRequestBinding : BindingBase<WebPubSubRequestAttribute>
    {
        private const string HttpRequestName = "$request";
        private readonly Type _userType;
        private readonly INameResolver _nameResolver;
        private readonly WebPubSubOptions _options;

        public WebPubSubRequestBinding(
            BindingProviderContext context,
            IConfiguration configuration,
            INameResolver nameResolver,
            WebPubSubOptions options) : base (context, configuration, nameResolver)
        {
            _userType = context.Parameter.ParameterType;
            _options = options;
            _nameResolver = nameResolver;
        }

        protected async override Task<IValueProvider> BuildAsync(WebPubSubRequestAttribute attrResolved, IReadOnlyDictionary<string, object> bindingData)
        {
            if (bindingData == null)
            {
                throw new ArgumentNullException(nameof(bindingData));
            }
            bindingData.TryGetValue(HttpRequestName, out var requestObj);
            var request = requestObj as HttpRequest;

            var httpContext = request?.HttpContext;

            // Build abuse response
            if (httpContext.Request.Method.Equals("options", StringComparison.OrdinalIgnoreCase) && 
                httpContext.Request.Headers.TryGetValue(Constants.Headers.WebHookRequestOrigin, out var values))
            {
                Utilities.RespondToServiceAbuseCheck(httpContext.Request.Method, values, _options.AllowedHosts, out var response);
                return new WebPubSubRequestValueProvider(new WebPubSubRequest(true, true, response), _userType, string.Empty);
            }

            // Build service event context
            if (!TryParseRequest(request, out var connectionContext))
            {
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                return new WebPubSubRequestValueProvider(new WebPubSubRequest(false, false, response), _userType, string.Empty);
            }

            var wpsRequest = new WebPubSubRequest(connectionContext, _options.AccessKeys);
            if (!wpsRequest.IsValid)
            {
                return new WebPubSubRequestValueProvider(wpsRequest, _userType, string.Empty);
            }

            var requestType = Utilities.GetRequestType(connectionContext.EventType, connectionContext.EventName);
            //var length = (int)request.ContentLength;
            //var payload = new MemoryStream();
            //await request?.Body.CopyToAsync(payload, length);

            //wpsRequest.Request = JObject.Parse(Encoding.UTF8.GetString(paylod, 0, length));
            switch (requestType)
            {
                case RequestType.Connect:
                    using (var sr = new StreamReader(request.Body))
                    {
                        var content = await sr.ReadToEndAsync();
                        wpsRequest.Request = JsonConvert.DeserializeObject<ConnectEventRequest>(content);
                        sr.Close();
                    }
                    break;
                case RequestType.Disconnect:
                    using (var sr = new StreamReader(request.Body))
                    {
                        var content = sr.ReadToEnd();
                        wpsRequest.Request = JsonConvert.DeserializeObject<DisconnectEventRequest>(content);
                    }
                    break;
                case RequestType.User:
                    var contentType = MediaTypeHeaderValue.Parse(request.ContentType);
                    if (!Utilities.ValidateContentType(contentType.MediaType, out var dataType))
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                        return new WebPubSubRequestValueProvider(new WebPubSubRequest(false, false, response), _userType, string.Empty);
                    }
                    wpsRequest.Request = new MessageEventRequest
                    {
                        Message = BinaryData.FromStream(request.Body),
                        DataType = dataType
                    };
                    break;
                default:
                    break;
            }
            return new WebPubSubRequestValueProvider(wpsRequest, _userType, string.Empty);
        }

        private static bool TryParseRequest(HttpRequest request, out ConnectionContext context)
        {
            // ConnectionId is required in upstream request, and method is POST.
            if (!request.Headers.ContainsKey(Constants.Headers.CloudEvents.ConnectionId)
                || !request.Method.Equals("post", StringComparison.OrdinalIgnoreCase))
            {
                context = null;
                return false;
            }

            context = new ConnectionContext();
            try
            {
                context.ConnectionId = GetValueOrDefault(request.Headers, Constants.Headers.CloudEvents.ConnectionId);
                context.Hub = GetValueOrDefault(request.Headers, Constants.Headers.CloudEvents.Hub);
                context.EventType = Utilities.GetEventType(GetValueOrDefault(request.Headers, Constants.Headers.CloudEvents.Type));
                context.EventName = GetValueOrDefault(request.Headers, Constants.Headers.CloudEvents.EventName);
                context.Signature = GetValueOrDefault(request.Headers, Constants.Headers.CloudEvents.Signature);
                //context.Headers = request.Headers //request.Headers.ToDictionary(x => x.Key, v => new StringValues(v.Value.ToArray()), StringComparer.OrdinalIgnoreCase);
                context.Headers = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();
                foreach (var item in request.Headers)
                {
                    context.Headers.Add(item.Key, item.Value);
                }


                // UserId is optional, e.g. connect
                if (request.Headers.ContainsKey(Constants.Headers.CloudEvents.UserId))
                {
                    context.UserId = GetValueOrDefault(request.Headers, Constants.Headers.CloudEvents.UserId);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static string GetValueOrDefault(IHeaderDictionary header, string key)
        {
            header.TryGetValue(key, out var value);
            return value.Count > 0 ? value[0] : string.Empty;
        }
    }

}
