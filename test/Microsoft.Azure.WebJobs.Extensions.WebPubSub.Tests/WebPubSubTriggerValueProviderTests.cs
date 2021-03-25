﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub.Tests
{
    public class WebPubSubTriggerValueProviderTests
    {
        [Theory]
        [InlineData("connectioncontext")]
        [InlineData("reason")]
        [InlineData("message")]
        [InlineData("datatype")]
        public void TestGetValueByName(string name)
        {
            var triggerEvent = new WebPubSubTriggerEvent
            {
                ConnectionContext = new ConnectionContext
                {
                    ConnectionId = "000000",
                    Event = "message",
                    Type = "user",
                    Hub = "testhub",
                    UserId = "user1"
                },
                Reason = "reason",
                Message = new WebPubSubMessage("message"),
                DataType = MessageDataType.Text
            };

            //var value = triggerEvent.GetType().GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase).GetValue(triggerEvent);
            var ttt = typeof(WebPubSubTriggerEvent).GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var value = ttt.GetValue(triggerEvent);
            Assert.NotNull(value);
        }

        [Fact]
        public void TestGetValueByName_Invalid()
        {
            var properties = typeof(WebPubSubTriggerEvent)
                    .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            string names = string.Empty;
            foreach (var property in properties)
            {
                names += property.Name + ";";
            }
            Console.WriteLine(names);
        }
    }
}
