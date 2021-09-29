using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebPubSub.AspNetCore.Tests.Samples
{
    class WebPubSubSample
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseWebPubSub(builder =>
            {
                builder.MapWebPubSubHub<SampleHub>("/eventhander");
            });
        }
    }
}
