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

using Common.Application.Paging;
using Common.Application.Interfaces;
using TrackHub.TripManagement.Application.Common;

namespace TrackHub.TripManagement.Application.Trips.Queries.GetTrips;

/// <summary>Paged dispatch board, group-filtered through <c>trip.vw_visible_transporter</c>.</summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Read)]
[RequireFeature(FeatureKeys.TripManagement)]
// Enforcement: the handler derives the caller's own account and passes it to the reader/writer,
// which filters every row on it (TripVisibility is the single visibility resolver - spec 11).
[AccountScopeEnforcedInHandler]
public readonly record struct GetTripsQuery(
    IReadOnlyCollection<string>? Statuses,
    DateTimeOffset? From,
    DateTimeOffset? To,
    Guid? TransporterId,
    Guid? StaffId,
    string? Customer,
    string? Search,
    int? Skip,
    int? Take,
    string? Exception = null) : IRequest<TripsPageVm>;

public sealed class GetTripsQueryHandler(
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<GetTripsQuery, TripsPageVm>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<TripsPageVm> Handle(GetTripsQuery request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var (skip, take) = PageRequest.Clamp(request.Skip, request.Take);

        return await reader.GetTripsPageAsync(
            caller.AccountId,
            TripVisibility.ResolveScopeUserId(user, UserId),
            request.Statuses,
            request.From,
            request.To,
            request.TransporterId,
            request.StaffId,
            request.Customer,
            request.Search,
            request.Exception,
            skip,
            take,
            cancellationToken);
    }
}

public sealed class GetTripsValidator : AbstractValidator<GetTripsQuery>
{
    public const int MaxWindowDays = 366;

    public GetTripsValidator()
    {
        RuleFor(v => v.Skip).GreaterThanOrEqualTo(0).When(v => v.Skip.HasValue);
        RuleFor(v => v.Take).InclusiveBetween(1, PageRequest.MaxPageSize).When(v => v.Take.HasValue);
        RuleFor(v => v)
            .Must(v => v.From!.Value <= v.To!.Value)
            .When(v => v.From.HasValue && v.To.HasValue)
            .WithName(nameof(GetTripsQuery.To))
            .WithMessage("The date window must end after it starts.");
        RuleFor(v => v)
            .Must(v => v.To!.Value - v.From!.Value <= TimeSpan.FromDays(MaxWindowDays))
            .When(v => v.From.HasValue && v.To.HasValue)
            .WithName(nameof(GetTripsQuery.To))
            .WithMessage($"The date window may span at most {MaxWindowDays} days.");
        RuleForEach(v => v.Statuses)
            .Must(TripStatuses.IsValid)
            .When(v => v.Statuses is not null)
            .WithMessage("Unknown trip status.");
        RuleFor(v => v.Exception)
            .Must(TripExceptions.IsValid)
            .When(v => v.Exception is not null)
            .WithMessage("Unknown trip exception.");
    }
}
