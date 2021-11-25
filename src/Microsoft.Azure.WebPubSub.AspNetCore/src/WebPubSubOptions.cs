﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    /// <summary>
    /// Options when using Web PubSub service.
    /// </summary>
    public class WebPubSubOptions
    {
        /// <summary>
        /// An service endpoint represents the Web PubSub service.
        /// </summary>
        public ServiceEndpoint ServiceEndpoint { get; set; }
    }
}
