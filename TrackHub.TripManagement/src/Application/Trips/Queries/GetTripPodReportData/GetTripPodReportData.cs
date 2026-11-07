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

namespace TrackHub.TripManagement.Application.Trips.Queries.GetTripPodReportData;

/// <summary>
/// POD register export feed behind <c>trip-pod-export</c>. Rows carry a document COUNT, never the
/// documents: this is a register, and the bytes stay behind Manager's access policy. Rows carry
/// receiver names, identity documents and signature coordinates, and the export audits through the
/// same governed path as every other report — severity classification is spec 24a's scope.
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Export, PrincipalTypes = "User,ServiceClient")]
[RequireFeature(FeatureKeys.TripManagement)]
// Enforcement: the feed filters on AccountId and the report scope; the other keys only narrow.
[AccountScopeEnforcedInHandler]
public readonly record struct GetTripPodReportDataQuery(
    Guid AccountId,
    DateTimeOffset From,
    DateTimeOffset To,
    Guid? TransporterId,
    Guid? StaffId,
    int? Skip,
    int? Take) : IRequest<TripPodReportPageVm>;

public sealed class GetTripPodReportDataQueryHandler(
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<GetTripPodReportDataQuery, TripPodReportPageVm>
{
    public async Task<TripPodReportPageVm> Handle(GetTripPodReportDataQuery request, CancellationToken cancellationToken)
    {
        var scopeUserId = await TripVisibility.ResolveReportScopeAsync(user, userReader, request.AccountId, cancellationToken);
        var (skip, take) = ReportPaging.Clamp(request.Skip, request.Take);

        return await reader.GetTripPodReportRowsAsync(
            request.AccountId, scopeUserId, request.From, request.To,
            request.TransporterId, request.StaffId, skip, take, cancellationToken);
    }
}

public sealed class GetTripPodReportDataValidator : AbstractValidator<GetTripPodReportDataQuery>
{
    public GetTripPodReportDataValidator()
    {
        RuleFor(v => v.AccountId).NotEmpty();
        RuleFor(v => v.To).GreaterThanOrEqualTo(v => v.From);
        RuleFor(v => v.Skip).GreaterThanOrEqualTo(0).When(v => v.Skip.HasValue);
        RuleFor(v => v.Take).InclusiveBetween(1, ReportPaging.MaxPageSize).When(v => v.Take.HasValue);
    }
}
