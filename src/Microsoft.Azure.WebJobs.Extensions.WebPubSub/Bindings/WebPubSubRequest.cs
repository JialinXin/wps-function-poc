using System.Collections.Generic;
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class WebPubSubRequest
    {
        public ConnectionContext ConnectionContext { get; internal set; }

        public ServiceRequest Request { get; internal set; }

        public bool IsAbuseRequest { get; internal set; } = false;

        public WebPubSubRequestStatus RequestStatus { get; }

        public HttpResponseMessage Response { get; }

        internal WebPubSubRequest(WebPubSubRequestStatus status, HttpStatusCode httpStatus, string message = null)
        {
            RequestStatus = status;
            Response = new HttpResponseMessage(httpStatus);
            if (!string.IsNullOrEmpty(message))
            {
                Response.Content = new StringContent(message);
            }
        }

        internal WebPubSubRequest(WebPubSubRequestStatus status, HttpResponseMessage response = null)
        {
            RequestStatus = status;
            Response = response ?? new HttpResponseMessage();
        }
    }
}
