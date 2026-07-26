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

using Common.Application.Interfaces;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Trip;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

/// <summary>
/// Drains the TripManagement report feeds for the spec 11 §13 reports. Query-only, so the client is
/// registered with retry resilience.
/// </summary>
public class TripReportReader(IGraphQLClientFactory graphQLClient, IUser user, IAccountFeatureReader featureReader)
    : GraphQLService(graphQLClient.CreateClient(Clients.TripManagement)), ITripReportReader
{
    // Single source of truth for the queries this reader sends; the ServiceContracts tests validate
    // these exact strings against the TripManagement schema.
    //
    // `tripReportData` is the trip-level feed (GetTripReportDataQuery, spec 11 §7.5). The other three
    // are the stop-, toll- and POD-level feeds the stop/dwell/toll/POD reports need — TripVm carries
    // none of that detail. All four take the same filter arguments and return { items, totalCount }.

    internal const string TripReportDataQuery = @"
                query($accountId: UUID!, $from: DateTime!, $to: DateTime!, $transporterId: UUID, $driverId: UUID, $skip: Int!, $take: Int!) {
                    tripReportData(query: { accountId: $accountId, from: $from, to: $to, transporterId: $transporterId, driverId: $driverId, skip: $skip, take: $take }) {
                        items {
                            tripId
                            code
                            status
                            transporterId
                            transporterName
                            driverId
                            driverName
                            customerName
                            plannedStartAt
                            plannedEndAt
                            actualStartAt
                            actualEndAt
                            plannedDistanceMeters
                            actualDistanceMeters
                            stopCount
                            estimatedTollAmount
                            tollCurrency
                            tollStatus
                        }
                        totalCount
                        nextSkip
                        hasMore
                    }
                }";

    internal const string TripStopReportDataQuery = @"
                query($accountId: UUID!, $from: DateTime!, $to: DateTime!, $transporterId: UUID, $driverId: UUID, $skip: Int!, $take: Int!) {
                    tripStopReportData(query: { accountId: $accountId, from: $from, to: $to, transporterId: $transporterId, driverId: $driverId, skip: $skip, take: $take }) {
                        items {
                            tripStopId
                            tripId
                            tripCode
                            transporterName
                            driverName
                            customerName
                            sequence
                            name
                            status
                            plannedArrivalFrom
                            plannedArrivalTo
                            actualArrivalAt
                            actualDepartureAt
                            deliveryCount
                            deliveredCount
                            failedDeliveryCount
                            partialDeliveryCount
                        }
                        totalCount
                        nextSkip
                        hasMore
                    }
                }";

    internal const string TripTollReportDataQuery = @"
                query($accountId: UUID!, $from: DateTime!, $to: DateTime!, $transporterId: UUID, $driverId: UUID, $skip: Int!, $take: Int!) {
                    tripTollReportData(query: { accountId: $accountId, from: $from, to: $to, transporterId: $transporterId, driverId: $driverId, skip: $skip, take: $take }) {
                        items {
                            tripId
                            tripCode
                            routePlanId
                            plannedStartAt
                            tollVehicleClass
                            tollStationId
                            stationName
                            stationCode
                            roadName
                            direction
                            amount
                            currency
                            hasTariff
                        }
                        totalCount
                        nextSkip
                        hasMore
                    }
                }";

    internal const string TripPodReportDataQuery = @"
                query($accountId: UUID!, $from: DateTime!, $to: DateTime!, $transporterId: UUID, $driverId: UUID, $skip: Int!, $take: Int!) {
                    tripPodReportData(query: { accountId: $accountId, from: $from, to: $to, transporterId: $transporterId, driverId: $driverId, skip: $skip, take: $take }) {
                        items {
                            proofOfDeliveryId
                            tripId
                            tripCode
                            tripStopId
                            stopSequence
                            stopName
                            receiverName
                            receiverDocument
                            capturedAt
                            latitude
                            longitude
                            documentCount
                        }
                        totalCount
                        nextSkip
                        hasMore
                    }
                }";

    // Producer-side page clamp is 500; loop until the page count is reached. The row ceiling is a
    // defensive source-fetch cap — the governed export limit is enforced downstream
    // (AppSettings:Reporting), the same rationale as GeofenceReader.MaxRows.
    private const int PageSize = 500;
    private const int MaxRows = 100_000;

    private Guid AccountId => user.AccountId ?? throw new UnauthorizedAccessException();

    public Task EnsureTripManagementFeatureAsync(CancellationToken cancellationToken)
        => featureReader.EnsureFeatureEnabledAsync(AccountId, FeatureKeys.TripManagement, cancellationToken);

    public Task<IReadOnlyCollection<ReportTripVm>> GetTripsAsync(
        DateTimeOffset? from, DateTimeOffset? to, Guid? transporterId, Guid? driverId, CancellationToken cancellationToken)
        => DrainAsync<ReportTripVm, TripReportDataPageVm>(
            TripReportDataQuery, p => p.Items, p => p.NextSkip, p => p.HasMore, from, to, transporterId, driverId, cancellationToken);

    public Task<IReadOnlyCollection<ReportTripStopVm>> GetTripStopsAsync(
        DateTimeOffset? from, DateTimeOffset? to, Guid? transporterId, Guid? driverId, CancellationToken cancellationToken)
        => DrainAsync<ReportTripStopVm, TripStopReportDataPageVm>(
            TripStopReportDataQuery, p => p.Items, p => p.NextSkip, p => p.HasMore, from, to, transporterId, driverId, cancellationToken);

    public Task<IReadOnlyCollection<ReportTripTollVm>> GetTripTollsAsync(
        DateTimeOffset? from, DateTimeOffset? to, Guid? transporterId, Guid? driverId, CancellationToken cancellationToken)
        => DrainAsync<ReportTripTollVm, TripTollReportDataPageVm>(
            TripTollReportDataQuery, p => p.Items, p => p.NextSkip, p => p.HasMore, from, to, transporterId, driverId, cancellationToken);

    public Task<IReadOnlyCollection<ReportTripPodVm>> GetTripProofsOfDeliveryAsync(
        DateTimeOffset? from, DateTimeOffset? to, Guid? transporterId, Guid? driverId, CancellationToken cancellationToken)
        => DrainAsync<ReportTripPodVm, TripPodReportDataPageVm>(
            TripPodReportDataQuery, p => p.Items, p => p.NextSkip, p => p.HasMore, from, to, transporterId, driverId, cancellationToken);

    // One drain loop for all four feeds, driven by the PRODUCER'S cursor.
    //
    // It used to skip by rows.Count and stop on rows.Count >= totalCount. That silently assumed
    // every feed pages in the same unit it returns rows in — true for three of them, false for the
    // toll feed, which pages route plans and expands each into one row per matched station. With
    // 700 plans averaging three stations, page one returned ~1500 rows against a totalCount of 700,
    // the loop terminated immediately, and trip-toll-cost under-reported by 200 trips with no
    // error anywhere. Following NextSkip/HasMore removes the assumption entirely.
    private async Task<IReadOnlyCollection<TRow>> DrainAsync<TRow, TPage>(
        string query,
        Func<TPage, IEnumerable<TRow>?> itemsOf,
        Func<TPage, int> nextSkipOf,
        Func<TPage, bool> hasMoreOf,
        DateTimeOffset? from,
        DateTimeOffset? to,
        Guid? transporterId,
        Guid? driverId,
        CancellationToken cancellationToken)
    {
        var accountId = AccountId;
        var rows = new List<TRow>();
        var skip = 0;

        while (rows.Count < MaxRows)
        {
            var request = new GraphQLRequest
            {
                Query = query,
                Variables = new
                {
                    accountId,
                    from,
                    to,
                    transporterId,
                    driverId,
                    skip,
                    take = PageSize
                }
            };

            var page = await QueryAsync<TPage>(request, cancellationToken);
            var pageItems = itemsOf(page);
            var items = pageItems as ICollection<TRow> ?? [.. pageItems ?? []];

            rows.AddRange(items);

            if (!hasMoreOf(page))
            {
                break;
            }

            var nextSkip = nextSkipOf(page);
            if (nextSkip <= skip)
            {
                // The producer did not advance. Bail rather than spin: a feed that reports HasMore
                // without moving its cursor is a bug on the producer side, and an infinite loop
                // here would hang a report request instead of surfacing it.
                break;
            }

            skip = nextSkip;
        }

        return rows;
    }
}
