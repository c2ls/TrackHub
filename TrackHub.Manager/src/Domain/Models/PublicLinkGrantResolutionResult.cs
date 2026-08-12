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

namespace TrackHub.Manager.Domain.Models;

/// <summary>
/// Manager-internal result of <see cref="IPublicLinkGrantResolver"/>: the discriminated outcome plus
/// the full grant on success. <see cref="Grant"/> is populated only when <see cref="Resolution"/> is
/// <see cref="PublicLinkResolution.Found"/>.
/// <para>
/// The anonymous REST endpoint projects this into its long-standing JSON body (scopes, purpose,
/// expiry, access counters). The ServiceClient GraphQL mutation projects it down to the much narrower
/// <see cref="PublicLinkResolutionVm"/>. One resolution implementation, two projections.
/// </para>
/// </summary>
public readonly record struct PublicLinkGrantResolutionResult(PublicLinkResolution Resolution, PublicLinkGrantVm? Grant);
