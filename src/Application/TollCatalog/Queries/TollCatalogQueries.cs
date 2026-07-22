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

using Ardalis.GuardClauses;

namespace TrackHub.TripManagement.Application.TollCatalog.Queries;

// Readable by ANY authenticated account user (spec 11 §5): it describes public road
// infrastructure, not tenant business data. Not feature-flagged, and carrying no accountId —
// there is nothing account-scoped to leak, which is also why [Caching] would be harmless here
// but is still omitted for consistency with the rest of the module.

/// <summary>Paged toll-station browser for the admin panel and the planner.</summary>
[Authorize(Resource = Resources.TollCatalog, Action = Actions.Read)]
public readonly record struct GetTollStationsQuery(
    string? Search,
    string? Country,
    bool? Active,
    int? Skip,
    int? Take) : IRequest<TollStationsPageVm>;

public sealed class GetTollStationsQueryHandler(ITollCatalogReader reader)
    : IRequestHandler<GetTollStationsQuery, TollStationsPageVm>
{
    private const int DefaultPageSize = 100;
    private const int MaxPageSize = 500;

    public async Task<TollStationsPageVm> Handle(GetTollStationsQuery request, CancellationToken cancellationToken)
    {
        var skip = Math.Max(request.Skip ?? 0, 0);
        var take = Math.Clamp(request.Take ?? DefaultPageSize, 1, MaxPageSize);
        return await reader.GetStationsPageAsync(request.Search, request.Country, request.Active, skip, take, cancellationToken);
    }
}

public sealed class GetTollStationsValidator : AbstractValidator<GetTollStationsQuery>
{
    public GetTollStationsValidator()
    {
        RuleFor(v => v.Skip).GreaterThanOrEqualTo(0).When(v => v.Skip.HasValue);
        RuleFor(v => v.Take).InclusiveBetween(1, 500).When(v => v.Take.HasValue);
        RuleFor(v => v.Country).Length(2).When(v => !string.IsNullOrWhiteSpace(v.Country));
    }
}

/// <summary>A station with its full, effective-dated tariff history.</summary>
[Authorize(Resource = Resources.TollCatalog, Action = Actions.Read)]
public readonly record struct GetTollStationDetailQuery(Guid TollStationId) : IRequest<TollStationDetailVm>;

public sealed class GetTollStationDetailQueryHandler(ITollCatalogReader reader)
    : IRequestHandler<GetTollStationDetailQuery, TollStationDetailVm>
{
    public async Task<TollStationDetailVm> Handle(GetTollStationDetailQuery request, CancellationToken cancellationToken)
    {
        Guard.Against.Default(request.TollStationId);
        return await reader.GetStationDetailAsync(request.TollStationId, cancellationToken);
    }
}

public sealed class GetTollStationDetailValidator : AbstractValidator<GetTollStationDetailQuery>
{
    public GetTollStationDetailValidator()
        => RuleFor(v => v.TollStationId).NotEmpty();
}

/// <summary>The deployment's vehicle classes. Empty until an operator defines them (spec 11 §7.7).</summary>
[Authorize(Resource = Resources.TollCatalog, Action = Actions.Read)]
public readonly record struct GetTollVehicleClassesQuery : IRequest<IReadOnlyCollection<TollVehicleClassVm>>;

public sealed class GetTollVehicleClassesQueryHandler(ITollCatalogReader reader)
    : IRequestHandler<GetTollVehicleClassesQuery, IReadOnlyCollection<TollVehicleClassVm>>
{
    public async Task<IReadOnlyCollection<TollVehicleClassVm>> Handle(GetTollVehicleClassesQuery request, CancellationToken cancellationToken)
        => await reader.GetVehicleClassesAsync(cancellationToken);
}
