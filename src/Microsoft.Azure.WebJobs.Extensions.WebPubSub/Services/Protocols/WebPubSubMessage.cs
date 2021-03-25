using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    //[JsonObject]
    public class WebPubSubMessage
    {
        private object _value;

        //[JsonProperty("value")]
        public object Value 
        { 
            get
            {
                return _value;
            }
            set 
            {
                if (value is string stringValue)
                {
                    _value = stringValue;
                    Payload = Encoding.UTF8.GetBytes(stringValue);
                }
                else if (value is Stream streamValue)
                {
                    _value = streamValue;
                    Payload = GetPayload(streamValue);
                }
                else if (value is byte[] binary)
                {
                    _value = binary;
                    Payload = binary;
                }
                else
                {
                    throw new ArgumentException($"Not supported input message type: {value.GetType()}");
                }
            }
        }

        /// <summary>
        /// used for internal convert
        /// </summary>
        internal byte[] Payload { get; private set; }

        //[JsonProperty("stringValue")]
        public string StringValue => Encoding.UTF8.GetString(Payload);

        /// <summary>
        /// constructor for non-csharp languages
        /// </summary>
        //[JsonConstructor]
        public WebPubSubMessage()
        { }

        public WebPubSubMessage(string message)
        {
            Value = message;
        }

        public WebPubSubMessage(Stream message)
        {
            Value = message;
        }

        internal WebPubSubMessage(byte[] message)
        {
            Value = message;
        }

        internal Stream GetStream()
        {
            if (Value != null)
            {
                if (Value is string stringValue)
                {
                    return new MemoryStream(Encoding.UTF8.GetBytes(stringValue));
                }
                if (Value is byte[] byteValue)
                {
                    return new MemoryStream(byteValue);
                }
                if (Value is Stream streamValue)
                {
                    return streamValue;
                }
            }
            return null;
        }

        private byte[] GetPayload(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(Payload);
        }
    }
}
