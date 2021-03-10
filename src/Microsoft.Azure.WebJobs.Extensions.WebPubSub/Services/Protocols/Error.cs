namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public class Error
    {
        public ErrorCode Code { get; set; }

        public string Message { get; set; }

        public Error(ErrorCode code)
        {
            Code = code;
        }

        public Error(ErrorCode code, string message)
        {
            Code = code;
            Message = message;
        }
    }

}
