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
using TrackHub.TripManagement.Application.Common;

namespace TrackHub.TripManagement.Application.Trips.Queries.GetTripTollReportData;

// No [Caching] — scope comes from the caller identity (SVD-09). See GetTripsQuery.
/// <summary>
/// Station-level export feed behind <c>trip-toll-cost</c>: one row per route-plan/station match.
/// <para>
/// A match with no tariff for the trip's class on the plan date comes back with a <b>null</b>
/// amount and <c>hasTariff = false</c>. That pair is what the report renders as its
/// <c>PartialNoTariff</c> column — a zero here would net a catalog gap silently into the total and
/// present an understated cost as a fact (spec 11 §18.9, acceptance 21).
/// </para>
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Export, PrincipalTypes = "User,ServiceClient")]
[RequireFeature(FeatureKeys.TripManagement)]
public readonly record struct GetTripTollReportDataQuery(
    Guid AccountId,
    DateTimeOffset From,
    DateTimeOffset To,
    Guid? TransporterId,
    Guid? DriverId,
    int? Skip,
    int? Take) : IRequest<TripTollReportPageVm>;

public sealed class GetTripTollReportDataQueryHandler(
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<GetTripTollReportDataQuery, TripTollReportPageVm>
{
    public async Task<TripTollReportPageVm> Handle(GetTripTollReportDataQuery request, CancellationToken cancellationToken)
    {
        var scopeUserId = await TripVisibility.ResolveReportScopeAsync(user, userReader, request.AccountId, cancellationToken);
        var (skip, take) = ReportPaging.Clamp(request.Skip, request.Take);

        return await reader.GetTripTollReportRowsAsync(
            request.AccountId, scopeUserId, request.From, request.To,
            request.TransporterId, request.DriverId, skip, take, cancellationToken);
    }
}

public sealed class GetTripTollReportDataValidator : AbstractValidator<GetTripTollReportDataQuery>
{
    public GetTripTollReportDataValidator()
    {
        RuleFor(v => v.AccountId).NotEmpty();
        RuleFor(v => v.To).GreaterThanOrEqualTo(v => v.From);
        RuleFor(v => v.Skip).GreaterThanOrEqualTo(0).When(v => v.Skip.HasValue);
        RuleFor(v => v.Take).InclusiveBetween(1, ReportPaging.MaxPageSize).When(v => v.Take.HasValue);
    }
}
