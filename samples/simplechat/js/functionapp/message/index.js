// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

module.exports = async function (context, message) {
  context.bindings.webPubSubEvent = {
    "operationKind": "sendToAll",
    "message": message,
    "dataType": context.bindingData.dataType
  };
  var response = { 
    "message": { from: '[System]', content: 'ack.'},
    "dataType" : "json"
  };
  return response;
};