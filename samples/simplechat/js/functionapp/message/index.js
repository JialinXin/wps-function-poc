// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

module.exports = async function (context, message) {
  context.bindings.webPubSubEvent = {
    "operationKind": "sendToAll",
    "message": JSON.stringify(message),
    "dataType": "text"
  };
  var response = { 
    "message": JSON.stringify(message),
    "dataType" : "text"
  };
  return response;
};