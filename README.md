# Azure Functions Bindings for Azure Web PubSub Service

## NuGet Packages

Package Name | Target Framework | NuGet
---|---|---
Microsoft.Azure.WebJobs.Extensions.WebPubSub | .NET Standard 2.0 | 

## Intro
These bindings allow Azure Functions to integrate with **Azure Web PubSub Service**.

### Supported scenarios

- Allow clients to connect to a Web PubSub Service hub without requiring an ASP.NET Core backend
- Use Azure Functions (any language supported by V2) to broadcast messages to all clients connected to a Web PubSub Service hub.
- Use Azure Functions (any language supported by V2) to send messages to a single user/connection, or all the users/connections in a group.
- Use Azure Functions (any language supported by V2) to manage group users like add/remove/check a single user/connection in a group.
- Example scenarios include: broadcast messages to a Web PubSub Service hub on HTTP requests.

### Bindings

`WebPubSubConnection` input binding makes it easy to generate websocket url align with access token for clients to initiate a connection to Azure Web PubSub Service.

`WebPubSub` output binding allows sending all kinds of messages to an Azure Web PubSub service.

`WebPubSubTrigger` trigger bindings allows responding to all kinds of upstream events to trigger different operations to services.

### Development Plan

[Azure WebPubSub Development Plan](https://github.com/Azure/azure-webpubsub/blob/main/docs/specs/development-plan.md)

- [ ] **Phase 1** Support simple websocket clients

- [ ] **Phase 2** Support subprotocol websocket clients

> Subprotocol deserialization is done by service side. Server will have a consistant `Event` property to understand the request. So not much gap between phase 1 & 2 in function side.

## Usage

### Create Azure Web PubSub Service instance
...

### Using the WebPubSubConnection input binding

In anonymous mode, `UserId` can be used with {headers.userid} or {query.userid} depends on where the userid is assigned in the negotiate call.

Similarly users can set customers generated JWT accesstoken by assign `AccessToken = {query.accesstoken}` where customized claims are built with. 

```cs
[FunctionName("login")]
public static WebPubSubConnection GetClientConnection(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
    [WebPubSubConnection(HubName = "simplechat", UserId = "{query.userid}")] WebPubSubConnectioconnection,
    ILogger log)
{
    return connection;
}
```

### Using the WebPubSubTrigger trigger binding

When clients already know Web PubSub service and communication to service, `WebPubSubTrigger` can be used as listerner towards all kinds of requests coming from service. To have a consistent routing logic that needs to configure in service side (Resource Provider), `FunctionName` will be used as the unique key to match upstream events. Rule is `<hub>-<event>` works for user defined hubs and `<event>` works for default hub. Service will map to `_default` hub which is unaware by customer.

For a connect request, server side has some controls to manager user's authentication before connected. Future on-hold messages can also be used like this. Proerties available to set will be opened in `InvocationContext` and function extension will help build correct response to service after user actions are done in function.

```cs
[FunctionName("connect")]
public static void Connect(
[WebPubSubTrigger]InvocationContext context)
{
    Console.WriteLine($"{context.ConnectionId}");
    Console.WriteLine("Connect.");
    if (context.UserId == "abc")
    {
        // some further check
        context.StatusCode = System.Net.HttpStatusCode.Unauthorized;
        // or set roles
        context.Roles = new string[] { "Admin" };
    }
}
```

### Using the WebPubSub output binding

For single message request, customer could bind to a target operation related data type to send the request.

```cs
[FunctionName("message")]
[return: WebPubSub]
public static MessageData Broadcast(
    [WebPubSubTrigger] InvocationContext context)
{
    return new MessageData
    {
        Message = GetString(context.Payload.Span)
    };
}
```

To send multiple messages, customer need to work with generic `WebPubSubEvent` and do multiple tasks in order.

```cs
[FunctionName("message")]
public static async Task Message(
    [WebPubSubTrigger] InvocationContext context,
    [WebPubSub] IAsyncCollector<WebPubSubEvent> eventHandler)
{
    await message.AddAsync(new GroupData
    {
        Action = GroupAction.Add,
        TargetType = TargetType.Users,
        TargetId = context.UserId,
        GroupId = "group1",
    })
    await message.AddAsync(new MessageData
    {
        Message = GetString(context.Payload.Span)
    });
}
```

> When SDK has better supports, server side could work with server sdk convenience layer methods without output binding data type limited.
> ```cs
> [FunctionName("message")]
> public static async Task Message(
>     [WebPubSubTrigger] InvocationContext context)
> {
>     var server = context.GetWebPubSubServer();
>     await server.AddToGroupAsync(context.UserId, "group1");
>     await server.SendAsync(context.Payload);
> }
> ```

### Supported object types for Output bindings.

#### WebPubSubEvent (Generic base class)

#### MessageData

1. `TargetType`, supports All, Users, Connections, Groups, default as All
2. `TargetId`, use with `TagetType`, where target id should be assigned if `TargetType` is not All.
3. `Excludes`, excludes connection ids
4. `Message`

#### GroupData

1. `GroupAction`, supports Add/Remove
2. `TargetType`, supports Users/Connections
3. `TargetId`
4. `GroupId`

#### ExistenceData

1. `TargetType` supports Users/Connections/Groups
2. `TargetId`

#### CloseConnectionData

1. `ConnectionId`
2. `Reason`