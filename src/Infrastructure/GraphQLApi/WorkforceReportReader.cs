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

using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

public class WorkforceReportReader(IGraphQLClientFactory graphQLClient, IUser user, IAccountFeatureReader featureReader)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IWorkforceReportReader
{
    // Manager clamps take to 500, so reports page through until exhausted or the report row limit.
    private const int PageSize = 500;
    private const int MaxRows = 100_000;

    internal const string DriversByAccountQuery = @"
                query($accountId: UUID!, $skip: Int!, $take: Int!) {
                    driversByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) {
                        driverId name phone documentType documentNumber active employeeCode licenseNumber licenseExpiresAt defaultTransporterId
                    }
                }";

    internal const string DriverQualificationsQuery = @"
                query($accountId: UUID!, $driverId: UUID, $expiringWithinDays: Int, $skip: Int!, $take: Int!) {
                    driverQualifications(query: { accountId: $accountId, driverId: $driverId, expiringWithinDays: $expiringWithinDays, skip: $skip, take: $take }) {
                        driverQualificationId driverId driverName qualificationType category number issuedAt expiresAt issuingAuthority status
                    }
                }";

    internal const string DriverAssignmentHistoryQuery = @"
                query($accountId: UUID!, $driverId: UUID, $transporterId: UUID, $from: DateTime, $to: DateTime, $skip: Int!, $take: Int!) {
                    driverAssignmentHistory(query: { accountId: $accountId, driverId: $driverId, transporterId: $transporterId, from: $from, to: $to, skip: $skip, take: $take }) {
                        driverId driverName transporterId transporterName startsAt endsAt assignmentType status createdByPrincipal
                    }
                }";

    private Guid AccountId => user.AccountId ?? throw new UnauthorizedAccessException();

    public Task EnsureWorkforceFeatureAsync(CancellationToken cancellationToken)
        => featureReader.EnsureFeatureEnabledAsync(AccountId, FeatureKeys.Workforce, cancellationToken);

    public Task<IReadOnlyCollection<ReportDriverVm>> GetDriversAsync(CancellationToken cancellationToken)
        => FetchAllAsync<ReportDriverVm>((skip, take) => new GraphQLRequest
        {
            Query = DriversByAccountQuery,
            Variables = new { accountId = AccountId, skip, take }
        }, cancellationToken);

    public Task<IReadOnlyCollection<ReportDriverQualificationVm>> GetDriverQualificationsAsync(
        Guid? driverId, int? expiringWithinDays, CancellationToken cancellationToken)
        => FetchAllAsync<ReportDriverQualificationVm>((skip, take) => new GraphQLRequest
        {
            Query = DriverQualificationsQuery,
            Variables = new { accountId = AccountId, driverId, expiringWithinDays, skip, take }
        }, cancellationToken);

    public Task<IReadOnlyCollection<ReportDriverAssignmentVm>> GetDriverAssignmentHistoryAsync(
        Guid? driverId, Guid? transporterId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
        => FetchAllAsync<ReportDriverAssignmentVm>((skip, take) => new GraphQLRequest
        {
            Query = DriverAssignmentHistoryQuery,
            Variables = new { accountId = AccountId, driverId, transporterId, from, to, skip, take }
        }, cancellationToken);

    private async Task<IReadOnlyCollection<T>> FetchAllAsync<T>(Func<int, int, GraphQLRequest> buildRequest, CancellationToken cancellationToken)
    {
        var all = new List<T>();
        var skip = 0;
        // Fetch one page BEYOND the report limit so an over-limit result set reaches ExcelHelper, which
        // then fails clearly (ReportLimitExceededException → 400) rather than silently truncating (AC12).
        while (all.Count <= MaxRows)
        {
            var page = await QueryAsync<List<T>>(buildRequest(skip, PageSize), cancellationToken);
            if (page.Count == 0)
            {
                break;
            }

            all.AddRange(page);
            if (page.Count < PageSize)
            {
                break;
            }

            skip += PageSize;
        }

        return all;
    }
}
