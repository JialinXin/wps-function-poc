using Azure.Messaging.WebPubSub;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebPubSub.AspNetCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace chatapp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddWebPubSub();
            services.AddWebPubSub(o => 
            {
                o.ServiceEndpoint = new ServiceEndpoint("Endpoint=http://localhost;Port=8080;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH;Version=1.0;");
            }).AddWebPubSubServiceClient<SampleHub>()
            .AddWebPubSubServiceClient<SampleHub1>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapWebPubSubHub<SampleHub>("/api/{event}");

                endpoints.MapGet("/negotiate", async context =>
                {
                    var id = context.Request.Query["id"];
                    if (id.Count != 1)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("missing user id");
                        return;
                    }
                    var serviceClient = context.RequestServices.GetService<WebPubSubServiceClient>();
                    if (serviceClient == null)
                    {
                        serviceClient = new WebPubSubServiceClient("Endpoint=http://localhost;Port=8080;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH;Version=1.0;", "samplehub");
                    }
                    await context.Response.WriteAsync(serviceClient.GetClientAccessUri(userId: id).AbsoluteUri);
                });
            });
        }
    }
}
