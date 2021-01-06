
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using Microsoft.Azure.WebJobs.Description;


namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
    [Binding]
    public class WebPubSubConnectionAttribute : Attribute
    {
        private string _userId;

        [ConnectionString]
        public string ConnectionStringSetting { get; set; } = Constants.WebPubSubConnectionStringName;

        [AutoResolve]
        public string HubName { get; set; }

        [AutoResolve]
        public string UserId 
        {
            get 
            { 
                return GetUserIdFromQuery(_userId); 
            }
            set 
            { 
                _userId = value; 
            }
        }

        internal IEnumerable<Claim> GetClaims()
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(UserId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, UserId));
            }
            return claims;
        }

        internal string GetUserIdFromQuery(string userId)
        {
            // query format
            if (userId.StartsWith("?"))
            {
                var queryCollection = HttpUtility.ParseQueryString(userId);
                return queryCollection["user"];
            }
            return userId;
        }
    }
}
