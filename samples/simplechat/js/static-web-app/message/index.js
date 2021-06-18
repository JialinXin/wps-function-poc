// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

module.exports = async function (context, req, wpsReq) {
  console.log("received message");
  console.log(`is abuse: ${wpsReq.isAbuseRequest}`);
  console.log(`response: ${wpsReq.response}`);
  if (wpsReq.isAbuseRequest)
  {
    return wpsReq.response;
  }
  else {
    context.bindings.webPubSubEvent = {
      "operationKind": "sendToAll",
      "message": wpsReq.request.message,
      "dataType": wpsReq.request.dataType
    };
    return { body: { from: '[System]', content: 'ack.'} };
  }

  context.done();
};