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

namespace TrackHub.TripManagement.Application.TollCatalog.Commands.Tariffs;

// NOT feature-flagged — platform reference data (spec 11 §3, §5). Administrator-only.

/// <summary>
/// Inserts a tariff and CLOSES the currently open row for the same <c>(station, class)</c> pair.
/// Prices are append-only: overwriting one would silently rewrite every historical trip's estimate,
/// so a change is a new effective-dated row and the old figure stays reproducible (acceptance 21).
/// An overlapping window for the same pair is a conflict (409), not a merge.
/// </summary>
[Authorize(Resource = Resources.TollCatalog, Action = Actions.Write)]
[PlatformScoped("SVD-12 toll catalog: stations, tariffs and vehicle classes are platform-owned reference data administered by the platform operator; no tenant owns a row.")]
public readonly record struct CreateTollTariffCommand(TollTariffDto Tariff) : IRequest<TollTariffVm>;

public sealed class CreateTollTariffCommandHandler(ITollCatalogWriter writer)
    : IRequestHandler<CreateTollTariffCommand, TollTariffVm>
{
    public async Task<TollTariffVm> Handle(CreateTollTariffCommand request, CancellationToken cancellationToken)
        // No overlap pre-check here. Creating a tariff for a pair that already has an OPEN row is
        // how a price change is expressed (spec 11 §7.6): the writer closes the open row at
        // EffectiveFrom - 1 day and inserts the new one. A pre-check with excludeTariffId: null
        // counted that open row as an overlap and made every price change a 409 — i.e. prices
        // could never be changed through the API at all. The writer owns overlap semantics
        // because it is the only place that knows which row it is about to close.
        => await writer.CreateTariffAsync(request.Tariff, cancellationToken);
}

public sealed class CreateTollTariffValidator : AbstractValidator<CreateTollTariffCommand>
{
    public CreateTollTariffValidator()
        => RuleFor(v => v.Tariff).SetValidator(new TollTariffDtoValidator());
}

[Authorize(Resource = Resources.TollCatalog, Action = Actions.Edit)]
[PlatformScoped("SVD-12 toll catalog: stations, tariffs and vehicle classes are platform-owned reference data administered by the platform operator; no tenant owns a row.")]
public readonly record struct UpdateTollTariffCommand(Guid TollTariffId, TollTariffDto Tariff) : IRequest;

public sealed class UpdateTollTariffCommandHandler(ITollCatalogWriter writer, ITollCatalogReader reader)
    : IRequestHandler<UpdateTollTariffCommand>
{
    public async Task Handle(UpdateTollTariffCommand request, CancellationToken cancellationToken)
    {
        var overlapping = await reader.HasOverlappingTariffAsync(
            request.Tariff.TollStationId, request.Tariff.TollVehicleClassCode,
            request.Tariff.EffectiveFrom, request.Tariff.EffectiveTo, request.TollTariffId, cancellationToken);

        if (overlapping)
            throw ConflictException.WithCode(TripErrorCodes.OverlappingTariff);

        await writer.UpdateTariffAsync(request.TollTariffId, request.Tariff, cancellationToken);
    }
}

public sealed class UpdateTollTariffValidator : AbstractValidator<UpdateTollTariffCommand>
{
    public UpdateTollTariffValidator()
    {
        RuleFor(v => v.TollTariffId).NotEmpty();
        RuleFor(v => v.Tariff).SetValidator(new TollTariffDtoValidator());
    }
}

[Authorize(Resource = Resources.TollCatalog, Action = Actions.Delete)]
[PlatformScoped("SVD-12 toll catalog: stations, tariffs and vehicle classes are platform-owned reference data administered by the platform operator; no tenant owns a row.")]
public readonly record struct DeleteTollTariffCommand(Guid TollTariffId) : IRequest<Guid>;

public sealed class DeleteTollTariffCommandHandler(ITollCatalogWriter writer)
    : IRequestHandler<DeleteTollTariffCommand, Guid>
{
    public async Task<Guid> Handle(DeleteTollTariffCommand request, CancellationToken cancellationToken)
    {
        await writer.DeleteTariffAsync(request.TollTariffId, cancellationToken);
        return request.TollTariffId;
    }
}

public sealed class DeleteTollTariffValidator : AbstractValidator<DeleteTollTariffCommand>
{
    public DeleteTollTariffValidator()
        => RuleFor(v => v.TollTariffId).NotEmpty();
}

public sealed class TollTariffDtoValidator : AbstractValidator<TollTariffDto>
{
    public TollTariffDtoValidator()
    {
        RuleFor(v => v.TollStationId).NotEmpty();
        RuleFor(v => v.TollVehicleClassCode).NotEmpty().MaximumLength(20);
        RuleFor(v => v.Amount).GreaterThanOrEqualTo(0m);
        RuleFor(v => v.Currency).NotEmpty().Length(3);
        RuleFor(v => v.EffectiveTo)
            .GreaterThanOrEqualTo(v => v.EffectiveFrom)
            .When(v => v.EffectiveTo.HasValue);
    }
}
