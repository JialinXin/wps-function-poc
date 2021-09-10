// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public static class WebPubSubJobsBuilderExtensions
    {
        public static IWebJobsBuilder AddWebPubSub(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<WebPubSubConfigProvider>()
                .ConfigureOptions<WebPubSubOptions>(ApplyConfiguration);

            return builder;
        }

        private static void SetResultHook(HttpRequest request, object result)
        {
            //request.HttpContext.Response.Headers["hello"] = "world";
        }

        private static void ApplyHttpConfiguration(IConfiguration config, HttpOptions options)
        {
            //var test = false;
            //if (test)
            //{
                options.SetResponse = (request, result) =>
                {
                    var jResult = JObject.FromObject(result);
                    var streamResult = new MemoryStream(Encoding.UTF8.GetBytes(jResult.ToString()));
                    //var response = new HttpResponseMessage
                    request = BuildRequest(request, result);
                };
            //}
            //options.SetResponse = SetResultHook;
        }

        private static void ApplyConfiguration(IConfiguration config, WebPubSubOptions options)
        {
            if (config == null)
            {
                return;
            }

            config.Bind(options);
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
