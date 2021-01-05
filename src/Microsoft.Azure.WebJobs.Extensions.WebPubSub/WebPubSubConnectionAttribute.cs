
using System;
using System.Collections.Generic;
using System.Security.Claims;
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
            get { return _userId; }
            set { _userId = value; }
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

        internal string GetUserIdFromQuery(string query)
        {

        }
    }
}
