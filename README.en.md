# TrackHub Trip Management API

[← Back to the landing page](README.md) · [Español](README.es.md)

The Trip Management API plans, dispatches and tracks trips: ordered stops, deliveries, route geometry with a deviation corridor, toll estimation, proof of delivery, and shareable public tracking links.

Built on .NET 10 with a HotChocolate GraphQL endpoint and NetTopologySuite geometry at SRID 4326. It owns the **`trip`** schema in the shared `TrackHub` database, and its whole tenant surface is gated by the `trip-management` account feature.

---

## What it does

- **Trip lifecycle** — a governed status machine (`Created → InProgress → Paused → Completed / Cancelled / Aborted`) with a single transition matrix as the source of truth
- **Stop and delivery planning** — ordered stops with add/update/remove/reorder, arrival and departure progress, skip handling, and per-stop delivery outcomes
- **Route planning** — route geometry and corridor buffers from OpenRouteService, stored as PostGIS geometry
- **Toll estimation** — a toll station, tariff and vehicle-class catalog with route-based cost estimation and explicit partial-coverage reporting
- **Detection** — stop arrivals, departures, delays and corridor deviations, computed from telemetry positions
- **Proof of delivery** — signature, photo and document capture linked to the document service, with clean-scan enforcement
- **Public tracking links** — anonymous, revocable, rate-limited, disclosure-controlled per share

Full detail: **[Trip Management](https://github.com/shernandezp/TrackHub/wiki/Trip-Management)** in the wiki.

---

## Quick start

### Prerequisites

- .NET 10 SDK
- PostgreSQL 14+ **with PostGIS enabled**
- A running TrackHub AuthorityServer, Management API and Telemetry API
- An OpenRouteService API key, for route planning
- The `TrackHubCommon.*` packages available from a local NuGet feed

### Steps

1. **Clone**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.TripManagement.git
   cd TrackHub.TripManagement
   ```

2. **Enable PostGIS** in the `TrackHub` database (one `CREATE EXTENSION` also covers the `geofencing` schema):

   ```sql
   CREATE EXTENSION IF NOT EXISTS postgis;
   ```

3. **Configure the database connection and the routing provider** in `src/Web/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHub;Username=postgres;Password=yourpassword"
     },
     "AppSettings": {
       "GraphQLManagerService": "https://localhost:5001/graphql",
       "GraphQLTelemetryService": "https://localhost:5011/graphql",
       "Routing": {
         "Provider": "OpenRouteService",
         "BaseUrl": "https://api.openrouteservice.org",
         "ApiKey": "your-api-key",
         "Profile": "driving-hgv"
       }
     }
   }
   ```

4. **Apply migrations** — this creates the `trip` schema:

   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Web
   ```

5. **Run**

   ```bash
   dotnet run --project src/Web
   ```

6. **Open the GraphQL endpoint** at `https://localhost:5006/graphql`.

In development the service listens on `https://localhost:5006` and `http://localhost:5007`; behind nginx it sits at `/Trip/`.

---

## Project-specific notes

- **Re-run `db-init` after deploying this service to an existing installation.** The migration creates the schema but seeds nothing. Without the `trip_client` registration and the `Trips` / `TripTracking` / `TollCatalog` resource and role seeding, **every trip call returns `FORBIDDEN` while the service reports healthy.**
- **This service registers its own `IFeatureFlagService`** (`Infrastructure/TripDB/DependencyInjection.cs`). Common's default is fail-**open** (`AlwaysEnabledFeatureFlagService`, `TryAddScoped`), so removing that registration would silently disable every `[RequireFeature]` check.
- **`Resources.TollCatalog` is deliberately not feature-gated.** The toll catalog is Administrator-maintained platform reference data with no `AccountId`; gating it on a tenant feature would misclassify it.
- **Detection state is persisted, never held in memory.** The Router pushes **one position per transporter per call**, so any debounce or run length kept in memory could never elapse. A test that feeds a whole scenario in a single call proves nothing about deployed behaviour — mirror the one-position-per-call reality.
- **Arrival geometry is snapshotted at `StartTrip`.** The linked geofence polygon is read once, into `TripStop.ArrivalGeom`, so editing that geofence mid-trip cannot move a running trip's arrival area. This read goes straight to `geofencing.geofences` as a read-only projection — **there is no TripManagement → Geofencing service call.**
- **The public tracking endpoint is deliberately not output-cached.** A cache hit would never reach Manager's resolver, so it would neither count the access nor write the `PublicLinkAccessed` audit event — and it would keep serving a revoked link. It also returns **404, not `FEATURE_DISABLED`**, when the feature is off: the page must not reveal that a trip exists.
- **Disclosure flags fail closed.** What a public recipient sees is driven by the `trip_shares` field flags, which all default to false — never by client-side filtering.
- **Toll estimates are explainable, never silently understated.** A matched station with no tariff for the trip's class contributes 0 and sets `TollStatus = PartialNoTariff`; an empty catalog yields `NoStations` and a **null** estimate rather than a fabricated number. Tariffs are temporal, so a historical trip's estimate stays explainable.
- **ORS failure degrades to `RoutePlan.Status = Failed` — it never blocks a trip command.**
- Both background jobs (`trip-eta-refresh`, `trip-schedule-reminder`) are **on-work-only recorders**: an old `BackgroundJobRun` timestamp is the healthy steady state, not a stuck job.
- Performance note: the `ST_DWithin(..., TRUE)` geography predicate used for toll-station matching cannot use the geometry GiST index.
- After changing any GraphQL surface, run the contract tests:

  ```bash
  dotnet test ../TrackHub.IntegrationTests/TrackHub.IntegrationTests.slnx
  ```

---

## Documentation

- **Technical** — the [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki): [Trip Management](https://github.com/shernandezp/TrackHub/wiki/Trip-Management), [Database](https://github.com/shernandezp/TrackHub/wiki/Database), [Inter-Service Communication](https://github.com/shernandezp/TrackHub/wiki/Inter-Service-Communication), [Reporting](https://github.com/shernandezp/TrackHub/wiki/Reporting)
- **User** — in the app: the Help button or **F1** on any screen
- **Deployment** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
