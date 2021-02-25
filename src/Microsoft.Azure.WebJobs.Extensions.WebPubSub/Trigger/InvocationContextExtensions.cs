namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public static class InvocationContextExtensions
    {
        public static ClientConnectResponseMessage ToConnectResponse(this InvocationContext context)
        {
            return new ClientConnectResponseMessage
            {
                ConnectionId = context.ConnectionId,
                UserId = context.UserId
            };
        }

        public static ClientPayloadResponseMessage ToPayloadResponse(this InvocationContext context)
        {
            return new ClientPayloadResponseMessage
            {
                ConnectionId = context.ConnectionId,
                Payload = context.Payload
            };
        }
    }
}
