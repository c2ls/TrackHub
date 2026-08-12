// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using HotChocolate;
using HotChocolate.Execution;
using TrackHub.Router.Domain.Exceptions;

namespace TrackHub.Router.Web.GraphQL;

// Maps a missing provider capability to a typed error so clients can attribute the limitation
// to the external GPS provider (extensions carry protocol + capability) — distinct from
// FEATURE_DISABLED (TrackHub account gating) and from the masked "Unexpected Execution Error"
// a raw exception would produce.
public sealed class ProviderCapabilityErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
        => error.Exception is ProviderCapabilityNotSupportedException notSupported
            ? ErrorBuilder.FromError(error)
                .SetMessage(notSupported.Message)
                .SetCode("PROVIDER_CAPABILITY_NOT_SUPPORTED")
                .SetExtension("protocol", notSupported.Protocol.ToString())
                .SetExtension("capability", notSupported.Capability.ToString())
                .Build()
            : error;
}
