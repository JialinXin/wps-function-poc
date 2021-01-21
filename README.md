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

`WebPubSub` output binding allows generic messages to send to an Azure Web PubSub service.

`WebPubSubTrigger` trigger bindings allows to responding to all kinds of upstream messages for trigger different operations to services.

### Limitations
- The upstream using persistent connection is out of scope.
- Supporting protocols where one protocol message is not bound to a single WebSocket frame is out of scope, e.g. MQTT.
- Supporting protocols where messages are having context or dependencies to previous messages is out of scope, e.g. streaming protocols.

## Usage

### Create Azure Web PubSub Service instance
...

### Using the WebPubSubConnection input binding

In anonymous mode, `UserId` can be used with {headers.userid} or {query.userid} depends on where the userid is assigned. Similarly users can set customers generated JWT accesstoken by assign `AccessToken = {query.accesstoken}` where customized claims are built with. 

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

### Using the WebPubSub output binding

```cs
[FunctionName("chat")]
public static Task Broadcast(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
    [WebPubSub(HubName = "simplechat")] IAsyncCollector<MessageData> messages)
{
    var msg = new MessageData
    {
        Message = (new StreamReader(req.Body)).ReadToEnd();
    };
    return messages.AddAsync(msg);
}
```

### Using the WebPubSubTrigger trigger binding

```cs
[FunctionName("broadcast")]
public static Task Broadcast(
    [WebPubSubTrigger]InvocationContext context,
    [WebPubSub] IAsyncCollector<MessageData> messages)
{
    var msg = new MessageData
    {
        Message = System.Text.Encoding.UTF8.GetString(context.Payload.Span)
    };
    return messages.AddAsync(msg);
}
```

And in client side the function should be triggered by assign a property `FunctionName` in the message which point the target function to be bind.

```js
this.websocket.send(JSON.stringify({
    FunctionName:"broadcast",
    from: this.username,
    content: content,
}));
```

> For connect/disconnect event, customer has no chance to bind the target Azure Function, so `WebPubSub` property `HubName` and `Event` will be used as the key to auto bind which means the properties will be required for this kind of events (connect/disconnect). 
> ```cs
> [FunctionName("connect")]
> public static void Connect(
>     [WebPubSubTrigger("simplechat", "connect")]InvocationContext context)
> {
>     Console.WriteLine($"{context.ConnectionId}");
>     Console.WriteLine("Connect.");
> }
> ```

### Supported object types for Output/Trigger actions.

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