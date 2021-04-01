using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubMessage
    {
        private object _value;

        public object Body 
        { 
            get
            {
                return _value;
            }
            set 
            {
                if (value is string stringValue)
                {
                    // try parse binary data encoded.
                    if (DataType == MessageDataType.Binary)
                    {
                        try
                        {
                            var decodedBytes = Convert.FromBase64String(stringValue);
                            _value = decodedBytes;
                            Payload = decodedBytes;
                            return;
                        }
                        catch (FormatException)
                        {
                            // ignore exception and fallback to string value.
                        }
                    }
                    // simple string
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
        /// DataType of the message.
        /// </summary>
        public MessageDataType DataType = MessageDataType.Text;

        internal byte[] Payload { get; private set; }

        /// <summary>
        /// Constructor for non-csharp languages
        /// </summary>
        public WebPubSubMessage()
        { }

        /// <summary>
        /// Constructor for string/json typed message
        /// </summary>
        public WebPubSubMessage(string message, MessageDataType dataType = MessageDataType.Text)
        {
            Body = message;
            DataType = dataType;
        }

        /// <summary>
        /// Constructor for stream type message
        /// </summary>
        public WebPubSubMessage(Stream message, MessageDataType dataType)
        {
            Body = message;
            DataType = dataType;
        }

        /// <summary>
        /// Constructor for binary type message
        /// </summary>
        public WebPubSubMessage(byte[] message, MessageDataType dataType)
        {
            Body = ConvertMessage(message, dataType);
            DataType = dataType;
        }

        internal Stream GetStream()
        {
            if (Body != null)
            {
                if (Body is string stringValue)
                {
                    return new MemoryStream(Encoding.UTF8.GetBytes(stringValue));
                }
                if (Body is byte[] byteValue)
                {
                    return new MemoryStream(byteValue);
                }
                if (Body is Stream streamValue)
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

        private object ConvertMessage(byte[] message, MessageDataType dataType)
        {
            if (dataType == MessageDataType.Binary)
            {
                return message;
            }
            return Encoding.UTF8.GetString(message);
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(Payload);
        }
    }
}
