// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    internal class HubRegistry
    {
        public WebPubSubHub Hub { get; }

        public string Path { get; }

        public HubRegistry(WebPubSubHub hub, string path)
        {
            Hub = hub;
            Path = path;
        }
    }
}
