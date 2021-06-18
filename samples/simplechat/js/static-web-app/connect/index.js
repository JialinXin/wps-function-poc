// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

module.exports = function (context, req) {
  //if (context.bindings.wpsReq.isAbuseRequest)
  //{
  //  console.log("response abuse");
  //  context.res = {
  //    body: "abuse pass",
  //    status: 200,
  //    headers: {
  //      "WebHook-Allowed-Origin": "localhost"
  //    }
  //  };
  //}
  //else
  //{
    console.log("response connect");
    console.log(context.bindings.wpsReq.connectionContext.userId);
    context.res = { body: {"userId": context.bindings.wpsReq.connectionContext.userId} };
  //}
  context.done();
};