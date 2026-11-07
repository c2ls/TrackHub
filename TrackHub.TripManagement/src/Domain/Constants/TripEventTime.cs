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

namespace TrackHub.TripManagement.Domain.Constants;

// A client-supplied event instant is accepted only if it is not in the future beyond clock skew,
// not before the moment it has to follow, and, for a staff, not further back than the offline
// window the app can hold events for.
public static class TripEventTime
{
    public static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan StaffBackdateWindow = TimeSpan.FromDays(7);

    public static bool IsAcceptable(DateTimeOffset occurredAt, string source, DateTimeOffset? notBefore, DateTimeOffset now)
        => occurredAt <= now + ClockSkew
            && (notBefore is not { } floor || occurredAt >= floor)
            && (!string.Equals(source, TripEventSources.Staff, StringComparison.Ordinal) || occurredAt >= now - StaffBackdateWindow);
}
