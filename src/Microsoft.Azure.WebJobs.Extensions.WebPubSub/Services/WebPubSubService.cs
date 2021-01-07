
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class WebPubSubService : IWebPubSubService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private string BaseEndpoint { get; }
        private string AccessKey { get; }
        private string Version { get; }

        public string HubName { get; } = string.Empty;

        internal WebPubSubService(string connectionString, string hubName = "")
        {
            (BaseEndpoint, AccessKey, Version) = ParseConnectionString(connectionString);
            HubName = hubName;
        }

        internal WebPubSubConnection GetClientConnection(string hubName, IEnumerable<Claim> claims = null)
        {
            var subPath = string.Empty;
            if (!string.IsNullOrEmpty(hubName))
            {
                subPath = $"/hubs/{hubName}";
            }
            var baseEndpoint = new Uri(BaseEndpoint);
            var hubUrl = $"{BaseEndpoint}/ws/client{subPath}";
            var scheme = baseEndpoint.Scheme == "http" ? "ws" : "wss";
            var token = AuthUtility.GenerateJwtBearer(null, hubUrl, claims, DateTime.UtcNow.AddMinutes(30), AccessKey);
            return new WebPubSubConnection
            {
                Url = $"{scheme}://{baseEndpoint.Host}/ws/client{subPath}",
                AccessToken = token
            };
        }

        internal WebPubSubConnection GetServerConnection(string hubName = "", string additionalPath = "")
        {
            var hubUrl = string.IsNullOrEmpty(hubName) ? $"{BaseEndpoint}/ws/api" : $"{BaseEndpoint}/ws/api/hubs/{hubName}";
            var audienceUrl = $"{hubUrl}{additionalPath}";
            var token = AuthUtility.GenerateJwtBearer(null, audienceUrl, null, DateTime.UtcNow.AddMinutes(30), AccessKey);
            return new WebPubSubConnection
            {
                Url = audienceUrl,
                AccessToken = token
            };
        }

        public Task Send(MessageData message)
        {
            var subPath = $"/messages";
            if (!string.IsNullOrEmpty(message.TargetId) && message.TargetType != TargetType.All)
            {
                subPath = $"/{message.TargetType.ToString().ToLower()}/{message.TargetId}/messages";
            }

            if (message.TargetType != TargetType.Connections && message.Excludes?.Length > 0)
            {
                var excludes = string.Join("&", message.Excludes.Select(x => $"excluded={x}"));
                subPath += excludes;
            }

            var connection = GetServerConnection(HubName, subPath);
            return RequestAsync(connection.Url, message.Message, connection.AccessToken, HttpMethod.Post);
        }

        public Task AddToGroup(GroupData groupData)
        {
            if (groupData.TargetType == TargetType.All || groupData.TargetType == TargetType.Groups)
            {
                throw new ArgumentException();
            }

            var subPath = $"/{groupData.TargetType.ToString().ToLower()}/{groupData.TargetId}/groups/{groupData.GroupId}";
            var connection = GetServerConnection(HubName, subPath);
            return RequestAsync(connection.Url, null, connection.AccessToken, HttpMethod.Put);
        }

        public Task RemoveFromGroup(GroupData groupData)
        {
            if (groupData.TargetType == TargetType.All || groupData.TargetType == TargetType.Groups)
            {
                throw new ArgumentException();
            }

            var subPath = $"/{groupData.TargetType.ToString().ToLower()}/{groupData.TargetId}/groups/{groupData.GroupId}";
            var connection = GetServerConnection(HubName, subPath);
            return RequestAsync(connection.Url, null, connection.AccessToken, HttpMethod.Delete);
        }

        public Task CheckExistence(ExistenceData existenceData)
        {
            if (existenceData.TargetType == TargetType.All)
            {
                throw new ArgumentException();
            }

            var subPath = $"/{existenceData.TargetType.ToString().ToLower()}/{existenceData.TargetId}";
            var connection = GetServerConnection(HubName, subPath);
            return RequestAsync(connection.Url, null, connection.AccessToken, HttpMethod.Head);
        }

        public Task CloseConnection(ConnectionCloseData closeData)
        {
            var subPath = $"/connections/{closeData.ConnectionId}";
            if (!string.IsNullOrEmpty(closeData.Reason))
            {
                subPath += $"?reason={closeData.Reason}";
            }

            var connection = GetServerConnection(HubName, subPath);
            return RequestAsync(connection.Url, null, connection.AccessToken, HttpMethod.Delete);
        }

        private Task<HttpResponseMessage> RequestAsync(string url, string message, string bearer, HttpMethod httpMethod)
        {
            var request = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = new Uri(url)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            request.Headers.AcceptCharset.Clear();
            request.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue("UTF-8"));
            request.Headers.Add("Asrs-User-Agent", GetProductInfo());

            if (message != null)
            {
                //var content = JsonConvert.SerializeObject(message);
                request.Content = new StringContent(message, Encoding.UTF8, "text/plain");
            }
            return _httpClient.SendAsync(request);
        }

        private static string GetProductInfo()
        {
            var assembly = typeof(WebPubSubService).GetTypeInfo().Assembly;
            var packageId = assembly.GetName().Name;
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            var runtime = RuntimeInformation.FrameworkDescription?.Trim();
            var operatingSystem = RuntimeInformation.OSDescription?.Trim();
            var processorArchitecture = RuntimeInformation.ProcessArchitecture.ToString().Trim();

            return $"{packageId}/{version} ({runtime}; {operatingSystem}; {processorArchitecture})";
        }

        private static (string EndPoint, string AccessKey, string Version) ParseConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Web PubSub Service connection string is empty");
            }

            var endpointMatch = Regex.Match(connectionString, @"endpoint=([^;]+)", RegexOptions.IgnoreCase);
            if (!endpointMatch.Success)
            {
                throw new ArgumentException("No endpoint present in Web PubSub Service connection string");
            }
            var accessKeyMatch = Regex.Match(connectionString, @"accesskey=([^;]+)", RegexOptions.IgnoreCase);
            if (!accessKeyMatch.Success)
            {
                throw new ArgumentException("No access key present in Web PubSub Service connection string");
            }
            var versionKeyMatch = Regex.Match(connectionString, @"version=([^;]+)", RegexOptions.IgnoreCase);
            if (versionKeyMatch.Success && !System.Version.TryParse(versionKeyMatch.Groups[1].Value, out var version))
            {
                throw new ArgumentException("Invalid version format in Web PubSub Service connection string");
            }

            return (endpointMatch.Groups[1].Value, accessKeyMatch.Groups[1].Value, versionKeyMatch.Groups[1].Value);
        }
    }
}
