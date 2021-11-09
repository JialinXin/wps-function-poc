// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Azure.WebPubSub.Common
{
    /// <summary>
    /// Validation request for abuse protection.
    /// </summary>
    public sealed class PreflightRequest : WebPubSubEventRequest
    {
        /// <summary>
        /// Flag to indicate whether is a valid request.
        /// </summary>
        [JsonPropertyName("isValid")]
        public bool IsValid { get; }

        internal PreflightRequest(bool isValid)
            :base(null)
        {
            IsValid = isValid;
        }
    }
}
