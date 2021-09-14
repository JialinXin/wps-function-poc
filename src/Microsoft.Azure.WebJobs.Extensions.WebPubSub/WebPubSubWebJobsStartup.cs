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
        }
    }
}
