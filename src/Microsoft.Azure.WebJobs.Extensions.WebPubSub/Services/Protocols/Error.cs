using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class Error
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public Error(int code)
        {
            Code = code;
        }

        public Error(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }

}
