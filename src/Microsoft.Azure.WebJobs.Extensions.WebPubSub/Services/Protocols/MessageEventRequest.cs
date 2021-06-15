// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public sealed class MessageEventRequest : ServiceRequest
    {
        public BinaryData Message { get; set; }
        public MessageDataType DataType { get; set; }
    }
}
