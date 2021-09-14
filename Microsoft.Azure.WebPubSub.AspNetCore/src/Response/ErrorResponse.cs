// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    /// <summary>
    /// Response for errors.
    /// </summary>
    public class ErrorResponse : ServiceResponse
    {
        /// <summary>
        /// Error code. Required field to deserialize ErrorResponse.
        /// </summary>
        [JsonPropertyName("code"), JsonConverter(typeof(JsonStringEnumConverter))]
        public WebPubSubErrorCode Code { get; set; }

        /// <summary>
        /// Error messages.
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public ErrorResponse(WebPubSubErrorCode code, string message = null)
        {
            Code = code;
            ErrorMessage = message;
        }

        /// <summary>
        /// Default constructor for JsonDeserialize.
        /// </summary>
        public ErrorResponse()
        { }
    }
}
