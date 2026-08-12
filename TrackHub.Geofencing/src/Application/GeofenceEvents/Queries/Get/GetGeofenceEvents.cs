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

namespace TrackHub.Geofencing.Application.GeofenceEvents.Queries.Get;

[Authorize(Resource = Resources.Geofencing, Action = Actions.Read)]
public readonly record struct GetGeofenceEventsQuery(
    DateTimeOffset From,
    DateTimeOffset To,
    Guid? TransporterId,
    Guid? GeofenceId,
    bool? OpenOnly,
    int? Skip,
    int? Take) : IRequest<GeofenceEventsPageVm>;

public class GetGeofenceEventsQueryHandler(IGeofenceEventReader reader, IUserReader userReader, IUser user, IAccountFeatureReader accountFeatureReader)
    : IRequestHandler<GetGeofenceEventsQuery, GeofenceEventsPageVm>
{
    // Same page-size ceiling as geofencesByAccount — consumers needing the full set
    // (e.g. Reporting) drain pages.
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 500;

    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<GeofenceEventsPageVm> Handle(GetGeofenceEventsQuery request, CancellationToken cancellationToken)
    {
        var userData = await userReader.GetUserAsync(UserId, cancellationToken);
        await accountFeatureReader.EnsureFeatureEnabledAsync(userData.AccountId, FeatureKeys.Geofencing, cancellationToken);

        var skip = Math.Max(request.Skip ?? 0, 0);
        var take = Math.Clamp(request.Take ?? DefaultPageSize, 1, MaxPageSize);
        return await reader.GetGeofenceEventsAsync(
            userData.AccountId,
            UserId,
            request.From,
            request.To,
            request.TransporterId,
            request.GeofenceId,
            request.OpenOnly ?? false,
            skip,
            take,
            cancellationToken);
    }
}

public sealed class GetGeofenceEventsValidator : AbstractValidator<GetGeofenceEventsQuery>
{
    // Bounded window keeps the event query from scanning unbounded history.
    public const int MaxWindowDays = 92;

    public GetGeofenceEventsValidator()
    {
        RuleFor(v => v.From)
            .NotEmpty();

        RuleFor(v => v.To)
            .NotEmpty()
            .GreaterThan(v => v.From)
            .WithMessage("'To' must be after 'From'.");

        RuleFor(v => v)
            .Must(v => (v.To - v.From) <= TimeSpan.FromDays(MaxWindowDays))
            .WithMessage($"The date window cannot exceed {MaxWindowDays} days.")
            .WithName(nameof(GetGeofenceEventsQuery.To));
    }
}
