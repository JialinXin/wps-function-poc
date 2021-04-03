# Azure Functions Bindings for Azure Web PubSub Service

## NuGet Packages

Package Name | Target Framework | NuGet
---|---|---
Microsoft.Azure.WebJobs.Extensions.WebPubSub | .NET Standard 2.0 | 

## Intro
These bindings allow Azure Functions to integrate with **Azure Web PubSub Service**.

## Supported scenarios

- Allow clients to connect to a Web PubSub Service hub without a self-host server.
- Use Azure Functions (any language supported by V2) to broadcast messages to all clients connected to a Web PubSub Service hub.
- Use Azure Functions (any language supported by V2) to send messages to a single user/connection, or all the users/connections in a group.
- Use Azure Functions (any language supported by V2) to manage group users like add/remove/check a single user/connection in a group.

## Development Plan

[Azure WebPubSub Development Plan](https://github.com/Azure/azure-webpubsub/blob/main/docs/specs/development-plan.md)

- [ ]  Support rest api covered scenarios.

- [ ] **Portal Support** Azure Portal integration for an easy working experience to create/configure Azure Functions for Web PubSub service.

- [ ] Funcions bundle integration.

> Before function bundle integration is done, user need to install the extension explicitly.
> 
> Steps:
> 1. Add package reference in `extensions.csproj`. Run below command.
>    ```cmd
>    func extensions install --package Microsoft.Azure.WebJobs.Extensions.WebPubSub --version 1.0.0
>    ```
>    So you extension project will have a package reference like below.
>    ```xml
>    <PackageReference Include="Microsoft.Azure.WebJobs.Extensions.WebPubSub" Version="1.0.0" />
>    ```
> 1. **Remove** bundle settings in `host.json` if exists to avoid skipping install our extension.
>    ```js
>    "extensionBundle": {
>        "id": "Microsoft.Azure.Functions.ExtensionBundle",
>        "version": "[1.*, 2.0.0)"
>    }
>    ```
> 
> Fur futher details. Please refer to [Explicitly install extensions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-register#explicitly-install-extensions).

## Bindings and Workflow

![functions workflow](https://user-images.githubusercontent.com/15338714/110567798-08074c80-818d-11eb-8583-c382483e9fff.png))

### `WebPubSubConnection` Input Binding
***Client Negotiation (1)-(2)***

Clients use `HttpTrigger` to request functions return `WebPubSubConnection` input binding which provides service websocket url along with access token. Input binding makes it easy to generate required information to setup websocket connections in client side. This step is optional that if clients already configured with service information, it can skip negotiation and direct raise websocket connection request to service and refer to next step.

### `WebPubSubTrigger` Trigger binding
***Client Websocket requests (3)-(4)***

Clients set up websocket connection to service, and clients can send connect/message/disconnect request through the websocket connection on demand. Service will forward these events to functions by `WebPubSubTrigger` to let function known and do something. Especially, functions can accept/block the request for connect/message(synchronous events), refer to [this](https://github.com/Azure/azure-webpubsub/blob/main/docs/specs/phase-1-simple-websocket-client.md#simple-websocket-connection) for details.

### `WebPubSub` Output Binding 
***Function requests (5)-(6)***

When function is triggered, it can send any messaging request by `WebPubSub` output bindings to service. And service will accordingly do broadcast or managing groups operation regarding the rest api calls.

## Bindings Usage

### WebPubSubOptions

To work with Azure Web PubSub service, `ConnectionString` and `Hub` name is required for each bindings beside other attributes. To make it convenient to have a centralize settings instead of set it every time, we support to read it from function settings, like `local.settings.json`. But still, user can set the attribute in different functions. Please notice attribute settings will overwrite global settings.

### Using the WebPubSubConnection input binding

 Customer can set `Hub`, `UserId` in the input binding where values can pass through the request parameters. For example, `UserId` can be used with {headers.userid} or {query.userid} depends on where the userid is assigned in the negotiate call. `Hub` is required in the binding.

* csharp usage:
```cs
[FunctionName("login")]
public static WebPubSubConnection GetClientConnection(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
    [WebPubSubConnection(Hub = "simplechat", UserId = "{query.userid}")] WebPubSubConnection connection,
    ILogger log)
{
    return connection;
}
```

* javascript usage:
```js
{
    "type": "webpubsubConnection",
    "name": "connection",
    "userId": "{query.userid}",
    "hub": "simplechat",
    "direction": "in"
}
```
```js
module.exports = function (context, req, connection) {
  context.res = { body: connection };
  context.done();
};
```

### Using the WebPubSubTrigger trigger binding

When clients already know Web PubSub service and communication to service, `WebPubSubTrigger` can be used as listener towards all kinds of requests coming from service. Function will use `WebPubSubTrigger` attributes as the **UNIQUE** key to map correct function. `EventType` will set to `system` by default. 

EventType|(Allowed) Event
--|--
system|connect, connected, disconnect
user|any, e.g. message or user defined in subprotocol

`ConnectionContext` is a binding object contains common fields among all request, basically refer to [CloudEvents](protocol-cloudevents.md#events) for available fields. Other optional binding objects differs on the scenarios can be bind on-demand, details are listed below. 

> [**Important**]
> 
> `ConnectionContext` is a common binding object in all requests. In dotnet, it will be binded by the type without naming restricted. And in non-dotnet like javascript, it will default bind to `WebPubSubTrigger` object. For rest properties listed below, it will be binded with object name. In dotnet, user should bind them both `name` and `type` correctly if needed, using as trigger parameters. In non-dotnet, user can get the values like `context.bindingData.<name>` directly in functions. Sample usages can be refered from later samples.

Binding Name | Binding Type | Description | Properties
--|--|--|--
connectionContext|`ConnectionContext`|Common request information|Type, Event, Hub, ConnectionId, UserId, Headers, Queries, Claims, MediaType
message|`Stream` |Request message content|-
dataType|`MessageDataType`|Data type of request message|-
claims|`IDictionary<string, string[]>`|User Claims in connect request|-
subprotocols|`string[]`|Available subprotocols in connect request|-
clientCertificates|`ClientCertificate[]`|A list of certificate thumbprint from clients in connect request|-
reason|`string`|Reason in disconnect request|-

`WebPubSubTrigger` will respect customer returned response for synchronous events of `connect` and `message`. Only matched response will be sent back to service, otherwise, it will be ignored. Notice that `Error` has higher priority than rest fields that if `Error` is set, service will regard this request as failed and take some actions like drop down client connection and log information in service side. If user needs to send message back to current connection using `MessageResponse`, `DataType` is suggested to set within `MessageResponse` to improve data encode/decode. `DataType` is limited to `text`, `json` and `binary` and default value is `binary`.

Return Type | Description | Properties
--|--|--
`ConnectResponse`| Response for `connect` event | Error, Groups, Roles, UserId, Subprotocol
`MessageResponse`| Response for user event | Error, DataType, Message

> If customer returns wrong response type, it'll be ignored.

* csharp usage:
```cs
[FunctionName("connect")]
public static ConnectResponse Connect(
[WebPubSubTrigger(Hub = "simplechat", EventName = "connect", EventType = "system")]ConnectionContext context)
{
    Console.WriteLine($"{context.ConnectionId}");
    Console.WriteLine("Connect.");
    if (context.UserId == "abc")
    {
        return new ConnectResponse()
        {
            Error = new Error { Code = ErrorCode.Unauthorized, Error = "Invalid User" }
        };
    }
    else 
    {
        return new ConnectResponse()
        {
            Roles = new string[] { "Admin" }
        };
    }
}
```

* javascript usage:
```js
{
    "type": "webwebPubSubTrigger",
    "name": "connectionContext",
    "hub": "simplechat",
    "event": "connect",
    "eventType", "system"
    "direction": "in"
},
{
    "type": "connectResponse"
    "name": "response",
    "direction": "out"
},
```
```js
module.exports = function (context, connectionContext) {
  context.log('Receive event: ${context.bindingData.event} from connection: ${context.bindingData.connectionId}.');
  context.response = [{
      "code": "unauthorized",
      "error": "Invalid User"
  }];
  context.done();
};
```

### Using the WebPubSub output binding

For a single request, customer can bind to a target operation related event type to send the request. For `MessageEvent`, customer can set `DataType` (allowed `binary`, `text`, `json`) to improve processing efficiency and `null` will be regarded as `binary`.

* csharp usage:
```cs
[FunctionName("broadcast")]
[return: WebPubSub]
public static WebPubSubEvent Broadcast(
    [WebPubSubTrigger(Hub = "simplechat", EventName = "message", EventType = "user")] ConnectionContext context,
    Stream message)
{
    return new WebPubSubEvent
    {
        Operation = WebPubSubOperation.SendToAll,
        Message = message,
        DataType = MessageDataType.Text
    };
}
```

* javascript usage:
```js
{
    "type": "webPubSubTrigger",
    "name": "connectionContext",
    "hub": "simplechat",
    "eventName": "message",
    "eventType": "user",
    "direction": "in"
},
{
    "type": "webPubSub",
    "name": "webPubSubEvent",
    "hub": "simplechat",
    "direction": "out"
}
```
```js
module.exports = async function (context, connectionContext) {
    context.bindings.messageData = [{
        "message": context.bindingData.message
        "dataType": "text"
    }];
    context.done();
};
```

To send multiple requests, customer need to work with generic `WebPubSubEvent` and do multiple tasks in order.

* csharp usage:
```cs
[FunctionName("connected")]
public static async Task Connected(
    [WebPubSubTrigger(Hub = "simplechat", EventName = "connected", EventType = "system")] ConnectionContext context,
    [WebPubSub] IAsyncCollector<WebPubSubEvent> eventHandler)
{
    await eventHandler.AddAsync(new WebPubSubEvent
    {
        Operation = WebPubSubOperation.SendToAll,
        Message = GetStream(new ClientContent($"{context.UserId} connected.").ToString()),
        DataType = MessageDataType.Json
    });
    await eventHandler.AddAsync(new WebPubSubEvent
    {
        Operation = WebPubSubOperation.AddUserToGroup,
        UserId = context.UserId,
        GroupId = "group1"
    });
    await eventHandler.AddAsync(new WebPubSubEvent
    {
        Operation = WebPubSubOperation.SendToUser,
        UserId = context.UserId,
        Message = GetStream(new ClientContent($"{context.UserId} joined group: group1.").ToString()),
        DataType = MessageDataType.Json
    });
}
```

* javascript usage
```js
"bindings": [
  {
    "type": "webPubSubTrigger",
    "direction": "in",
    "name": "abc",
    "hub": "simplechat",
    "eventName": "connected",
    "eventType": "system"
  },
  {
    "type": "webPubSub",
    "name": "webPubSubEvent",
    "hub": "simplechat",
    "direction": "out"
  }
]
```
```js
module.exports = function (context, connectionContext) {
  context.bindings.webPubSubEvent = [];

  context.bindings.webPubSubEvent.push({
    "operation": "sendToAll",
    "message": {
      "body": JSON.stringify({
          from: '[System]',
          content: `${connectionContext.userId} connected.`
      }),
      "dataType": "json"
    }
  });

  context.bindings.webPubSubEvent.push({
    "operation": "addUserToGroup",
    "userId": `${connectionContext.userId}`,
    "groupId": "group1"
  });

  context.bindings.webPubSubEvent.push({
    "operation": "sendToAll",
    "message": {
      "body": JSON.stringify({
          from: '[System]',
          content: `${connectionContext.userId} joined group: group1.`
      }),
      "dataType": "json"
    }
    
  });
  context.done();
};
```

> When SDK has better supports, server side could work with server sdk convenience layer methods without output binding data type limited. And method response will have enrich properties.
> ```cs
> [FunctionName("message")]
> public static async Task Message(
>     [WebPubSubTrigger(Hub = "simplechat", EventName = "message", EventType = "user")] ConnectionContext context,
>     Stream message,
>     MessageDataType dataType)
> {
>     var server = context.GetWebPubSubServer();
>     await server.Users.AddToGroupAsync(context.UserId, "group1");
>     await server.All.SendAsync(message, dataType);
> }
> ```

### Supported object types for Output bindings.

#### WebPubSubEvent 
`WebPubSubEvent` is an object contains all the properties user can set to invoke rest calls to service. Among the properties, `Operation` is required which matches rest api method names in swagger file. In the initial version, operations listed below are supported. Rest fields should be set depends on the operation type, and will fail if missed or with wrong values.

Name|Type|IsRequired|Description
--|--|--|--
Operation|`WebPubSubOperation`|True|SendToAll</br>CloseClientCOnnection</br>SendToConnection</br>SendTOGroup</br>AddConnectionToGroup</br>RemoveConnectionFromGroup</br>SendToUser</br>AddUserToGroup</br>RemoveUserFromGroup</br>RemoveUserFromAllGroups</br>GrantGroupPermission</br>RevokeGroupPermission</br>
GroupId|`string`|False|group id in operations related to groups
UserId|`string`|False|user id in operations related to user
ConnectionId|`string`|False|connection id in operations related to connection
Excluded|`string[]`|False|list of connection to exlude in operations like SendToAll and SendToGroup
Reason|`string`|False|optional reason when function need to close connection
Permission|`string`|False|permission need to grant/revoke
Message|`Stream`|False|message to send in the send methods
DataType|`MessageDataType`|False|message data type in the send methods

### Abuse Protection

Azure Web PubSub service will deliver client events to the upstream webhook using the CloudEvents HTTP protocol. And service will send `OPTIONS` request to upstream(function/server) following [Abuse Protection](https://github.com/cloudevents/spec/blob/v1.0/http-webhook.md#4-abuse-protection). In Azure Web PubSub service function bindings, this check will be handled by the extension. And customers using functions extension don't have to do anything for this.

> Inner logic: Function will check the host from incoming `OPTIONS` requests with available ones from connection strings. If the host is valid, then return service `200OK` with all available hosts with correct format, else, directly return `400BadRequest`.

