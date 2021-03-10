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
        [ConnectionString]
        public string ConnectionStringSetting { get; set; }

        [AutoResolve]
        public string Hub { get; set; }

        [AutoResolve]
        public string UserId { get; set; }

        /// <summary>
        /// Format: key=value&key1=value1
        /// </summary>
        [AutoResolve]
        public string CustomClaims { get; set; }

        internal IEnumerable<Claim> GetClaims()
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(UserId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, UserId));
            }
            if (CustomClaims != null)
            {
                foreach (var claim in CustomClaims.Split('&'))
                {
                    var items = claim.Split('=');
                    claims.Add(new Claim(items[0], items[1]));
                }
            }
            return claims;
        }
    }
}
