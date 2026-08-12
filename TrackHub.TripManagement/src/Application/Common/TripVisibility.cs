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
using Common.Application.Interfaces;

namespace TrackHub.TripManagement.Application.Common;

/// <summary>
/// The single visibility resolver for this module. Every read path (list, detail, timeline,
/// report, export) and every write-side transporter check funnels through here so acceptance 3
/// ("the same filter applies to list, detail, report and export paths identically") and
/// acceptance 4 ("<c>trip.vw_visible_transporter</c> is the single visibility source — no handler
/// re-implements group logic") hold by construction rather than by convention. A helper copied
/// per handler would drift; this one cannot.
/// </summary>
public static class TripVisibility
{
    /// <summary>
    /// The caller's user id, or <c>null</c> when the principal sees the whole account.
    /// Administrator/Manager roles and service clients are account-wide; the dispatcher/operations
    /// <c>User</c> role is scoped through <c>trip.vw_visible_transporter</c>.
    /// </summary>
    public static Guid? ResolveScopeUserId(IUser user, Guid userId)
        => SeesWholeAccount(user) ? null : userId;

    /// <summary>True when the principal is not group-scoped at all.</summary>
    public static bool SeesWholeAccount(IUser user)
        => user.PrincipalType == PrincipalType.ServiceClient
        || string.Equals(user.Role, Roles.Administrator, StringComparison.OrdinalIgnoreCase)
        || string.Equals(user.Role, Roles.Manager, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Write-side counterpart: a dispatcher may not attach a trip to a transporter outside their
    /// groups (spec 11 §4).
    /// <para>
    /// A principal that sees the whole account skips the GROUP predicate but NOT the ACCOUNT one.
    /// This method used to return early for Administrator/Manager/ServiceClient with no check at
    /// all, so an Account A administrator could create a trip pointing at Account B's transporter.
    /// That is not a cosmetic dangling reference: the report readers resolve
    /// <c>TransporterName</c>/<c>DriverName</c> by unscoped id, so Account B's transporter and
    /// driver names surfaced inside Account A's report output. Cross-account references are invalid
    /// at write time (spec 11 §5, acceptance 2).
    /// </para>
    /// <para>
    /// The two failures are deliberately different: a transporter that is not in the account at all
    /// is <c>NotFoundException</c> (it must not be distinguishable from a made-up id), while one
    /// that IS in the account but outside the caller's groups is <c>ForbiddenAccessException</c> —
    /// the caller already knows their own account's transporters exist.
    /// </para>
    /// </summary>
    public static async Task EnsureTransporterVisibleAsync(
        ITripReader reader,
        IUser user,
        Guid accountId,
        Guid userId,
        Guid transporterId,
        CancellationToken cancellationToken)
    {
        await EnsureTransporterInAccountAsync(reader, accountId, transporterId, cancellationToken);

        if (SeesWholeAccount(user))
            return;

        var visible = await reader.IsTransporterVisibleAsync(accountId, userId, transporterId, cancellationToken);
        if (!visible)
            throw new ForbiddenAccessException(Resources.Trips, Actions.Write, "Transporter is outside the caller's groups or account.");
    }

    /// <summary>
    /// The account half of the transporter check on its own, for the service-client import path,
    /// which has no user and therefore no group scope but is still bound by the account boundary.
    /// </summary>
    public static async Task EnsureTransporterInAccountAsync(
        ITripReader reader,
        Guid accountId,
        Guid transporterId,
        CancellationToken cancellationToken)
    {
        var exists = await reader.TransporterExistsInAccountAsync(transporterId, accountId, cancellationToken);
        if (!exists)
            throw new NotFoundException($"{transporterId}", "Transporter");
    }

    /// <summary>
    /// Validates a stop's optional linked geofence at WRITE time.
    /// <para>
    /// The arrival-snapshot code (taken at <c>StartTrip</c>) looks the geofence up with an account
    /// predicate and, on a miss, falls back to buffering the stop point by
    /// <c>ArrivalRadiusMeters</c> — with no error. That fallback is correct for a geofence deleted
    /// AFTER the stop was created, but it also silently swallowed a wrong or cross-account id
    /// entered at creation: a dispatcher who deliberately picked a polygon got a 150 m circle and
    /// no indication that detection would behave differently. Validating here keeps the fallback
    /// for the genuine case while making a bad id a 404 at the moment it is submitted.
    /// </para>
    /// </summary>
    public static async Task EnsureGeofenceInAccountAsync(
        ITripReader reader,
        Guid? geofenceId,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        if (geofenceId is not { } id)
            return;

        var exists = await reader.GeofenceExistsInAccountAsync(id, accountId, cancellationToken);
        if (!exists)
            throw new NotFoundException($"{id}", "Geofence");
    }

    /// <summary>
    /// Validates a trip's optional service-order reference against the account, through the
    /// <see cref="IServiceOrderValidator"/> port. Null is always valid ("no service order").
    /// </summary>
    public static async Task EnsureServiceOrderInAccountAsync(
        IServiceOrderValidator validator,
        Guid? serviceOrderId,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        if (serviceOrderId is not { } id)
            return;

        var exists = await validator.ExistsInAccountAsync(id, accountId, cancellationToken);
        if (!exists)
            throw new NotFoundException($"{id}", "ServiceOrder");
    }

    /// <summary>
    /// Resolves the trip owning a stop, applying the caller's visibility scope. The stop-addressed
    /// commands carry no trip id, so without this they wrote against any stop in the account.
    /// Invisible or unknown → <c>NotFoundException</c> (non-disclosure).
    /// </summary>
    public static async Task<Guid> ResolveVisibleTripByStopAsync(
        ITripReader reader,
        Guid tripStopId,
        Guid accountId,
        Guid? scopeUserId,
        CancellationToken cancellationToken)
        => await reader.FindVisibleTripIdByStopAsync(tripStopId, accountId, scopeUserId, cancellationToken)
            ?? throw new NotFoundException($"{tripStopId}", "TripStop");

    /// <summary>Delivery-addressed counterpart of <see cref="ResolveVisibleTripByStopAsync"/>.</summary>
    public static async Task<Guid> ResolveVisibleTripByDeliveryAsync(
        ITripReader reader,
        Guid deliveryId,
        Guid accountId,
        Guid? scopeUserId,
        CancellationToken cancellationToken)
        => await reader.FindVisibleTripIdByDeliveryAsync(deliveryId, accountId, scopeUserId, cancellationToken)
            ?? throw new NotFoundException($"{deliveryId}", "Delivery");

    /// <summary>
    /// Share-addressed counterpart of <see cref="ResolveVisibleTripByStopAsync"/>, for
    /// <c>RevokeTripShareCommand</c>.
    /// <para>
    /// Revoke resolved the share by id under ACCOUNT scope only and never looked the trip up at
    /// all, so a dispatcher holding a borrowed or guessed <c>TripShareId</c> could kill a public
    /// tracking link belonging to another group's trip — a denial of service against a
    /// customer-facing link they were never allowed to see. The returned trip id is also the
    /// authoritative one: the command carries a caller-supplied <c>TripId</c> that must not be
    /// trusted to address the audit event, or a mismatched pair would write the revocation onto a
    /// different trip's timeline.
    /// </para>
    /// <para>
    /// Composed from the existing mechanisms rather than adding a fourth bespoke lookup: the share
    /// reader answers "which trip", and <see cref="ITripReader.GetTripAsync"/> applies the one
    /// <c>Visible()</c> predicate and raises <c>NotFoundException</c> for a trip outside the
    /// caller's groups. Unknown share and invisible trip are therefore indistinguishable, which is
    /// the point (non-disclosure, spec 11 §7.10).
    /// </para>
    /// </summary>
    public static async Task<Guid> ResolveVisibleTripByShareAsync(
        ITripShareReader shareReader,
        ITripReader reader,
        Guid tripShareId,
        Guid accountId,
        Guid? scopeUserId,
        CancellationToken cancellationToken)
    {
        var tripId = await shareReader.FindTripIdByShareAsync(tripShareId, accountId, cancellationToken)
            ?? throw new NotFoundException($"{tripShareId}", "TripShare");

        // Throws NotFoundException when the trip is outside the caller's group scope.
        await reader.GetTripAsync(tripId, accountId, scopeUserId, cancellationToken);

        return tripId;
    }

    /// <summary>
    /// Resolves the acting user id from the token subject. Never <c>Guid.Parse(user.Id)</c>:
    /// service-client subjects are not Guids (rules.md).
    /// </summary>
    public static Guid RequireUserId(IUser user)
        => Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    /// <summary>
    /// The export/report path, where the account travels on the request because Reporting calls
    /// under a service identity. A USER caller may only ask for their own account (acceptance 1)
    /// and is group-scoped exactly as on the list and detail paths (acceptance 3); a service client
    /// is already constrained by its seeded grants and sees the whole account.
    /// </summary>
    public static async Task<Guid?> ResolveReportScopeAsync(
        IUser user,
        IUserReader userReader,
        Guid requestedAccountId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(user.Id, out var userId))
            return null;

        var caller = await userReader.GetUserAsync(userId, cancellationToken);
        if (caller.AccountId != requestedAccountId)
            throw new ForbiddenAccessException(Resources.Trips, Actions.Export, "Account mismatch.");

        return ResolveScopeUserId(user, userId);
    }
}
