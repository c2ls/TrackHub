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

namespace TrackHub.TripManagement.Application.Common;

/// <summary>
/// Uniqueness probes for <c>Code</c> and <c>ExternalReference</c>, both unique per account.
/// <para>
/// NOTE (Domain contract gap): <see cref="ITripReader"/> exposes no by-code / by-external-reference
/// lookup, so these probe the paged board with the value as the free-text <c>search</c> term and
/// then compare exactly. A dedicated reader method would be cheaper and exact; this is the closest
/// the frozen contract allows and it never returns a false positive because the comparison is on
/// the projected value, not on the search.
/// </para>
/// </summary>
public static class TripLookup
{
    private const int ProbePageSize = 200;

    public static async Task<TripVm?> FindByCodeAsync(ITripReader reader, Guid accountId, string code, CancellationToken cancellationToken)
    {
        var page = await reader.GetTripsPageAsync(accountId, null, null, null, null, null, null, null, code, 0, ProbePageSize, cancellationToken);
        foreach (var trip in page.Items)
        {
            if (string.Equals(trip.Code, code, StringComparison.OrdinalIgnoreCase))
                return trip;
        }

        return null;
    }

    public static async Task<TripVm?> FindByExternalReferenceAsync(ITripReader reader, Guid accountId, string externalReference, CancellationToken cancellationToken)
    {
        var page = await reader.GetTripsPageAsync(accountId, null, null, null, null, null, null, null, externalReference, 0, ProbePageSize, cancellationToken);
        foreach (var trip in page.Items)
        {
            if (string.Equals(trip.ExternalReference, externalReference, StringComparison.OrdinalIgnoreCase))
                return trip;
        }

        return null;
    }
}
