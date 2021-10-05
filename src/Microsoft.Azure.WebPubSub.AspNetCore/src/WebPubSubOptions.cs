// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    /// <summary>
    /// Options when using Web PubSub service.
    /// </summary>
    public class WebPubSubOptions
    {
        /// <summary>
        /// Validation options for Abuse Protection and Signature checks.
        /// </summary>
        public WebPubSubValidationOptions ValidationOptions => new WebPubSubValidationOptions();

        public WebPubSubOptions()
        {
        }
    }
}
