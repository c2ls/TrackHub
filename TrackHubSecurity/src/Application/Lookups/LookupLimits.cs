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

using FluentValidation.Results;

namespace TrackHub.Security.Application.Lookups;

/// <summary>
/// The ceiling every picker lookup shares. Lookups are deliberately unpaged — the allocator dialogs
/// subtract one list from another, and a truncated operand silently corrupts the difference — so the
/// reader fetches <see cref="Ceiling"/> + 1 rows and the handler raises when the extra row arrives.
/// </summary>
public static class LookupLimits
{
    /// <summary>Largest lookup a picker may bind.</summary>
    public const int Ceiling = 5000;

    /// <summary>Rows to request: one past the ceiling, so an over-ceiling set is detectable.</summary>
    public const int FetchSize = Ceiling + 1;

    public const string LimitExceededCode = "LOOKUP_LIMIT_EXCEEDED";

    /// <summary>
    /// Returns <paramref name="rows"/> unchanged when it fits the ceiling; raises otherwise.
    /// </summary>
    public static IReadOnlyCollection<T> EnsureWithinCeiling<T>(IReadOnlyCollection<T> rows, string lookupName)
        => rows.Count <= Ceiling
            ? rows
            : throw new Common.Application.Exceptions.ValidationException(LimitExceededCode,
            [
                new ValidationFailure(lookupName,
                    $"The {lookupName} lookup returned more than {Ceiling} records. Narrow the request instead of binding a truncated list.")
            ]);
}

/// <summary>
/// The bound for the seeded permission catalogs — roles, policies, resources, actions. These are
/// fixed-size sets that only ever change with a code change, and they are the AXES of the permission
/// matrix: paging them would render a matrix missing rows or columns, which is worse than useless.
/// So they stay whole, and a catalog that has grown past anything a seeded set should reach raises
/// loudly instead of being trimmed to fit.
/// </summary>
public static class SeededCatalogLimits
{
    public const int Ceiling = 1000;

    public const string LimitExceededCode = "SEEDED_CATALOG_LIMIT_EXCEEDED";

    public static IReadOnlyCollection<T> EnsureWithinCeiling<T>(IReadOnlyCollection<T> rows, string catalogName)
        => rows.Count <= Ceiling
            ? rows
            : throw new Common.Application.Exceptions.ValidationException(LimitExceededCode,
            [
                new ValidationFailure(catalogName,
                    $"The {catalogName} catalog holds more than {Ceiling} entries, far past the size a seeded catalog should reach.")
            ]);
}
