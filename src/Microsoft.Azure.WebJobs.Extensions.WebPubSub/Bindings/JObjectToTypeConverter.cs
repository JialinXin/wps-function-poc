using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    internal class JObjectToTypeConverter<TOutput> where TOutput : class
    {
        public bool TryConvert(JObject input, out TOutput output)
        {
            try
            {
                output = JsonConvert.DeserializeObject<TOutput>(input.ToString());
            }
            catch (Exception)
            {
                output = null;
                return false;
            }

            return true;
        }
    }
}
