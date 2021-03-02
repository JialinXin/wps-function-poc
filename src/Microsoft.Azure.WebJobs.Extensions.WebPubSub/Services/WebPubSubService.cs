
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

        private readonly string _baseEndpoint;
        private readonly string _accessKey;
        private readonly string _version;
        private readonly string _port;

        public string HubName { get; } = string.Empty;

        private readonly string _hubPath;

        internal WebPubSubService(string connectionString, string hubName = "")
        {
            (_baseEndpoint, _accessKey, _version, _port) = ParseConnectionString(connectionString);
            _port = string.IsNullOrEmpty(_port) ? string.Empty : $":{_port}";
            HubName = hubName;
            _hubPath = string.IsNullOrEmpty(hubName) ? string.Empty : $"/hubs/{hubName}";
        }

        internal WebPubSubConnection GetClientConnection(IEnumerable<Claim> claims = null)
        {
            var subPath = "?";
            if (!string.IsNullOrEmpty(HubName))
            {
                subPath += $"hub={HubName}";
            }
            var hubUrl = $"{_baseEndpoint}/client/{subPath}";
            var baseEndpoint = new Uri(_baseEndpoint);
            var scheme = baseEndpoint.Scheme == "http" ? "ws" : "wss";
            var token = AuthUtility.GenerateJwtBearer(null, hubUrl, claims, DateTime.UtcNow.AddMinutes(30), _accessKey);
            return new WebPubSubConnection
            {
                Url = $"{scheme}://{baseEndpoint.Authority}{_port}/client{subPath}&access_token={token}",
                AccessToken = token
            };
        }

        internal WebPubSubConnection GetServerConnection(string additionalPath = "")
        {
            var audienceUrl = $"{_baseEndpoint}/api{_hubPath}{additionalPath}";
            var token = AuthUtility.GenerateJwtBearer(null, audienceUrl, null, DateTime.UtcNow.AddMinutes(30), _accessKey);
            return new WebPubSubConnection
            {
                Url = $"{_baseEndpoint}{_port}/api{_hubPath}{additionalPath}",
                AccessToken = token
            };
        }

        public Task Send(MessageData message)
        {
            var subPath = $"/:send";
            if (!string.IsNullOrEmpty(message.TargetId) && message.TargetType != TargetType.All)
            {
                subPath = $"/{message.TargetType.ToString().ToLower()}/{message.TargetId}/:send";
            }

            if (message.TargetType != TargetType.Connections && message.Excludes?.Length > 0)
            {
                var excludes = string.Join("&", message.Excludes.Select(x => $"excluded={x}"));
                subPath += $"?{excludes}";
            }

            var connection = GetServerConnection(subPath);
            return RequestAsync(connection.Url, message.Message, connection.AccessToken, HttpMethod.Post);
        }

        public Task AddToGroup(GroupData groupData)
        {
            if (groupData.TargetType == TargetType.All || groupData.TargetType == TargetType.Groups)
            {
                throw new ArgumentException();
            }

            var subPath = $"/{groupData.TargetType.ToString().ToLower()}/{groupData.TargetId}/groups/{groupData.GroupId}";
            var connection = GetServerConnection(subPath);
            return RequestAsync(connection.Url, null, connection.AccessToken, HttpMethod.Put);
        }

        public Task RemoveFromGroup(GroupData groupData)
        {
            if (groupData.TargetType == TargetType.All || groupData.TargetType == TargetType.Groups)
            {
                throw new ArgumentException();
            }

            var subPath = $"/{groupData.TargetType.ToString().ToLower()}/{groupData.TargetId}/groups/{groupData.GroupId}";
            var connection = GetServerConnection(subPath);
            return RequestAsync(connection.Url, null, connection.AccessToken, HttpMethod.Delete);
        }

        public Task CheckExistence(ExistenceData existenceData)
        {
            if (existenceData.TargetType == TargetType.All)
            {
                throw new ArgumentException();
            }

            var subPath = $"/{existenceData.TargetType.ToString().ToLower()}/{existenceData.TargetId}";
            var connection = GetServerConnection(subPath);
            return RequestAsync(connection.Url, null, connection.AccessToken, HttpMethod.Head);
        }

        public Task CloseConnection(ConnectionCloseData closeData)
        {
            var subPath = $"/connections/{closeData.ConnectionId}";
            if (!string.IsNullOrEmpty(closeData.Reason))
            {
                subPath += $"?reason={closeData.Reason}";
            }

            var connection = GetServerConnection(subPath);
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
            request.Headers.Add("wps-User-Agent", GetProductInfo());

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

        private static (string EndPoint, string AccessKey, string Version, string Port) ParseConnectionString(string connectionString)
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
            var portKeyMatch = Regex.Match(connectionString, @"port=([^;]+)", RegexOptions.IgnoreCase);
            var port = portKeyMatch.Success ? portKeyMatch.Groups[1].Value : string.Empty;

            return (endpointMatch.Groups[1].Value, accessKeyMatch.Groups[1].Value, versionKeyMatch.Groups[1].Value, port);
        }
    }
}
