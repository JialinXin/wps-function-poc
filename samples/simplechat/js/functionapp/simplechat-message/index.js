// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

module.exports = function (context, invocation) {
  context.bindings.eventhandler = [{
    "message": invocation.payload.span
  }];
  context.done();
};