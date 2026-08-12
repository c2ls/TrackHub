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

namespace TrackHub.TripManagement.Application.TollCatalog.Commands.VehicleClasses;

// NOT feature-flagged, and deliberately so. The toll catalog is PLATFORM configuration describing
// public road infrastructure — the same classification as geocoding providers, roles, resources and
// actions — not a tenant capability. Gating it on `trip-management` would make platform reference
// data disappear for an account that had the module switched off, which is a different (and wrong)
// statement about who owns the data (spec 11 §3, §5). Write access is restricted to
// Administrator through the Security role matrix on Resources.TollCatalog.

/// <summary>Defines an axle/weight category tariffs are priced by. The platform ships no rows.</summary>
[Authorize(Resource = Resources.TollCatalog, Action = Actions.Write)]
[PlatformScoped("SVD-12 toll catalog: stations, tariffs and vehicle classes are platform-owned reference data administered by the platform operator; no tenant owns a row.")]
public readonly record struct CreateTollVehicleClassCommand(TollVehicleClassDto VehicleClass) : IRequest<TollVehicleClassVm>;

public sealed class CreateTollVehicleClassCommandHandler(ITollCatalogWriter writer)
    : IRequestHandler<CreateTollVehicleClassCommand, TollVehicleClassVm>
{
    public async Task<TollVehicleClassVm> Handle(CreateTollVehicleClassCommand request, CancellationToken cancellationToken)
        => await writer.CreateVehicleClassAsync(request.VehicleClass, cancellationToken);
}

public sealed class CreateTollVehicleClassValidator : AbstractValidator<CreateTollVehicleClassCommand>
{
    public CreateTollVehicleClassValidator()
        => RuleFor(v => v.VehicleClass).SetValidator(new TollVehicleClassDtoValidator());
}

[Authorize(Resource = Resources.TollCatalog, Action = Actions.Edit)]
[PlatformScoped("SVD-12 toll catalog: stations, tariffs and vehicle classes are platform-owned reference data administered by the platform operator; no tenant owns a row.")]
public readonly record struct UpdateTollVehicleClassCommand(Guid TollVehicleClassId, TollVehicleClassDto VehicleClass) : IRequest;

public sealed class UpdateTollVehicleClassCommandHandler(ITollCatalogWriter writer)
    : IRequestHandler<UpdateTollVehicleClassCommand>
{
    public async Task Handle(UpdateTollVehicleClassCommand request, CancellationToken cancellationToken)
        => await writer.UpdateVehicleClassAsync(request.TollVehicleClassId, request.VehicleClass, cancellationToken);
}

public sealed class UpdateTollVehicleClassValidator : AbstractValidator<UpdateTollVehicleClassCommand>
{
    public UpdateTollVehicleClassValidator()
    {
        RuleFor(v => v.TollVehicleClassId).NotEmpty();
        RuleFor(v => v.VehicleClass).SetValidator(new TollVehicleClassDtoValidator());
    }
}

/// <summary>Deactivates rather than deletes: historical tariffs still reference the class.</summary>
[Authorize(Resource = Resources.TollCatalog, Action = Actions.Delete)]
[PlatformScoped("SVD-12 toll catalog: stations, tariffs and vehicle classes are platform-owned reference data administered by the platform operator; no tenant owns a row.")]
public readonly record struct DeactivateTollVehicleClassCommand(Guid TollVehicleClassId) : IRequest<Guid>;

public sealed class DeactivateTollVehicleClassCommandHandler(ITollCatalogWriter writer)
    : IRequestHandler<DeactivateTollVehicleClassCommand, Guid>
{
    public async Task<Guid> Handle(DeactivateTollVehicleClassCommand request, CancellationToken cancellationToken)
    {
        await writer.DeactivateVehicleClassAsync(request.TollVehicleClassId, cancellationToken);
        return request.TollVehicleClassId;
    }
}

public sealed class DeactivateTollVehicleClassValidator : AbstractValidator<DeactivateTollVehicleClassCommand>
{
    public DeactivateTollVehicleClassValidator()
        => RuleFor(v => v.TollVehicleClassId).NotEmpty();
}

public sealed class TollVehicleClassDtoValidator : AbstractValidator<TollVehicleClassDto>
{
    public TollVehicleClassDtoValidator()
    {
        RuleFor(v => v.Code).NotEmpty().MaximumLength(20);
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Description).MaximumLength(500);
        RuleFor(v => v.SortOrder).GreaterThanOrEqualTo(0);
    }
}
