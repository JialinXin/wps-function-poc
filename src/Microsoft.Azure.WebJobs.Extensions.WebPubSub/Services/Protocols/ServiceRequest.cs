﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public abstract class ServiceRequest
    {
        /// <summary>
        /// Flag to indicate whether it's an <see href="https://github.com/cloudevents/spec/blob/v1.0/http-webhook.md#4-abuse-protection">Abuse Protection</see> request.
        /// </summary>
        public bool IsPreflight { get; }

        public bool Valid { get; }

        public bool Unauthorized { get; }

        public bool BadRequest { get; }

        public string ErrorMessage { get;}

        public abstract string Name { get; }

        public ServiceRequest(bool isPreflight, bool valid, bool unauthorized, bool badRequest, string error = null)
        {
            IsPreflight = isPreflight;
            Valid = valid;
            Unauthorized = unauthorized;
            BadRequest = badRequest;
            ErrorMessage = error;
        }

        internal ServiceRequest(bool isPreflight, bool valid)
        {
            IsPreflight = isPreflight;
            Valid = valid;
        }

        internal ServiceRequest(HttpStatusCode status, string error = null)
        {
            switch(status)
            {
                case HttpStatusCode.OK: 
                    Valid = true;
                    break;
                case HttpStatusCode.Unauthorized:
                    Unauthorized = true;
                    break;
                case HttpStatusCode.BadRequest:
                    BadRequest = true;
                    break;
                default:
                    BadRequest = true;
                    break;
            }
            if (!string.IsNullOrEmpty(error))
            {
                ErrorMessage = error;
            }
        }
    }
}
