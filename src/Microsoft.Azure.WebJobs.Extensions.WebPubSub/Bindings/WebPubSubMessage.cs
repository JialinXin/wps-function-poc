using System;
using System.IO;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubMessage
    {
        /// <summary>
        /// Web PubSub data message
        /// </summary>
        public BinaryData Body { get; private set; }

        /// <summary>
        /// DataType of the message.
        /// </summary>
        public MessageDataType DataType { get; private set; } = MessageDataType.Text;

        /// <summary>
        /// Constructor for string/json typed message
        /// </summary>
        public WebPubSubMessage(string message, MessageDataType dataType = MessageDataType.Text)
        {
            DataType = dataType;

            if (DataType == MessageDataType.Binary)
            {
                try
                {
                    var decodedBytes = Convert.FromBase64String(message);
                    Body = BinaryData.FromBytes(decodedBytes);
                    return;
                }
                catch (FormatException)
                {
                    // ignore exception and fallback to string value.
                }
            }
            // simple string
            Body = BinaryData.FromString(message);
        }

        /// <summary>
        /// Constructor for stream type message
        /// </summary>
        public WebPubSubMessage(Stream message, MessageDataType dataType)
        {
            Body = BinaryData.FromStream(message);
            DataType = dataType;
        }

        /// <summary>
        /// Constructor for binary type message
        /// </summary>
        public WebPubSubMessage(byte[] message, MessageDataType dataType)
        {
            Body = BinaryData.FromBytes(message);
            DataType = dataType;
        }

        public override string ToString()
        {
            return Body.ToString();
        }
    }
}
