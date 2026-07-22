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

namespace TrackHub.TripManagement.Application.Trips.Queries.GetTrips;

// No [Caching] on this — or on ANY query in this module. CachingBehavior builds its cache key from
// the request's own properties, and every trip query resolves its account and its group scope from
// the CALLER's identity rather than from the request. A cached page would therefore be replayed to
// the next caller with the same filters, across accounts and across groups (findings.md SVD-09,
// proved by the geofencing case). Scope-from-identity and request-keyed caching are mutually
// exclusive; identity wins.
/// <summary>Paged dispatch board, group-filtered through <c>trip.vw_visible_transporter</c>.</summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Read)]
[RequireFeature(FeatureKeys.TripManagement)]
public readonly record struct GetTripsQuery(
    IReadOnlyCollection<string>? Statuses,
    DateTimeOffset? From,
    DateTimeOffset? To,
    Guid? TransporterId,
    Guid? DriverId,
    string? Customer,
    string? Search,
    int? Skip,
    int? Take) : IRequest<TripsPageVm>;

public sealed class GetTripsQueryHandler(
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<GetTripsQuery, TripsPageVm>
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 200;

    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<TripsPageVm> Handle(GetTripsQuery request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var skip = Math.Max(request.Skip ?? 0, 0);
        var take = Math.Clamp(request.Take ?? DefaultPageSize, 1, MaxPageSize);

        return await reader.GetTripsPageAsync(
            caller.AccountId,
            TripVisibility.ResolveScopeUserId(user, UserId),
            request.Statuses,
            request.From,
            request.To,
            request.TransporterId,
            request.DriverId,
            request.Customer,
            request.Search,
            skip,
            take,
            cancellationToken);
    }
}

public sealed class GetTripsValidator : AbstractValidator<GetTripsQuery>
{
    public GetTripsValidator()
    {
        RuleFor(v => v.Skip).GreaterThanOrEqualTo(0).When(v => v.Skip.HasValue);
        RuleFor(v => v.Take).InclusiveBetween(1, 200).When(v => v.Take.HasValue);
        RuleForEach(v => v.Statuses)
            .Must(TripStatuses.IsValid)
            .When(v => v.Statuses is not null)
            .WithMessage("Unknown trip status.");
    }
}
