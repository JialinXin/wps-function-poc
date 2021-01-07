using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubOutputConverter
    {
        private readonly JObjectToTypeConverter<MessageData> _messageConverter;
        private readonly JObjectToTypeConverter<GroupData> _groupConverter;
        private readonly JObjectToTypeConverter<ExistenceData> _existenceConverter;
        private readonly JObjectToTypeConverter<ConnectionCloseData> _closeConvert;

        public WebPubSubOutputConverter()
        {
            _messageConverter = new JObjectToTypeConverter<MessageData>();
            _groupConverter = new JObjectToTypeConverter<GroupData>();
            _existenceConverter = new JObjectToTypeConverter<ExistenceData>();
            _closeConvert = new JObjectToTypeConverter<ConnectionCloseData>();
        }

        // We accept multiple output binding types and rely on them to determine rest api actions
        // But in non .NET language, it's not able to convert JObject to different types
        // So need a converter to accurate convert JObject to acceptable data object
        public object ConvertToWebPubSubOutput(object input)
        {
            if (input.GetType() != typeof(JObject))
            {
                return input;
            }

            var jobject = input as JObject;

            if (_messageConverter.TryConvert(jobject, out var message))
            {
                return message;
            }

            if (_groupConverter.TryConvert(jobject, out var groupData))
            {
                return groupData;
            }

            if (_existenceConverter.TryConvert(jobject, out var existenceData))
            {
                return existenceData;
            }

            if (_closeConvert.TryConvert(jobject, out var closeData))
            {
                return closeData;
            }

            throw new ArgumentException("Unable to convert JObject to valid output binding type, check parameters.");
        }
    }
}
