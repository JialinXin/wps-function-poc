// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

module.exports = function (context, connectionContext) {
  context.bindings.webPubSubEvent = [];
  context.bindings.webPubSubEvent.push({
    "operationKind": "sendToAll",
    "message": JSON.stringify({
        from: '[System]',
        content: `${context.bindingData.connectionContext.userId} connected.`
      }),
    "dataType" : "json"
  });

  context.bindings.webPubSubEvent.push({
    "operationKind": "addUserToGroup",
    "userId": `${context.bindingData.connectionContext.userId}`,
    "group": "group1"
  });

  context.bindings.webPubSubEvent.push({
    "operationKind": "sendToAll",
    "message": JSON.stringify({
          from: '[System]',
          content: `${context.bindingData.connectionContext.userId} joined group: group1.`
      }),
    "dataType": "json"
  });
  context.done();
};