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

using Npgsql;

namespace TrackHub.TripManagement.Infrastructure.TripDB.Writers;

/// <summary>
/// Recognises a PostgreSQL unique-constraint violation behind an EF <see cref="DbUpdateException"/>.
/// <para>
/// Idempotency in this module is enforced by the DATABASE, not by a read-then-write check: a
/// pre-flight <c>AnyAsync</c> loses the race under two concurrent retries of the same offline
/// submission. The writers attempt the insert and translate 23505 on the known index into the
/// "already existed" answer (acceptance 15).
/// </para>
/// </summary>
internal static class UniqueViolation
{
    private const string UniqueViolationSqlState = "23505";

    internal static bool Matches(DbUpdateException exception, string indexName)
        => exception.InnerException is PostgresException postgres
            && string.Equals(postgres.SqlState, UniqueViolationSqlState, StringComparison.Ordinal)
            && (string.IsNullOrEmpty(postgres.ConstraintName)
                || postgres.ConstraintName.Contains(indexName, StringComparison.OrdinalIgnoreCase));

    internal static bool Matches(DbUpdateException exception)
        => exception.InnerException is PostgresException postgres
            && string.Equals(postgres.SqlState, UniqueViolationSqlState, StringComparison.Ordinal);
}
