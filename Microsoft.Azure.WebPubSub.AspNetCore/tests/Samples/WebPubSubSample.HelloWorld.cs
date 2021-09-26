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
                builder.MapHub("/eventhander", new SampleHub());
            },
            options =>
            {
                options = new WebPubSubValidationOptions("<connection-string1>", "<connection-string2");
            });
        }
    }
}
