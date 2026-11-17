# TrackHub Router

The GPS integration layer. It talks to each external tracking provider through its own API,
normalizes positions into one shape (km/h, decimal degrees, UTC), and serves live and historical
positions, device sync, manual sync and connectivity ping. This folder also holds the **SyncWorker**,
the background host that runs the position, device and health loops against every operator.

The Router has no database: master data comes from [Manager](../TrackHub.Manager); positions,
history, health checks and sync runs go to [Telemetry](../TrackHub.Telemetry); detection feeds go to
[Geofencing](../TrackHub.Geofencing) and [Trip Management](../TrackHub.TripManagement).

Implemented providers: CommandTrack, Traccar, Flespi, Geotab, GpsGate, Navixy, Samsara, Wialon,
Protrack, MettaX IoT, Tracksolid Pro, Motive, Cartrack, Pegasus Gateway, Webfleet, RedGPS, Plaspy,
Ruptela TrustTrack, Mapon and GPS Trackit — one project each under `src/Infrastructure/<Protocol>`.

Wiki: [Router](https://github.com/shernandezp/TrackHub/wiki/Router) (provider table, capabilities,
sync loops), [Adding a Provider](https://github.com/shernandezp/TrackHub/wiki/Adding-a-Provider).

## Build, run, test

From this folder:

```bash
dotnet build TrackHub.Router.slnx
dotnet test TrackHub.Router.slnx

dotnet run --project src/Web          # https://localhost:5002/graphql, REST reference at /scalar
dotnet run --project src/SyncWorker   # optional, the background loops
```

Settings, in **both** `src/Web/appsettings.json` and `src/SyncWorker/appsettings.json`:
`AppSettings:GraphQL{Manager,Telemetry,Geofence,TripManagement}Service`, `AppSettings:EncryptionKey`
(must match Manager's) and `AppSettings:Protocols`, the enabled providers. Operator credentials are
entered through the portal and stored by Manager. The `/scalar` and OpenAPI endpoints exist in the
Development environment only.

## Gotchas

- **A misconfigured provider fails at startup, on purpose.** A listed protocol with no assembly, no
  readers, or a capability declaration that does not match its readers throws instead of being
  skipped.
- **Adding a provider touches three places:** the `ProtocolType` value in TrackHubCommon, the
  provider project (namespace `TrackHub.Router.Infrastructure.<Protocol>`, readers with matching
  `Protocol`, a `ProviderDescriptor`), and `AppSettings:Protocols` in **both** Web and SyncWorker.
  The capability catalog and the portal's provider list build themselves from the descriptors.
- **A provider's polling limit outranks the account's storing interval.** `ProviderDescriptor`
  may declare `MinPositionIntervalSeconds`; the position loop skips an operator polled more
  recently than that.
- **`Ping` must make a real provider round-trip.** Sessions are cached in `IProviderSessionStore`,
  so a ping that only initializes would report HEALTHY while the provider is down.
- **Provider errors must throw, never degrade to empty results.** A swallowed error becomes a
  "successful" sync of 0 positions while health stays green. Wialon and Navixy report failures as
  HTTP 200 with an error payload; both are detected.
- **The SyncWorker is single-instance by design.** `IOperatorSyncLock`, `ExecutionIntervalManager`
  and `IProviderSessionStore` are in-process; scaling out needs a cross-instance claim first.
- **Tenant scope follows the caller; credentials are read with the service identity.** Resolve the
  operator through `IOperatorReader` (caller's token), then the decrypted credential through
  `IOperatorSystemReader`. Never widen a user's permissions to reach credential material.
- **Provider `HttpClient`s disable auto-redirect**, so an operator-configured URL cannot redirect
  the Router to an internal address.
- `AttributesVm.Hourmeter` is in hours; `Mileage` is not converted at the mapper boundary.
- After changing the GraphQL surface, run `dotnet test ../TrackHub.IntegrationTests` and then the
  portal's `npm run codegen`.
