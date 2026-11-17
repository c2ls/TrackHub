# TrackHub Geofencing

Geographic zones and the visits transporters make to them, in the `geofencing` schema of the
`TrackHub` database. It evaluates the positions the [Router](../TrackHubRouter) forwards with
PostGIS, records one row per visit (entry plus a nullable departure), runs the dwell evaluator, and
emits alert events to [Manager](../TrackHub.Manager). Gated by the `geofencing` account feature.

Wiki: [Geofencing](https://github.com/shernandezp/TrackHub/wiki/Geofencing).

## Build, run, test

From this folder:

```bash
dotnet build TrackHub.Geofencing.slnx
dotnet test TrackHub.Geofencing.slnx

# needs CREATE EXTENSION postgis in TrackHub, run once as a superuser
dotnet ef database update --project src/Infrastructure/ManagerDB --startup-project src/Web

dotnet run --project src/Web          # https://localhost:5010/graphql
```

Settings: `ConnectionStrings:DefaultConnection` in `src/Web/appsettings.json` (the `TrackHub`
database) and `AppSettings:GraphQLManagerService`. The migrations project is named `ManagerDB`
for historical reasons; it holds this service's own context.

## Gotchas

- **A circle is stored as metadata plus a buffered polygon.** `CircleCenter` and
  `CircleRadiusMeters` sit beside `Geom`, which always holds a polygon, so detection, indexing and
  reports are shape-agnostic.
- **A geofence with recorded visits cannot be deleted** (`CONFLICT`). Deactivation retires a zone
  and keeps its history readable.
- **Alert emission uses this service's own `geofence_client` identity**, never the caller's token.
  It needs `Alerts/Write` and `BackgroundJobs/Write` in `security.service_client_permissions`, or
  every emission returns `FORBIDDEN`.
- **Emission is best-effort.** A failure is logged and never fails position processing.
- **The dwell evaluator stamps `DwellAlertedAt` only after a successful emission.** An old
  `BackgroundJobRun` timestamp for it is the healthy steady state: it records only when it has work.
- **Geometry is validated, not approximated.** Circle centres beyond ±85° latitude, rings crossing
  the ±180° meridian, self-intersecting polygons and out-of-range coordinates are rejected by
  `GeofenceDtoValidator`.
- **Visit history is served to Reporting**, through the *Geofence Events* report; the portal has no
  direct query for it.
- `app.accounts`, `app.account_features` and `app.audit_events` are mapped read-only and excluded
  from this service's migrations.
- After changing the GraphQL surface, run `dotnet test ../TrackHub.IntegrationTests` and then the
  portal's `npm run codegen`.
