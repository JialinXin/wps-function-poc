// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

module.exports = async function (context, abc) {
  context.bindings.webPubSubEvent = [{
    "operation": "sendToAll",
    "message": context.bindingData.message
  }];
  var response = { 
    "message": {
      "body": JSON.stringify({
        from: '[System]',
        content: 'ack.'
      }),
      "dataType" : "json"
    }
  };
  //context.response = response;
  return response;
};