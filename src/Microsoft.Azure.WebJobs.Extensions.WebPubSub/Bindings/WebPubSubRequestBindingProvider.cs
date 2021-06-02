using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    //internal class WebPubSubRequestBindingProvider : IBindingProvider
    //{
    //    private readonly WebPubSubOptions _options;
    //    private readonly INameResolver _nameResolver;
    //
    //    public WebPubSubRequestBindingProvider(WebPubSubOptions options, INameResolver nameResolver)
    //    {
    //        _options = options;
    //        _nameResolver = nameResolver;
    //    }
    //
    //    public Task<IBinding> TryCreateAsync(BindingProviderContext context)
    //    {
    //        if (context == null)
    //        {
    //            throw new ArgumentNullException("context");
    //        }
    //
    //        ParameterInfo parameter = context.Parameter;
    //        WebPubSubRequestAttribute attribute = parameter.GetCustomAttribute<WebPubSubRequestAttribute>(inherit: false);
    //        if (attribute == null)
    //        {
    //            return Task.FromResult<IBinding>(null);
    //        }
    //
    //        if (_nameResolver != null)
    //        {
    //            // resolve headers.
    //        }
    //
    //        return Task.FromResult<IBinding>(new )
    //    }
    //}
}
