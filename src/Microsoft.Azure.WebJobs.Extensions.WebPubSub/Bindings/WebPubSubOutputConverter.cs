using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubOutputConverter
    {
        // We accept multiple output binding types and rely on them to determine rest api actions
        // But in non .NET language, it's not able to convert JObject to different types
        // So need a converter to accurate convert JObject to acceptable data object
        //public object ConvertToWebPubSubData(object input)
        //{
        //    if (input.GetType() != typeof(JObject))
        //    {
        //        return input;
        //    }
        //
        //    var jobject = input as JObject;
        //
        //    if (jobject.TryConvert<MessageEvent>(out var message))
        //    {
        //        return message;
        //    }
        //
        //    if (jobject.TryConvert<GroupEvent>(out var groupData))
        //    {
        //        return groupData;
        //    }
        //
        //    if (jobject.TryConvert<ExistenceEvent>(out var existenceData))
        //    {
        //        return existenceData;
        //    }
        //
        //    if (jobject.TryConvert<ConnectionCloseData>(out var closeData))
        //    {
        //        return closeData;
        //    }
        //
        //    throw new ArgumentException("Unable to convert JObject to valid output binding type, check parameters.");
        //}
    }
}
