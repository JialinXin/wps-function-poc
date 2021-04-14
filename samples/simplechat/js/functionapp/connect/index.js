// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

module.exports = async function (context, abc) {
  //context.log(`Connection ${connectionContext.connectionId} connect.`);
  //context.log(`SubProtocols = ${context.bindingData.subprotocols}`);
  if (abc.userId == "attacker")
  {
    var connectResponse = {
      "code": "unauthorized",
      "errorMessage": "invalid user"
    }
  }
  else 
  {
    var connectResponse = {
      "userId": abc.userId
    };
  }
  return connectResponse;
};

