# TrackHub Geofencing API

[← Back to the landing page](README.md) · [Español](README.es.md)

The Geofencing API owns geographic zones and the visit events transporters generate against them. It evaluates positions fed by the [Router](https://github.com/shernandezp/TrackHubRouter) with PostGIS spatial queries and emits alert events through the [Management API](https://github.com/shernandezp/TrackHub.Manager).

Built on .NET 10 with a HotChocolate GraphQL endpoint and NetTopologySuite geometry. Gated by the `geofencing` account feature.

---

## What it owns

| Table | Purpose |
|---|---|
| `geofencing.geofences` | Zone definitions — polygon geometry, circle metadata, alert opt-ins, dwell threshold |
| `geofencing.geofenceevents` | One row per **visit**: an entry timestamp plus a nullable departure |

Two behaviours are worth knowing up front:

- **A circle is stored as metadata plus a buffered polygon.** `CircleCenter` and `CircleRadiusMeters` sit alongside `Geom`, which always holds a polygon (a 64-segment buffer computed at write time). Detection, indexing, reporting and overlays are therefore shape-agnostic; editors render the true circle from the metadata.
- **One row is one visit**, not one event. Entry and exit share a row, with a 30 s exit debounce and an idempotency guard.

Full detail: **[Geofencing](https://github.com/shernandezp/TrackHub/wiki/Geofencing)** in the wiki.

---

## Quick start

### Prerequisites

- .NET 10 SDK
- PostgreSQL 14+ **with PostGIS enabled**
- A running TrackHub AuthorityServer and Management API
- The `TrackHubCommon.*` packages available from a local NuGet feed

### Steps

1. **Clone**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.Geofencing.git
   cd TrackHub.Geofencing
   ```

2. **Enable PostGIS** in the `TrackHub` database (one `CREATE EXTENSION` also covers the `trip` schema — they share the database):

   ```sql
   CREATE EXTENSION IF NOT EXISTS postgis;
   ```

3. **Configure the database connection** in `src/Web/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHub;Username=postgres;Password=yourpassword"
     }
   }
   ```

4. **Apply migrations** — this creates the `geofencing` schema:

   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Web
   ```

5. **Run**

   ```bash
   dotnet run --project src/Web
   ```

6. **Open the GraphQL endpoint** at `https://localhost:<port>/graphql`.

---

## Project-specific notes

- **Never add `[Caching]` to `geofencesByAccount`.** The cache key is built from request properties only and cannot scope to the caller's account, so a cached response leaks geofences across tenants. The query's `enableCaching` input is deliberately inert. This is the case that established the platform-wide rule.
- **A geofence with recorded visits cannot be deleted** — the attempt returns `ConflictException` → `CONFLICT`. Visit history is permanent; deactivation is the supported way to retire a zone, and a deactivated geofence keeps its history readable.
- **Alert emission uses this service's own `geofence_client` identity** (`CreateClient(Clients.Manager, asService: true)`), never the propagated caller token. That identity needs `Alerts/Write` and `BackgroundJobs/Write` seeded in `security.service_client_permissions`, or every emission returns `FORBIDDEN`.
- **Emission is best-effort.** A failure logs and never fails position processing — a Manager outage must not stop detection or lose positions.
- **The dwell evaluator stamps `DwellAlertedAt` only after a successful emission**, which is retry-safe thanks to Manager's dedup. It is an on-work-only recorder: an old `BackgroundJobRun` timestamp is the healthy steady state.
- **Geometry is validated, not silently degraded.** Circle centres beyond ±85° latitude, circle rings crossing the ±180° meridian, self-intersecting polygons and out-of-range coordinates are all 400s from `GeofenceDtoValidator`. Planar PostGIS plus normalized positions would misdetect the meridian case, so it is rejected rather than approximated.
- **Geofence event history is not a portal query.** It is served through the Reports screen's *Geofence Events* report — the portal ships no `geofenceEvents` document, because Reporting is that query's consumer.
- The `app.accounts`, `app.account_features` and `app.audit_events` tables are mapped **read-only** and excluded from this service's migrations.
- After changing any GraphQL surface, run the contract tests:

  ```bash
  dotnet test ../TrackHub.IntegrationTests/TrackHub.IntegrationTests.slnx
  ```

---

## Documentation

- **Technical** — the [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki): [Geofencing](https://github.com/shernandezp/TrackHub/wiki/Geofencing), [Database](https://github.com/shernandezp/TrackHub/wiki/Database), [Inter-Service Communication](https://github.com/shernandezp/TrackHub/wiki/Inter-Service-Communication)
- **User** — in the app: the Help button or **F1** on any screen
- **Deployment** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
