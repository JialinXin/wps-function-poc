using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal static class JObjectExtensions
    {
        public static bool TryConvert<TOutput>(this JObject input, out TOutput output)
        {
            try
            {
                output = JsonConvert.DeserializeObject<TOutput>(input.ToString());
            }
            catch (Exception)
            {
                output = default;
                return false;
            }

            return true;
        }
    }
}
