// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebPubSub.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;

[assembly: WebJobsStartup(typeof(WebPubSubWebJobsStartup))]
namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddWebPubSub();

            builder.AddHttp(o =>
            {
                o.SetResponse = (request, result) =>
                {
                    if (result is ConnectResponse response)
                    {
                        request.Headers.Add("test", "test");
                    }
                    return;
                };
            });
        }

        private static HttpRequest BuildRequest(HttpRequest request, object result)
        {
            var context = new DefaultHttpContext();
            var resFeature = context.Request.HttpContext.Features.Get<IHttpResponseFeature>();

            var headers = new HeaderDictionary();

            foreach (var header in request.HttpContext.Response.Headers)
            {
                headers.Add(header.Key, header.Value);
            }
            headers.Add("test", "value");

            var jResult = JObject.FromObject(result);
            var streamResult = new MemoryStream(Encoding.UTF8.GetBytes(jResult.ToString()));
            resFeature.Body = streamResult;
            context.Response.ContentLength = jResult.ToString().Length;
            headers["Content-Length"] = jResult.ToString().Length.ToString();

            resFeature.Headers = headers;

            return context.Request;
        }
    }
}
