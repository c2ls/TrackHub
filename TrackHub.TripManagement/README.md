# TrackHub Trip Management

Plans, dispatches and tracks trips: the zero-touch lifecycle (trips arm, start and complete from
their origin and stop zones), ordered stops and deliveries, route plans and a deviation corridor
from OpenRouteService, the toll catalog and estimates, proof of delivery, and public tracking links.
It owns the `trip` schema of the `TrackHub` database; its tenant surface is gated by the
`trip-management` account feature.

Wiki: [Trip Management](https://github.com/shernandezp/TrackHub/wiki/Trip-Management).

## Build, run, test

From this folder:

```bash
dotnet build TrackHub.TripManagement.slnx
dotnet test TrackHub.TripManagement.slnx

# needs CREATE EXTENSION postgis in TrackHub, run once as a superuser
dotnet ef database update --project src/Infrastructure/TripDB --startup-project src/Web

dotnet run --project src/Web          # https://localhost:5006/graphql
```

Settings in `src/Web/appsettings.json`: `ConnectionStrings:DefaultConnection` (the `TrackHub`
database), `AppSettings:GraphQLManagerService`, `AppSettings:GraphQLTelemetryService`, and
`AppSettings:Routing` — provider, base URL, `ApiKey` and profile. Get a free key at
<https://openrouteservice.org/dev/#/signup> or point `BaseUrl` at a self-hosted instance; without
one every route plan fails with `ROUTING_NOT_CONFIGURED` while trips keep working.

## Gotchas

- **Migrations seed nothing.** The `trip_client` identity and the `Trips` / `TripTracking` /
  `TollCatalog` resources come from the Security seeder; without them every trip call returns
  `FORBIDDEN` while the service reports healthy.
- **This service registers its own `IFeatureFlagService`** (`Infrastructure/TripDB/DependencyInjection.cs`).
  Common has no default and the host refuses to start without one.
- **`TollCatalog` is deliberately not feature-gated.** The toll catalog is platform reference data
  with no account id.
- **Detection state is persisted, never held in memory.** The Router pushes one position per
  transporter per call, so a test that feeds a whole scenario in one call proves nothing about the
  deployed behaviour.
- **Arrival geometry is snapshotted when the trip starts.** The linked geofence polygon is copied
  into `TripStop.ArrivalGeom` by a read-only projection of `geofencing.geofences` — there is no
  TripManagement → Geofencing service call — so editing the geofence mid-trip does not move it.
- **The public tracking surface is not output-cached and answers 404 when the feature is off.** A
  cached response would skip the access count and audit and keep serving a revoked link; a
  `FEATURE_DISABLED` would reveal that the trip exists.
- **Disclosure flags fail closed.** What a public recipient sees comes from the `trip_shares` flags,
  all false by default.
- **Toll estimates are never silently understated.** A station with no tariff for the trip's class
  marks the estimate `PartialNoTariff`; an empty catalog gives a null estimate, not zero.
- **An OpenRouteService failure leaves the route plan `Failed`**; it never blocks a trip command.
- `trip-eta-refresh` and `trip-schedule-reminder` record a job run only when they did work, so an
  old timestamp is the healthy steady state.
- After changing the GraphQL surface, run `dotnet test ../TrackHub.IntegrationTests` and then the
  portal's `npm run codegen`.
