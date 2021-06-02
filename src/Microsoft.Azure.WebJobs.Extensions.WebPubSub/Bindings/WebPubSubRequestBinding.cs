using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    //public class WebPubSubRequestBinding : IBinding
    //{
    //    public bool FromAttribute => true;
    //
    //    public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
    //    {
    //        if (value == null)
    //        {
    //            throw new ArgumentNullException("value");
    //        }
    //
    //        var connectionContxt = value as ConnectionContext;
    //        return Task.FromResult<IValueProvider>(new )
    //    }
    //
    //    public Task<IValueProvider> BindAsync(BindingContext context)
    //    {
    //        throw new NotImplementedException();
    //    }
    //
    //    public ParameterDescriptor ToParameterDescriptor()
    //    {
    //        throw new NotImplementedException();
    //    }
    //
    //    private class WebPubSubRequestValueBinder : IOrderedValueBinder
    //    {
    //        public BindStepOrder StepOrder => throw new NotImplementedException();
    //
    //        public Type Type => throw new NotImplementedException();
    //
    //        public Task<object> GetValueAsync()
    //        {
    //            throw new NotImplementedException();
    //        }
    //
    //        public Task SetValueAsync(object value, CancellationToken cancellationToken)
    //        {
    //            throw new NotImplementedException();
    //        }
    //
    //        public string ToInvokeString()
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }
    //}
}
