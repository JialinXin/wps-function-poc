// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

module.exports = function (context, connectionContext) {
  context.bindings.webPubSubEvent = [{
    "operation": "sendToAll",
    "message": JSON.stringify(
      {
          from: '[System]',
          content: `${connectionContext.userId} connected.`
      }),
      "dataType": "text"
  }];

  context.bindings.webPubSubEvent = [{
    "operation": "addUserToGroup",
    "userId": `${connectionContext.userId}`,
    "groupId": "group1"
  }];

  context.bindings.webPubSubEvent = [{
    "operation": "sendToAll",
    "message": JSON.stringify(
      {
          from: '[System]',
          content: `${connectionContext.userId} joined group: group1.`
      }),
      "dataType": "text"
  }];
  context.done();
};