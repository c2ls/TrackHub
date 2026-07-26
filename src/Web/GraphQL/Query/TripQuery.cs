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

using TrackHub.TripManagement.Application.Trips.Queries.GetActiveTrips;
using TrackHub.TripManagement.Application.Trips.Queries.GetTripDetail;
using TrackHub.TripManagement.Application.Trips.Queries.GetTripPodReportData;
using TrackHub.TripManagement.Application.Trips.Queries.GetTripReportData;
using TrackHub.TripManagement.Application.Trips.Queries.GetTripRouteReplay;
using TrackHub.TripManagement.Application.Trips.Queries.GetTripStopReportData;
using TrackHub.TripManagement.Application.Trips.Queries.GetTripTollReportData;
using TrackHub.TripManagement.Application.Trips.Queries.GetTrips;
using TrackHub.TripManagement.Application.Trips.Queries.GetTripTimeline;

namespace TrackHub.TripManagement.Web.GraphQL.Query;

/// <summary>Trip read surface. Resolvers only dispatch — every rule lives in the handler.</summary>
public partial class Query
{
    public async Task<TripsPageVm> GetTrips([Service] ISender sender, [AsParameters] GetTripsQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<TripDetailVm> GetTripDetail([Service] ISender sender, [AsParameters] GetTripDetailQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<IReadOnlyCollection<TripVm>> GetActiveTrips([Service] ISender sender, CancellationToken cancellationToken)
        => await sender.Send(new GetActiveTripsQuery(), cancellationToken);

    public async Task<TripTimelinePageVm> GetTripTimeline([Service] ISender sender, [AsParameters] GetTripTimelineQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<RouteReplayVm> GetTripRouteReplay([Service] ISender sender, [AsParameters] GetTripRouteReplayQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    // The four Reporting export feeds (spec 11 §13). Trip-, stop-, station- and POD-level, because
    // the six catalogued reports live at four different grains.
    public async Task<TripReportPageVm> GetTripReportData([Service] ISender sender, [AsParameters] GetTripReportDataQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<TripStopReportPageVm> GetTripStopReportData([Service] ISender sender, [AsParameters] GetTripStopReportDataQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<TripTollReportPageVm> GetTripTollReportData([Service] ISender sender, [AsParameters] GetTripTollReportDataQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<TripPodReportPageVm> GetTripPodReportData([Service] ISender sender, [AsParameters] GetTripPodReportDataQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);
}
