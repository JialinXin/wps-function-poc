using System;

using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [AttributeUsage(AttributeTargets.ReturnValue)]
    [Binding]
    public class ConnectResponseAttribute : Attribute
    {
    }
}
