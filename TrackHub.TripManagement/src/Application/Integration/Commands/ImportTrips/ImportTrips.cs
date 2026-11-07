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
using TrackHub.TripManagement.Application.Trips.Services.Interfaces;

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
// Enforcement: each item's transporter, staff and geofences are checked against AccountId.
[AccountScopeEnforcedInHandler]
public readonly record struct ImportTripsCommand(Guid AccountId, IReadOnlyCollection<TripImportDto> Trips)
    : IRequest<IReadOnlyCollection<TripImportResultVm>>;

public sealed class ImportTripsCommandHandler(
    ITripWriter writer,
    ITripReader reader,
    ITripStopWriter stopWriter,
    IManagerValidationClient managerValidationClient,
    ITripStartBackfillService backfillService,
    ITransporterTollClassStore tollClassStore,
    ITripTransaction transaction,
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
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Trip import failed for external reference {ExternalReference}", item.ExternalReference);
                results.Add(PartnerErrorResult.Describe(ex, item.ExternalReference, null, "TRIP_IMPORT_FAILED"));
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

        foreach (var stop in item.Stops)
            await TripVisibility.EnsureGeofenceInAccountAsync(reader, stop.GeofenceId, accountId, cancellationToken);

        await TripVisibility.EnsureGeofenceInAccountAsync(reader, item.OriginGeofenceId, accountId, cancellationToken);

        var existing = await TripLookup.FindByExternalReferenceAsync(reader, accountId, item.ExternalReference, cancellationToken);
        if (existing is { } current && TripStatuses.IsTerminal(current.Status))
            return new TripImportResultVm(item.ExternalReference, false, current.TripId, TripErrorCodes.TripAlreadyTerminal, "Trip is closed.");

        // Only a trip still in Created may be re-routed: once it is running, its stops carry
        // measurements and a replacement would erase them.
        if (existing is { } running && item.Stops.Count > 0 && !string.Equals(running.Status, TripStatuses.Created, StringComparison.Ordinal))
            return new TripImportResultVm(item.ExternalReference, false, running.TripId, TripErrorCodes.TripNotActive, "Stops cannot be replaced once the trip has started.");

        // The staff needs the SAME check the portal create/update paths apply. Without it a partner
        // scoped to account A could import a trip carrying account B's staffId: nothing here has an
        // FK to app.staff_members, and the report readers resolve staff names by UNSCOPED id, so B's
        // staff name would render inside A's trip and stop reports (acceptance 2). Manager's
        // assignment rule is the account boundary here — the transporter is already confirmed
        // in-account above, and a staff only qualifies through an assignment to it. A staff carried
        // over from the stored trip is checked again when the partner moves the trip to another unit.
        var staffId = item.StaffId ?? existing?.StaffId;
        if (staffId is { } effectiveStaffId && (item.StaffId is not null || existing?.TransporterId != item.TransporterId))
        {
            var assignable = await managerValidationClient.ValidateStaffAssignmentAsync(
                effectiveStaffId, "Transporter", item.TransporterId, cancellationToken);
            if (!assignable)
                throw new ForbiddenAccessException(Resources.Trips, Actions.Write, TripErrorCodes.StaffNotAssignable);
        }

        // A re-send updates only what the partner contract carries: an optional field the partner
        // left out keeps the value a dispatcher set, and fields outside the contract (service order,
        // origin radius, toll class) are never touched.
        var dto = new TripDto(
            item.Code,
            item.TransporterId,
            staffId,
            existing?.ServiceOrderId,
            item.ExternalReference,
            item.CustomerName ?? existing?.CustomerName,
            item.OriginName,
            item.OriginLatitude,
            item.OriginLongitude,
            item.OriginGeofenceId ?? existing?.OriginGeofenceId,
            existing?.OriginRadiusMeters ?? TripGeometry.DefaultRadiusMeters,
            item.PlannedStartAt,
            item.PlannedEndAt ?? existing?.PlannedEndAt,
            item.Notes ?? existing?.Notes,
            existing?.TollVehicleClass);

        if (string.IsNullOrWhiteSpace(dto.TollVehicleClass) || existing?.TransporterId != item.TransporterId)
            dto = dto with { TollVehicleClass = await tollClassStore.ResolveClassAsync(accountId, item.TransporterId, cancellationToken) ?? dto.TollVehicleClass };

        var tripId = existing?.TripId ?? Guid.Empty;
        await transaction.ExecuteAsync(async token =>
        {
            if (existing is { } current)
            {
                await writer.UpdateTripAsync(current.TripId, dto, accountId, null, token);
                if (item.Stops.Count > 0)
                    await stopWriter.ReplaceStopsAsync(current.TripId, accountId, item.Stops, token);
                return;
            }

            tripId = (await writer.CreateTripAsync(dto, accountId, token)).TripId;
            foreach (var stop in item.Stops)
                await stopWriter.AddStopAsync(tripId, accountId, stop, token);
        }, cancellationToken);

        await DeclareInTransitIfRequestedAsync(tripId, accountId, item, cancellationToken);
        return new TripImportResultVm(item.ExternalReference, true, tripId, null, null);
    }


    /// <summary>
    /// A partner that reports <c>startedAt</c> is telling us the truck already left. Backfill runs
    /// LAST, once the row's stops exist, because it replays the route (spec 11a §5.4).
    /// <para>
    /// The caller here is a tenant-bound service client, not a user, so there is no group scope to
    /// resolve — it sees the whole account by design (SVD-13).
    /// </para>
    /// </summary>
    private async Task DeclareInTransitIfRequestedAsync(
        Guid tripId, Guid accountId, TripImportDto item, CancellationToken cancellationToken)
    {
        if (item.StartedAt is null)
            return;

        await backfillService.ApplyAsync(tripId, accountId, null, item.StartedAt, cancellationToken);
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
