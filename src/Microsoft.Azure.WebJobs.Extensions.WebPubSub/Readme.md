<?xml version="1.0"?>
<doc>
    <assembly>
        <name>Microsoft.Azure.WebJobs.Extensions.WebPubSub</name>
    </assembly>
    <members>
        <member name="P:Microsoft.Azure.WebJobs.Extensions.WebPubSub.ConnectionContext.EventType">
            <summary>
            The type of the message.
            </summary>
        </member>
        <member name="P:Microsoft.Azure.WebJobs.Extensions.WebPubSub.ConnectionContext.EventName">
            <summary>
            The event name of the message.
            </summary>
        </member>
        <member name="P:Microsoft.Azure.WebJobs.Extensions.WebPubSub.ConnectionContext.Hub">
            <summary>
            The hub which the message belongs to.
            </summary>
        </member>
        <member name="P:Microsoft.Azure.WebJobs.Extensions.WebPubSub.ConnectionContext.ConnectionId">
            <summary>
            The connection-id of the client which send the message.
            </summary>
        </member>
        <member name="P:Microsoft.Azure.WebJobs.Extensions.WebPubSub.ConnectionContext.UserId">
            <summary>
            The user identity of the client which send the message.
            </summary>
        </member>
        <member name="P:Microsoft.Azure.WebJobs.Extensions.WebPubSub.ConnectionContext.Signature">
            <summary>
            The signature for validation
            </summary>
        </member>
        <member name="P:Microsoft.Azure.WebJobs.Extensions.WebPubSub.ConnectionContext.Headers">
            <summary>
            The headers of request.
            </summary>
        </member>
        <member name="M:Microsoft.Azure.WebJobs.Extensions.WebPubSub.WebPubSubOptions.Format">
            <summary>
            Formats the options as JSON objects for display.
            </summary>
            <returns>Options formatted as JSON.</returns>
        </member>
        <member name="M:Microsoft.Azure.WebJobs.Extensions.WebPubSub.WebPubSubTriggerAttribute.#ctor(System.String,Microsoft.Azure.WebJobs.Extensions.WebPubSub.WebPubSubEventType,System.String)">
            <summary>
            Used to map to method name automatically
            </summary>
            <param name="hub"></param>
            <param name="eventName"></param>
            <param name="eventType"></param>
        </member>
        <member name="P:Microsoft.Azure.WebJobs.Extensions.WebPubSub.WebPubSubTriggerAttribute.Hub">
            <summary>
            The hub of request.
            </summary>
        </member>
        <member name="P:Microsoft.Azure.WebJobs.Extensions.WebPubSub.WebPubSubTriggerAttribute.EventName">
            <summary>
            The event of the request
            </summary>
        </member>
        <member name="P:Microsoft.Azure.WebJobs.Extensions.WebPubSub.WebPubSubTriggerAttribute.EventType">
            <summary>
            The event type, allowed value is system or user
            </summary>
        </member>
        <member name="M:Microsoft.Azure.WebJobs.Extensions.WebPubSub.WebPubSubTriggerBinding.CreateBindingContract(System.Reflection.ParameterInfo)">
            <summary>
            Defined what other bindings can use and return value.
            </summary>
        </member>
        <member name="T:Microsoft.Azure.WebJobs.Extensions.WebPubSub.WebPubSubTriggerBinding.WebPubSubTriggerValueProvider">
            <summary>
            A provider that responsible for providing value in various type to be bond to function method parameter.
            </summary>
        </member>
        <member name="T:Microsoft.Azure.WebJobs.Extensions.WebPubSub.WebPubSubTriggerBinding.TriggerReturnValueProvider">
            <summary>
            A provider to handle return value.
            </summary>
        </member>
        <member name="P:Microsoft.Azure.WebJobs.Extensions.WebPubSub.WebPubSubTriggerEvent.ConnectionContext">
            <summary>
            Web PubSub common request context from cloud event headers.
            </summary>
        </member>
        <member name="P:Microsoft.Azure.WebJobs.Extensions.WebPubSub.WebPubSubTriggerEvent.TaskCompletionSource">
            <summary>
            A TaskCompletionSource will set result when the function invocation has finished.
            </summary>
        </member>
    </members>
</doc>
