// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

module.exports = function (context, invocation) {
  context.bindings.eventhandler = [{
    "message": JSON.stringify(
      {
          from: '[System]',
          content: `${invocation.userId} connected.`
      })
  }];

  context.bindings.eventhandler = [{
    "targetType": "users",
    "targetId": `${context.userId}`,
    "action": "add",
    "groupId": "group1"
  }];

  context.bindings.eventhandler = [{
    "message": JSON.stringify(
      {
          from: '[System]',
          content: `${invocation.userId} joined group: group1.`
      })
  }];
  context.done();
};