﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.WebJobs.Extensions.WebPubSub
{
    public enum WebPubSubRequestStatus
    {
        RequestValid,
        FormatInvalid,
        SignatureInvalid,
        ContentTypeInvalid,
        Unknown
    }
}