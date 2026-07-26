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

using Common.Domain.Constants;

namespace DBInitializer;

/// <summary>
/// The core platform's RBAC seed data. Feature modules add their own
/// <see cref="IRbacSeedContribution"/> files rather than growing these lists.
/// </summary>
internal sealed class CoreRbacSeedContribution : IRbacSeedContribution
{
    public IReadOnlyList<string> Resources { get; } =
    [
        Common.Domain.Constants.Resources.Accounts,
        Common.Domain.Constants.Resources.AccountsMaster,
        Common.Domain.Constants.Resources.Administrative,
        Common.Domain.Constants.Resources.AccountFeatures,
        Common.Domain.Constants.Resources.AccountFeaturesMaster,
        Common.Domain.Constants.Resources.Alerts,
        Common.Domain.Constants.Resources.Audit,
        Common.Domain.Constants.Resources.BackgroundJobs,
        Common.Domain.Constants.Resources.Credentials,
        Common.Domain.Constants.Resources.Devices,
        Common.Domain.Constants.Resources.DevicesMaster,
        Common.Domain.Constants.Resources.Documents,
        Common.Domain.Constants.Resources.Drivers,
        Common.Domain.Constants.Resources.GeocodingProviders,
        Common.Domain.Constants.Resources.Geofences,
        Common.Domain.Constants.Resources.Geofencing,
        Common.Domain.Constants.Resources.GpsIntegrationDashboard,
        Common.Domain.Constants.Resources.Groups,
        Common.Domain.Constants.Resources.Notifications,
        Common.Domain.Constants.Resources.Operators,
        Common.Domain.Constants.Resources.OperatorHealth,
        Common.Domain.Constants.Resources.OperatorSyncRuns,
        Common.Domain.Constants.Resources.OperatorsMaster,
        Common.Domain.Constants.Resources.Permissions,
        Common.Domain.Constants.Resources.PointsOfInterest,
        Common.Domain.Constants.Resources.Positions,
        Common.Domain.Constants.Resources.PositionHistory,
        Common.Domain.Constants.Resources.Profile,
        Common.Domain.Constants.Resources.PublicLinks,
        Common.Domain.Constants.Resources.Reports,
        Common.Domain.Constants.Resources.ServiceClients,
        Common.Domain.Constants.Resources.SupportGrants,
        Common.Domain.Constants.Resources.SynchronizedDevices,
        Common.Domain.Constants.Resources.TollCatalog,
        Common.Domain.Constants.Resources.Transporters,
        Common.Domain.Constants.Resources.TransporterType,
        Common.Domain.Constants.Resources.Trips,
        Common.Domain.Constants.Resources.TripTracking,
        Common.Domain.Constants.Resources.Users,
    ];

    // Resources.Trips needs Custom because planTripRoute, shareTrip and
    // revokeTripShare are user-facing Custom operations. Without a ResourceAction row here
    // there is no ResourceActionRole to grant, so IdentityService resolves no roles and
    // AuthorizationBehavior returns FORBIDDEN for EVERY user including Administrator —
    // route planning and the whole public-sharing surface would be dead on a fresh deploy.
    // (TripTracking/Custom is deliberately NOT here: only service clients call it, and they
    // authorize through the separate service_client_permissions table.)
    public IReadOnlyList<string> CustomActionResources { get; } =
    [
        Common.Domain.Constants.Resources.Users,
        Common.Domain.Constants.Resources.Positions,
        Common.Domain.Constants.Resources.Credentials,
        Common.Domain.Constants.Resources.Notifications,
        Common.Domain.Constants.Resources.Trips,
        Common.Domain.Constants.Resources.Operators,
    ];

