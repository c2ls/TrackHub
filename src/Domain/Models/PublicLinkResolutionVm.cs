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
/// GraphQL-facing result of <c>resolvePublicLinkGrant</c> — deliberately narrow.
/// <para>
/// It carries only the echo a downstream service needs to correlate its own record
/// (<see cref="PublicLinkGrantId"/>, <see cref="ResourceId"/>) and never the grant's scopes,
/// purpose, expiry or access counters: the caller already knows what it asked for, and a
/// ServiceClient resolving a link on a visitor's behalf has no need for the grant's internals.
/// The richer <see cref="PublicLinkGrantResolutionResult"/> stays inside Manager for the anonymous
/// REST endpoint, whose pre-existing body shape is a fixed contract.
/// </para>
/// <para>
/// Both identifiers are null unless <see cref="Resolution"/> is
/// <see cref="PublicLinkResolution.Found"/>, so a rejected caller learns nothing.
/// </para>
/// </summary>
public readonly record struct PublicLinkResolutionVm(
    PublicLinkResolution Resolution,
    Guid? PublicLinkGrantId,
    string? ResourceId);
