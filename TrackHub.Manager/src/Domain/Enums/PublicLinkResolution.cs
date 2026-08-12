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

namespace TrackHub.Manager.Domain.Enums;

/// <summary>
/// Outcome of resolving an anonymous public link. These are normal answers, not faults:
/// "no such grant", "revoked" and "scope not granted" are deliberately indistinguishable
/// (<see cref="NotFound"/>) so a caller cannot probe grant existence, while
/// <see cref="Expired"/> is disclosable because the holder legitimately had the link.
/// <para>
/// Maps to the anonymous REST contract as 404 (<see cref="NotFound"/>),
/// 410 (<see cref="Expired"/>) and 200 (<see cref="Found"/>), and travels over GraphQL as
/// the enum literals <c>FOUND</c> / <c>NOT_FOUND</c> / <c>EXPIRED</c>.
/// </para>
/// </summary>
public enum PublicLinkResolution
{
    Found = 0,
    NotFound = 1,
    Expired = 2
}
