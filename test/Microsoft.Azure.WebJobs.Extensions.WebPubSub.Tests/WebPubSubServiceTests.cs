using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;
using NUnit.Framework;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub.Tests
{
    public class WebPubSubServiceTests
    {
        private const string NormConnectionString = "Endpoint=http://localhost;Port=8080;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH;Version=1.0;";
        private const string SecConnectionString = "Endpoint=https://abc;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH;Version=1.0;";

        [TestCase(NormConnectionString)]
        [TestCase(SecConnectionString)]
        public void TestWebPubSubConnection(string connectionString)
        {
            var service = new WebPubSubService(connectionString, "testHub");

            var clientConnection = service.GetClientConnection();

            Assert.NotNull(clientConnection);
        }
    }
}
