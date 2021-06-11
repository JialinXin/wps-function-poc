// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal static class Utilities
    {
        private static readonly char[] HeaderSeparator = { ',' };

        public static MediaTypeHeaderValue GetMediaType(MessageDataType dataType) => new MediaTypeHeaderValue(GetContentType(dataType));

        public static string GetContentType(MessageDataType dataType) =>
            dataType switch
            {
                MessageDataType.Binary => Constants.ContentTypes.BinaryContentType,
                MessageDataType.Text => Constants.ContentTypes.PlainTextContentType,
                MessageDataType.Json => Constants.ContentTypes.JsonContentType,
                // Default set binary type to align with service side logic
                _ => Constants.ContentTypes.BinaryContentType
            };

        public static MessageDataType GetDataType(string mediaType) =>
            mediaType switch
            {
                Constants.ContentTypes.BinaryContentType => MessageDataType.Binary,
                Constants.ContentTypes.JsonContentType => MessageDataType.Json,
                Constants.ContentTypes.PlainTextContentType => MessageDataType.Text,
                _ => throw new ArgumentException($"{Constants.ErrorMessages.NotSupportedDataType}{mediaType}")
            };

        public static WebPubSubEventType GetEventType(string ceType)
        {
            return ceType.StartsWith(Constants.Headers.CloudEvents.TypeSystemPrefix, StringComparison.OrdinalIgnoreCase) ?
                WebPubSubEventType.System :
                WebPubSubEventType.User;
        }

        public static HttpResponseMessage BuildResponse(MessageResponse response)
        {
            HttpResponseMessage result = new HttpResponseMessage();

            if (response.Message != null)
            {
                result.Content = new StreamContent(response.Message.ToStream());
            }
            result.Content.Headers.ContentType = GetMediaType(response.DataType);

            return result;
        }

        public static HttpResponseMessage BuildResponse(ConnectResponse response)
        {
            var connectEvent = new ConnectEventResponse
            {
                UserId = response.UserId,
                Groups = response.Groups,
                Subprotocol = response.Subprotocol,
                Roles = response.Roles
            };

            return BuildResponse(JsonConvert.SerializeObject(connectEvent), MessageDataType.Json);
        }

        public static HttpResponseMessage BuildResponse(string response, MessageDataType dataType = MessageDataType.Text)
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(response, Encoding.UTF8, GetContentType(dataType)),
            };
        }

        public static HttpResponseMessage BuildErrorResponse(ErrorResponse error)
        {
            return new HttpResponseMessage
            {
                StatusCode = GetStatusCode(error.Code),
                Content = new StringContent(error.ErrorMessage)
            };
        }

        public static HttpStatusCode GetStatusCode(WebPubSubErrorCode errorCode) =>
            errorCode switch
            {
                WebPubSubErrorCode.UserError => HttpStatusCode.BadRequest,
                WebPubSubErrorCode.Unauthorized => HttpStatusCode.Unauthorized,
                WebPubSubErrorCode.ServerError => HttpStatusCode.InternalServerError,
                _ => HttpStatusCode.InternalServerError
            };

        public static IReadOnlyList<string> GetSignatureList(string signatures)
        {
            if (string.IsNullOrEmpty(signatures))
            {
                return default;
            }

            return signatures.Split(HeaderSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        public static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static PropertyInfo GetProperty(Type type, string name)
        {
            return type.GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static string FirstOrDefault(params string[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrEmpty(v));
        }

        public static RequestType GetRequestType(WebPubSubEventType eventType, string eventName)
        {
            if (eventType == WebPubSubEventType.User)
            {
                return RequestType.User;
            }
            if (eventName.Equals(Constants.Events.ConnectEvent))
            {
                return RequestType.Connect;
            }
            if (eventName.Equals(Constants.Events.DisconnectedEvent))
            {
                return RequestType.Disconnect;
            }
            return RequestType.Ignored;
        }

        public static bool ValidateSignature(string connectionId, string signature, HashSet<string> accessKeys)
        {
            foreach (var accessKey in accessKeys)
            {
                var signatures = Utilities.GetSignatureList(signature);
                if (signatures == null)
                {
                    continue;
                }
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(accessKey)))
                {
                    var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(connectionId));
                    var hash = "sha256=" + BitConverter.ToString(hashBytes).Replace("-", "");
                    if (signatures.Contains(hash, StringComparer.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return false;
        }

        public static bool ValidateContentType(string mediaType, out MessageDataType dataType)
        {
            try
            {
                dataType = GetDataType(mediaType);
                return true;
            }
            catch (Exception)
            {
                dataType = MessageDataType.Binary;
                return false;
            }
        }

        public static bool RespondToServiceAbuseCheck(HttpRequestMessage req, HashSet<string> allowedHosts, out HttpResponseMessage response)
        {
            var hosts = req.Headers.GetValues(Constants.Headers.WebHookRequestOrigin);
            return RespondToServiceAbuseCheck(req.Method.ToString(), hosts, allowedHosts, out response);
        }

        public static bool RespondToServiceAbuseCheck(string requestHost, HashSet<string> allowedHosts, out HttpResponseMessage response)
        {
            var requestHosts = new string[] { requestHost };
            return RespondToServiceAbuseCheck("options", requestHosts, allowedHosts, out response);
        }

        public static bool RespondToServiceAbuseCheck(string method, IEnumerable<string> requestHosts, HashSet<string> allowedHosts, out HttpResponseMessage response)
        {
            response = new HttpResponseMessage();
            if (method.Equals("options", StringComparison.OrdinalIgnoreCase) || method.Equals("get", StringComparison.OrdinalIgnoreCase))
            {
                if (requestHosts != null && requestHosts.Any())
                {
                    foreach (var item in allowedHosts)
                    {
                        if (requestHosts.Contains(item))
                        {
                            response.Headers.Add(Constants.Headers.WebHookAllowedOrigin, requestHosts);
                            return true;
                        }
                    }
                    response.StatusCode = HttpStatusCode.BadRequest;
                }
                return true;
            }
            return false;
        }
    }
}