    // Data-driven default role matrix for the non-admin roles. Each entry is the full set of
    // resource -> actions a role is granted; the initializer's role-grant seeding is idempotent
    // and silently skips any resource/action pair that isn't in the ResourceAction catalog, so
    // resources not listed for a role receive no grant. Administrator is intentionally absent
    // here — it receives grant-all over the whole catalog.
    //
    // Rationale anchors: Geofences full CRUD for User (portal Geofence Manager route is
    // USER-gated); Positions is Read-only for both roles (Positions/Custom is a service-identity
    // write from the Router feed, with no portal or mobile caller); Operators/Custom is ping and
    // manual sync, held by Manager and User; Users/Custom is SELF-SERVICE password change only
    // (UpdatePasswordCommand re-checks that the subject is the caller);
    // Credentials (viewing decrypted credential material) and Drivers are Manager-only;
    // Notifications/Custom (SendTest) is Manager-only;
    // Reports/Edit, SupportGrants, ServiceClients, Administrative, *Master and GeocodingProviders
    // stay Administrator-only.
    //
    // Trip Management: Manager gets Trips R/W/E/D/Export/Custom + TollCatalog/Read;
    // User gets Trips R/W/E/Export/Custom + TollCatalog/Read. TollCatalog Write/Edit/Delete
    // is deliberately NOT granted to Manager or User — the toll catalog is platform reference
    // data, so a non-administrator can read stations/tariffs/vehicle classes but can never
    // create, edit or delete one.
    //
    // TripTracking carries NO role grant at all. called for Manager → TripTracking/Read,
    // but the resource has exactly one operation — processTripPositions, which is
    // TripTracking/Custom — so a Read grant authorized nothing while standing as a live
    // permission row that would silently pre-authorize any TripTracking read added later.
    // TripTracking/Custom is a service-identity grant only and is seeded via
    // ServiceClientGrants below.
    //
    // NOTE: role-grant seeding only ADDS missing rows, it never removes. A deployment
    // already seeded with the earlier matrix keeps its inert Manager → TripTracking/Read row;
    // delete it manually if you want the permission table to match this list exactly.
    public IReadOnlyDictionary<string, (string Resource, string[] Actions)[]> RoleGrants { get; } =
        new Dictionary<string, (string Resource, string[] Actions)[]>
        {
            [Roles.Manager] =
            [
                (Common.Domain.Constants.Resources.Accounts, [Actions.Read, Actions.Edit]),
                (Common.Domain.Constants.Resources.AccountFeatures, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete]),
                (Common.Domain.Constants.Resources.Alerts, [Actions.Read, Actions.Edit]),
                (Common.Domain.Constants.Resources.Audit, [Actions.Read]),
                (Common.Domain.Constants.Resources.BackgroundJobs, [Actions.Read]),
                (Common.Domain.Constants.Resources.Credentials, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete]),
                (Common.Domain.Constants.Resources.Devices, [Actions.Read, Actions.Delete]),
                (Common.Domain.Constants.Resources.Documents, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete]),
                (Common.Domain.Constants.Resources.Drivers, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete]),
                (Common.Domain.Constants.Resources.Geofences, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete]),
                (Common.Domain.Constants.Resources.Geofencing, [Actions.Read]),
                (Common.Domain.Constants.Resources.GpsIntegrationDashboard, [Actions.Read]),
                (Common.Domain.Constants.Resources.Groups, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete]),
                (Common.Domain.Constants.Resources.Notifications, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete, Actions.Custom]),
                // Custom = operate the GPS integration (pingOperator, triggerOperatorSync). The Router
                // reaches the provider with its own service identity, so this grants operation of the
                // integration without granting sight of credential material.
                (Common.Domain.Constants.Resources.Operators, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete, Actions.Custom]),
                (Common.Domain.Constants.Resources.OperatorHealth, [Actions.Read]),
                (Common.Domain.Constants.Resources.OperatorSyncRuns, [Actions.Read]),
                (Common.Domain.Constants.Resources.Permissions, [Actions.Read]),
                (Common.Domain.Constants.Resources.PointsOfInterest, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete]),
                // Read only. Positions/Custom gates two commands — bulkTransporterPosition and
                // persistResolvedAddress — and both are service-identity writes from the Router feed.
                // Every user-facing positions operation uses Actions.Read.
                (Common.Domain.Constants.Resources.Positions, [Actions.Read]),
                (Common.Domain.Constants.Resources.PositionHistory, [Actions.Read]),
                (Common.Domain.Constants.Resources.Profile, [Actions.Read, Actions.Edit]),
                (Common.Domain.Constants.Resources.PublicLinks, [Actions.Read, Actions.Write, Actions.Delete]),
                (Common.Domain.Constants.Resources.Reports, [Actions.Read]),
                (Common.Domain.Constants.Resources.SynchronizedDevices, [Actions.Read, Actions.Write, Actions.Edit, Actions.Execute]),
                (Common.Domain.Constants.Resources.TollCatalog, [Actions.Read]),
                (Common.Domain.Constants.Resources.Transporters, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete]),
                (Common.Domain.Constants.Resources.TransporterType, [Actions.Read]),
                // Custom is planTripRoute / shareTrip / revokeTripShare. Adding Trips to the
                // Actions.Custom resource list only makes the pair GRANTABLE — authorization reads
                // ResourceActionRole, and role inheritance does not fill the gap
                // (ResourceActionRoleReader matches role names exactly, no ParentRoleId walk).
                // Without it an account administrator could create a trip but not plan its route
                // or share it, and only the platform Administrator's grant-all covered it.
                (Common.Domain.Constants.Resources.Trips, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete, Actions.Export, Actions.Custom]),
                (Common.Domain.Constants.Resources.Users, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete, Actions.Custom]),
            ],
            [Roles.User] =
            [
                (Common.Domain.Constants.Resources.Accounts, [Actions.Read]),
                (Common.Domain.Constants.Resources.Alerts, [Actions.Read]),
                (Common.Domain.Constants.Resources.Devices, [Actions.Read]),
                (Common.Domain.Constants.Resources.Documents, [Actions.Read]),
                (Common.Domain.Constants.Resources.Geofences, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete]),
                (Common.Domain.Constants.Resources.Geofencing, [Actions.Read]),
                (Common.Domain.Constants.Resources.Groups, [Actions.Read]),
                (Common.Domain.Constants.Resources.Notifications, [Actions.Read, Actions.Write, Actions.Edit, Actions.Delete]),
                // Custom = ping / manual sync. See the Manager entry.
                (Common.Domain.Constants.Resources.Operators, [Actions.Read, Actions.Custom]),
                (Common.Domain.Constants.Resources.PointsOfInterest, [Actions.Read]),
                // Read only — see the note on the Manager entry above.
                (Common.Domain.Constants.Resources.Positions, [Actions.Read]),
                (Common.Domain.Constants.Resources.PositionHistory, [Actions.Read]),
                (Common.Domain.Constants.Resources.Profile, [Actions.Read, Actions.Edit]),
                (Common.Domain.Constants.Resources.Reports, [Actions.Read]),
                (Common.Domain.Constants.Resources.TollCatalog, [Actions.Read]),
                (Common.Domain.Constants.Resources.Transporters, [Actions.Read]),
                (Common.Domain.Constants.Resources.TransporterType, [Actions.Read]),
                // Custom = plan route / share / revoke. The dispatcher is the primary actor
                // for exactly these three operations, so withholding it left the primary user of
                // the module unable to plan a route on a trip they had just created.
                // Export = the six trip-* report feeds, including the POD register. Without it the
                // dispatcher-facing catalog reports were visible in the picker but failed closed for
                // the role that owns them.
                (Common.Domain.Constants.Resources.Trips, [Actions.Read, Actions.Write, Actions.Edit, Actions.Export, Actions.Custom]),
                // Custom on Users gates exactly one command: UpdatePasswordCommand, whose handler
                // requires the subject to be the caller (or a verified manager of the subject). This
                // is the self-service password change; it grants no user administration.
                (Common.Domain.Constants.Resources.Users, [Actions.Custom]),
            ],
        };

    public IReadOnlyList<string> ServiceClientNames { get; } =
        ["router_client", "syncworker_client", "security_client", "geofence_client", "trip_client"];

    public IReadOnlyList<(string[] Clients, (string Resource, string Action)[] Grants)> ServiceClientGrants { get; } =
    [
        // Service-client allowlist for the TrackHub.Telemetry surface. The token audience is the
        // shared trackhub_api, so that is the audience these grants match at enforcement time.
        // Removing a row blocks the corresponding operation with FORBIDDEN.
        (["router_client", "syncworker_client"],
        [
            (Common.Domain.Constants.Resources.Positions, Actions.Custom),
            (Common.Domain.Constants.Resources.PositionHistory, Actions.Write),
            (Common.Domain.Constants.Resources.PositionHistory, Actions.Read),
            (Common.Domain.Constants.Resources.OperatorHealth, Actions.Write),
            (Common.Domain.Constants.Resources.OperatorSyncRuns, Actions.Write),
        ]),
        // Manager master-data surface the Router/SyncWorker call with their SERVICE identity
        // (client credentials): account/operator/device-sync reads, device synchronization writes,
        // credential token refresh, alert recording, and the ServiceClient-only geocoding provider
        // read. Sourced from the [Authorize] attributes on the corresponding Manager handlers.
        (["router_client", "syncworker_client"],
        [
            (Common.Domain.Constants.Resources.Accounts, Actions.Read),
            (Common.Domain.Constants.Resources.AccountsMaster, Actions.Read),
            (Common.Domain.Constants.Resources.AccountFeatures, Actions.Read),
            (Common.Domain.Constants.Resources.AccountFeaturesMaster, Actions.Read),
            (Common.Domain.Constants.Resources.Operators, Actions.Read),
            (Common.Domain.Constants.Resources.OperatorsMaster, Actions.Read),
            (Common.Domain.Constants.Resources.SynchronizedDevices, Actions.Read),
            (Common.Domain.Constants.Resources.SynchronizedDevices, Actions.Write),
            (Common.Domain.Constants.Resources.Devices, Actions.Delete),
            (Common.Domain.Constants.Resources.TransporterType, Actions.Read),
            (Common.Domain.Constants.Resources.Credentials, Actions.Write),
            (Common.Domain.Constants.Resources.Alerts, Actions.Write),
            (Common.Domain.Constants.Resources.GeocodingProviders, Actions.Read),
        ]),
        // The security_client posts security audit events to Manager's central AuditEvent store.
        // It needs exactly one grant — Audit/Write — and nothing else.
        (["security_client"],
        [
            (Common.Domain.Constants.Resources.Audit, Actions.Write),
        ]),
        // The geofence_client records geofence alert events (recordAlertEvent) and its dwell
        // evaluator's job runs (createBackgroundJobRun) in Manager — exactly two grants.
        (["geofence_client"],
        [
            (Common.Domain.Constants.Resources.Alerts, Actions.Write),
            (Common.Domain.Constants.Resources.BackgroundJobs, Actions.Write),
        ]),
        // The Router/SyncWorker side additionally needs Geofencing/Custom to feed the real-time
        // detection pipeline (processPositions) — without it every batch is denied and no geofence
        // events or alerts can ever be produced.
        (["router_client", "syncworker_client"],
        [
            (Common.Domain.Constants.Resources.Geofencing, Actions.Custom),
        ]),
        // The trip_client calls Manager (alert events, job runs, driver/transporter/group master
        // data, trip document metadata, public tracking links) and Telemetry (route replay position
        // history) with its SERVICE identity. Sourced from the [Authorize] attributes on the
        // corresponding producer handlers — this is the complete allowlist; removing a row blocks
        // that operation with FORBIDDEN.
        (["trip_client"],
        [
            (Common.Domain.Constants.Resources.Alerts, Actions.Write),
            (Common.Domain.Constants.Resources.BackgroundJobs, Actions.Write),
            (Common.Domain.Constants.Resources.Drivers, Actions.Read),
            (Common.Domain.Constants.Resources.Transporters, Actions.Read),
            (Common.Domain.Constants.Resources.Groups, Actions.Read),
            (Common.Domain.Constants.Resources.Documents, Actions.Read),
            (Common.Domain.Constants.Resources.PublicLinks, Actions.Write),
            (Common.Domain.Constants.Resources.PublicLinks, Actions.Delete),
            (Common.Domain.Constants.Resources.PublicLinks, Actions.Read),
            (Common.Domain.Constants.Resources.PositionHistory, Actions.Read),
            (Common.Domain.Constants.Resources.AccountFeatures, Actions.Read),
        ]),
        // The Router/SyncWorker side gets TripTracking/Custom and NOTHING else in this module, so
        // those two identities can call processTripPositions only.
        // NOTE: db-init MUST be re-run on existing deployments after a change here, or the affected
        // calls return FORBIDDEN — the resource catalog, role grants and service-client permission
        // rows are what the authorization pipeline consults at enforcement time.
        (["router_client", "syncworker_client"],
        [
            (Common.Domain.Constants.Resources.TripTracking, Actions.Custom),
        ]),
    ];
}
