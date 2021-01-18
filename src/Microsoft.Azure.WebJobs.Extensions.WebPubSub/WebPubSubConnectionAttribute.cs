
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
        [ConnectionString]
        public string ConnectionStringSetting { get; set; } = Constants.WebPubSubConnectionStringName;

        [AutoResolve]
        public string HubName { get; set; }

        [AutoResolve]
        public string UserId { get; set; }

        internal IEnumerable<Claim> GetClaims()
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(UserId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, UserId));
            }
            return claims;
        }
    }
}
