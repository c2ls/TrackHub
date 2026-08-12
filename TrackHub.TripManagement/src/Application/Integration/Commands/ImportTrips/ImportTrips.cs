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

using Microsoft.Extensions.Logging;
using TrackHub.TripManagement.Application.Common;

namespace TrackHub.TripManagement.Application.Integration.Commands.ImportTrips;

/// <summary>
/// Partner/TMS trip import.
/// <para>
/// <b>Per-item results, never a batch failure.</b> A partner that sends 200 trips and gets one
/// rejection must not have the other 199 silently dropped — every row reports its own outcome and
/// the caller can retry precisely what failed (spec 11 §7.9).
/// </para>
/// <para>
/// Idempotent on <c>ExternalReference</c>, which is unique per account: a re-sent batch updates
/// rather than duplicating, so a partner retrying after a timeout cannot double-book a fleet.
/// </para>
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Write, PrincipalTypes = "ServiceClient")]
[RequireFeature(FeatureKeys.TripManagement)]
public readonly record struct ImportTripsCommand(Guid AccountId, IReadOnlyCollection<TripImportDto> Trips)
    : IRequest<IReadOnlyCollection<TripImportResultVm>>;

public sealed class ImportTripsCommandHandler(
    ITripWriter writer,
    ITripReader reader,
    ITripStopWriter stopWriter,
    IManagerValidationClient managerValidationClient,
    ILogger<ImportTripsCommandHandler> logger) : IRequestHandler<ImportTripsCommand, IReadOnlyCollection<TripImportResultVm>>
{
    public async Task<IReadOnlyCollection<TripImportResultVm>> Handle(ImportTripsCommand request, CancellationToken cancellationToken)
    {
        var results = new List<TripImportResultVm>(request.Trips.Count);

        foreach (var item in request.Trips)
        {
            try
            {
                results.Add(await ImportOneAsync(request.AccountId, item, cancellationToken));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Trip import failed for external reference {ExternalReference}", item.ExternalReference);
                results.Add(new TripImportResultVm(item.ExternalReference, false, null, "TRIP_IMPORT_FAILED", ex.Message));
            }
        }

        return results;
    }

    private async Task<TripImportResultVm> ImportOneAsync(Guid accountId, TripImportDto item, CancellationToken cancellationToken)
    {
        // The import path skipped transporter validation entirely, so a partner could book a trip
        // against a transporter in a DIFFERENT account — whose name the report readers then resolve
        // by unscoped id. There is no user and therefore no group scope here, but the account
        // boundary still applies (spec 11 §5, acceptance 2). Per-item, so one bad row does not fail
        // the batch: the exception is caught by Handle and reported against this row alone.
        await TripVisibility.EnsureTransporterInAccountAsync(reader, accountId, item.TransporterId, cancellationToken);

        // The driver needs the SAME check the portal create/update paths apply. Without it a partner
        // scoped to account A could import a trip carrying account B's driverId: nothing here has an
        // FK to app.drivers, and the report readers resolve driver names by UNSCOPED id, so B's
        // driver name would render inside A's trip and stop reports (acceptance 2). Manager's
        // assignment rule is the account boundary here — the transporter is already confirmed
        // in-account above, and a driver only qualifies through an assignment to it.
        if (item.DriverId is { } driverId)
        {
            var assignable = await managerValidationClient.ValidateDriverAssignmentAsync(
                driverId, "Transporter", item.TransporterId, cancellationToken);
            if (!assignable)
                throw new ForbiddenAccessException(Resources.Trips, Actions.Write, TripErrorCodes.DriverNotAssignable);
        }

        foreach (var stop in item.Stops)
            await TripVisibility.EnsureGeofenceInAccountAsync(reader, stop.GeofenceId, accountId, cancellationToken);

        var dto = new TripDto(
            item.Code,
            item.TransporterId,
            item.DriverId,
            null,
            item.ExternalReference,
            item.CustomerName,
            item.OriginName,
            item.OriginLatitude,
            item.OriginLongitude,
            item.PlannedStartAt,
            item.PlannedEndAt,
            item.Notes,
            null);

        var existing = await TripLookup.FindByExternalReferenceAsync(reader, accountId, item.ExternalReference, cancellationToken);
        if (existing is { } current)
        {
            if (TripStatuses.IsTerminal(current.Status))
                return new TripImportResultVm(item.ExternalReference, false, current.TripId, TripErrorCodes.TripAlreadyTerminal, "Trip is closed.");

            await writer.UpdateTripAsync(current.TripId, dto, accountId, cancellationToken);
            return new TripImportResultVm(item.ExternalReference, true, current.TripId, null, null);
        }

        var created = await writer.CreateTripAsync(dto, accountId, cancellationToken);
        foreach (var stop in item.Stops)
            await stopWriter.AddStopAsync(created.TripId, accountId, stop, cancellationToken);

        return new TripImportResultVm(item.ExternalReference, true, created.TripId, null, null);
    }
}

public sealed class ImportTripsValidator : AbstractValidator<ImportTripsCommand>
{
    public ImportTripsValidator()
    {
        RuleFor(v => v.AccountId).NotEmpty();
        RuleFor(v => v.Trips).NotEmpty();
        RuleForEach(v => v.Trips).ChildRules(trip =>
        {
            trip.RuleFor(t => t.ExternalReference).NotEmpty().MaximumLength(80);
            trip.RuleFor(t => t.Code).NotEmpty().MaximumLength(40);
            trip.RuleFor(t => t.TransporterId).NotEmpty();
            trip.RuleFor(t => t.OriginName).NotEmpty().MaximumLength(200);
            trip.RuleFor(t => t.OriginLatitude).InclusiveBetween(-90d, 90d);
            trip.RuleFor(t => t.OriginLongitude).InclusiveBetween(-180d, 180d);
        });
    }
}
