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

namespace TrackHub.TripManagement.Application.TollCatalog.Commands.Stations;

// NOT feature-flagged — platform reference data, not a tenant feature (spec 11 §3, §5).
// Administrator-only through the Security role matrix on Resources.TollCatalog.
// Every write is audited: station changes move money on estimates.

/// <summary>Registers a toll station with its coordinates.</summary>
[Authorize(Resource = Resources.TollCatalog, Action = Actions.Write)]
public readonly record struct CreateTollStationCommand(TollStationDto Station) : IRequest<TollStationVm>;

public sealed class CreateTollStationCommandHandler(ITollCatalogWriter writer)
    : IRequestHandler<CreateTollStationCommand, TollStationVm>
{
    public async Task<TollStationVm> Handle(CreateTollStationCommand request, CancellationToken cancellationToken)
        => await writer.CreateStationAsync(request.Station, cancellationToken);
}

public sealed class CreateTollStationValidator : AbstractValidator<CreateTollStationCommand>
{
    public CreateTollStationValidator()
        => RuleFor(v => v.Station).SetValidator(new TollStationDtoValidator());
}

[Authorize(Resource = Resources.TollCatalog, Action = Actions.Edit)]
public readonly record struct UpdateTollStationCommand(Guid TollStationId, TollStationDto Station) : IRequest;

public sealed class UpdateTollStationCommandHandler(ITollCatalogWriter writer)
    : IRequestHandler<UpdateTollStationCommand>
{
    public async Task Handle(UpdateTollStationCommand request, CancellationToken cancellationToken)
        => await writer.UpdateStationAsync(request.TollStationId, request.Station, cancellationToken);
}

public sealed class UpdateTollStationValidator : AbstractValidator<UpdateTollStationCommand>
{
    public UpdateTollStationValidator()
    {
        RuleFor(v => v.TollStationId).NotEmpty();
        RuleFor(v => v.Station).SetValidator(new TollStationDtoValidator());
    }
}

/// <summary>Deactivates rather than deletes: historical estimates cite the station.</summary>
[Authorize(Resource = Resources.TollCatalog, Action = Actions.Delete)]
public readonly record struct DeactivateTollStationCommand(Guid TollStationId) : IRequest<Guid>;

public sealed class DeactivateTollStationCommandHandler(ITollCatalogWriter writer)
    : IRequestHandler<DeactivateTollStationCommand, Guid>
{
    public async Task<Guid> Handle(DeactivateTollStationCommand request, CancellationToken cancellationToken)
    {
        await writer.DeactivateStationAsync(request.TollStationId, cancellationToken);
        return request.TollStationId;
    }
}

public sealed class DeactivateTollStationValidator : AbstractValidator<DeactivateTollStationCommand>
{
    public DeactivateTollStationValidator()
        => RuleFor(v => v.TollStationId).NotEmpty();
}

public sealed class TollStationDtoValidator : AbstractValidator<TollStationDto>
{
    public TollStationDtoValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Code).MaximumLength(40);
        RuleFor(v => v.Latitude).InclusiveBetween(-90d, 90d);
        RuleFor(v => v.Longitude).InclusiveBetween(-180d, 180d);
        RuleFor(v => v.Country).Length(2).When(v => !string.IsNullOrWhiteSpace(v.Country));
        RuleFor(v => v.Region).MaximumLength(100);
        RuleFor(v => v.RoadName).MaximumLength(200);
        RuleFor(v => v.Direction).MaximumLength(50);
        RuleFor(v => v.Operator).MaximumLength(200);
        RuleFor(v => v.Notes).MaximumLength(1000);
    }
}
