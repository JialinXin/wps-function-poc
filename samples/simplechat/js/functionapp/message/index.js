// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

module.exports = async function (context, data) {
  context.bindings.actions = {
    "actionName": "sendToAll",
    "data": JSON.stringify(data),
    "dataType": "text"
  };
  var response = { 
    "data": JSON.stringify(data),
    "dataType" : "text"
  };
  return response;
};