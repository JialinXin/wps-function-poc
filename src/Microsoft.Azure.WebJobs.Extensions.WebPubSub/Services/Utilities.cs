﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal static class Utilities
    {
        private const int MaxTokenLength = 4096;

        private static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();

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
                _ => MessageDataType.NotSupported
            };

        public static bool IsUserEvent(string eventType)
            => eventType.StartsWith(Constants.CloudEventTypeUserPrefix, StringComparison.OrdinalIgnoreCase);

        public static bool IsSystemConnect(string eventType)
            => eventType.Equals($"{Constants.CloudEventTypeSystemPrefix}{Constants.Events.ConnectEvent}", StringComparison.OrdinalIgnoreCase);

        public static bool IsSystemDisconnected(string eventType)
            => eventType.Equals($"{Constants.CloudEventTypeSystemPrefix}{Constants.Events.DisconnectedEvent}", StringComparison.OrdinalIgnoreCase);

        public static bool IsSyncMethod(string eventType)
            => IsUserEvent(eventType) || IsSystemConnect(eventType);

        public static HttpResponseMessage BuildResponse(MessageResponse response)
        {
            HttpResponseMessage result = new HttpResponseMessage();

            if (response.Error != null)
            {
                result.StatusCode = GetStatusCode(response.Error.Code);
                result.Content = new StringContent(response.Error.Message);
                return result;
            }

            result.Content = new StreamContent(response.Message);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(response.DataType));

            return result;
        }

        public static HttpResponseMessage BuildResponse(ConnectResponse response)
        {
            HttpResponseMessage result = new HttpResponseMessage();

            if (response.Error != null)
            {
                result.StatusCode = GetStatusCode(response.Error.Code);
                result.Content = new StringContent(response.Error.Message);
                return result;
            }

            var connectEvent = new ConnectEventResponse
            {
                UserId = response.UserId,
                Groups = response.Groups,
                Subprotocol = response.Subprotocol,
                Roles = response.Roles
            };
            result.Content = new StringContent(JsonConvert.SerializeObject(connectEvent));

            return result;
        }

        public static HttpStatusCode GetStatusCode(ErrorCode errorCode) =>
            errorCode switch
            {
                ErrorCode.UserError => HttpStatusCode.BadRequest,
                ErrorCode.Unauthorized => HttpStatusCode.Unauthorized,
                ErrorCode.ServerError => HttpStatusCode.InternalServerError,
                _ => HttpStatusCode.InternalServerError
            };

        public static string GenerateJwtBearer(
            string issuer = null,
            string audience = null,
            IEnumerable<Claim> claims = null,
            DateTime? expires = null,
            string signingKey = null,
            DateTime? issuedAt = null,
            DateTime? notBefore = null)
        {
            var subject = claims == null ? null : new ClaimsIdentity(claims);
            return GenerateJwtBearer(issuer, audience, subject, expires, signingKey, issuedAt, notBefore);
        }

        public static string GenerateAccessToken(string signingKey, string audience, IEnumerable<Claim> claims, TimeSpan lifetime)
        {
            var expire = DateTime.UtcNow.Add(lifetime);

            var jwtToken = GenerateJwtBearer(
                audience: audience,
                claims: claims,
                expires: expire,
                signingKey: signingKey
            );

            if (jwtToken.Length > MaxTokenLength)
            {
                throw new ArgumentException("AccessToken too long.");
            }

            return jwtToken;
        }

        private static string GenerateJwtBearer(
            string issuer = null,
            string audience = null,
            ClaimsIdentity subject = null,
            DateTime? expires = null,
            string signingKey = null,
            DateTime? issuedAt = null,
            DateTime? notBefore = null)
        {
            SigningCredentials credentials = null;
            if (!string.IsNullOrEmpty(signingKey))
            {
                // Refer: https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/releases/tag/5.5.0
                // From version 5.5.0, SignatureProvider caching is turned On by default, assign KeyId to enable correct cache for same SigningKey
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
                securityKey.KeyId = signingKey.GetHashCode().ToString();
                credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            }

            var token = JwtTokenHandler.CreateJwtSecurityToken(
                issuer: issuer,
                audience: audience,
                subject: subject,
                notBefore: notBefore,
                expires: expires,
                issuedAt: issuedAt,
                signingCredentials: credentials);
            return JwtTokenHandler.WriteToken(token);
        }
    }
}