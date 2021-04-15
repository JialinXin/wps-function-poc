﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub.Tests
{
    public class WebPubSubTriggerValueProviderTests
    {
        [Theory]
        [InlineData("connectioncontext")]
        [InlineData("reason")]
        [InlineData("message")]
        public void TestGetValueByName(string name)
        {
            var triggerEvent = new WebPubSubTriggerEvent
            {
                ConnectionContext = new ConnectionContext
                {
                    ConnectionId = "000000",
                    EventName = "message",
                    EventType = "user",
                    Hub = "testhub",
                    UserId = "user1"
                },
                Reason = "reason",
                Message = new WebPubSubMessage("message"),
            };

            //var value = triggerEvent.GetType().GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase).GetValue(triggerEvent);
            var ttt = typeof(WebPubSubTriggerEvent).GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var value = ttt.GetValue(triggerEvent);
            Assert.NotNull(value);
        }
    }
}