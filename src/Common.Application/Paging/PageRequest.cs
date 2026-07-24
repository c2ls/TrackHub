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

namespace Common.Application.Paging;

/// <summary>
/// The one page-size policy every paged query shares. Handlers used to carry private
/// <c>DefaultPageSize</c>/<c>MaxPageSize</c> consts, which drifted the moment one was edited in
/// isolation; the clamp lives here so a caller can neither request an unbounded page nor a
/// non-positive one.
/// </summary>
public static class PageRequest
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 500;

    /// <summary>
    /// Normalizes a caller-supplied page window: a missing or negative <paramref name="skip"/>
    /// becomes 0, and a missing <paramref name="take"/> becomes <see cref="DefaultPageSize"/>,
    /// clamped to [1, <see cref="MaxPageSize"/>].
    /// </summary>
    public static (int Skip, int Take) Clamp(int? skip, int? take)
        => (Math.Max(skip ?? 0, 0), Math.Clamp(take ?? DefaultPageSize, 1, MaxPageSize));
}
