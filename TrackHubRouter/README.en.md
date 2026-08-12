# TrackHub Router API

[← Back to the landing page](README.md) · [Español](README.es.md)

The Router is TrackHub's **multi-protocol integration layer**. It connects to external GPS tracking providers — each with its own API technology — and standardizes their data into a unified format the rest of the platform consumes.

This repository also contains the **SyncWorker**, the background host that drives synchronization.

Built on .NET 10 with a HotChocolate GraphQL endpoint plus a REST surface for third-party integration.

---

## What it does

- **Multi-provider integration** — nine GPS providers implemented (CommandTrack, Traccar, Flespi, GeoTab, GpsGate, Navixy, Samsara, Wialon, Protrack)
- **Data normalization** — provider payloads become one `PositionVm` shape: speed in km/h, decimal degrees, UTC timestamps
- **Real-time and historical position retrieval** across every connected operator
- **Background synchronization** — the SyncWorker's position, device and health loops
- **Manual sync and connectivity ping** as authorized, rate-limited user features
- **Provider capability declaration** — a request for something a provider cannot do fails as a *provider* limitation, not a TrackHub error

The Router has **no database of its own**. Master data goes through the [Management API](https://github.com/shernandezp/TrackHub.Manager); positions, history, health checks and sync runs go to the [Telemetry API](https://github.com/shernandezp/TrackHub.Telemetry); detection feeds go to [Geofencing](https://github.com/shernandezp/TrackHub.Geofencing) and [Trip Management](https://github.com/shernandezp/TrackHub.TripManagement).

Full detail: **[Router](https://github.com/shernandezp/TrackHub/wiki/Router)** in the wiki.

---

## Quick start

### Prerequisites

- .NET 10 SDK
- A running TrackHub AuthorityServer, Management API and Telemetry API
- At least one GPS provider account (Traccar and CommandTrack are the tested ones)
- The `TrackHubCommon.*` packages available from a local NuGet feed

### Steps

1. **Clone**

   ```bash
   git clone https://github.com/shernandezp/TrackHubRouter.git
   cd TrackHubRouter
   ```

2. **Configure the downstream services and enabled protocols** in `src/Web/appsettings.json` (and `src/SyncWorker/appsettings.json`):

   ```json
   {
     "AppSettings": {
       "GraphQLManagerService": "https://localhost:5001/graphql",
       "GraphQLTelemetryService": "https://localhost:5011/graphql",
       "GraphQLGeofenceService": "https://localhost:5004/graphql",
       "GraphQLTripManagementService": "https://localhost:5006/graphql",
       "Protocols": [ "CommandTrack", "Traccar" ],
       "MaxConcurrentOperatorSyncs": 10,
       "DeviceCatalogCacheSeconds": 60
     }
   }
   ```

3. **Configure operator credentials** through the Management API or the web portal — the Router reads them, it does not store them.

4. **Run the API**

   ```bash
   dotnet run --project src/Web
   ```

5. **Run the background worker** (optional, in a second terminal)

   ```bash
   dotnet run --project src/SyncWorker
   ```

6. **Open** the GraphQL endpoint at `https://localhost:<port>/graphql` and the REST reference at `https://localhost:<port>/scalar`.

---

## Supported providers

| Provider | Documentation | Status | Tested |
|---|---|---|---|
| CommandTrack | [Docs](https://www.c2ls.co/home/documentacion-de-la-api/) | ✅ Implemented | ✅ |
| Traccar | [Docs](https://www.traccar.org/api-reference/) | ✅ Implemented | ✅ |
| Flespi | [Docs](https://flespi.io/docs/) | ✅ Implemented | ❌ |
| GeoTab | [Docs](https://developers.geotab.com/myGeotab/guides/codeBase/usingInDotnet) | ✅ Implemented | ❌ |
| GpsGate | [Docs](https://support.gpsgate.com/hc/en-us/articles/360016602140-REST-API-Documentation) | ⚠️ No position-history API | ❌ |
| Navixy | [Docs](https://www.navixy.com/docs/navixy-api/user-api/getting-started) | ✅ Implemented | ❌ |
| Samsara | [Docs](https://developers.samsara.com/docs/tms-integration) | ✅ Implemented | ❌ |
| Wialon | [Docs](https://help.wialon.com/en/api/user-guide) | ✅ Implemented | ❌ |
| Protrack | — | ✅ Implemented | ❌ |

`Mettax` is reserved in the `ProtocolType` enum with no provider assembly — configuring it throws at startup.

**Adding a provider**: the full walkthrough is [Adding a Provider](https://github.com/shernandezp/TrackHub/wiki/Adding-a-Provider) in the wiki.

---

## Project-specific notes

- **A misconfigured provider fails at startup, on purpose.** A configured protocol that resolves no assembly, no reader types, or a capability declaration mismatching its readers throws during startup rather than being silently skipped.
- **Three things must align when adding a provider**: the `ProtocolType` enum value (in TrackHubCommon), the provider assembly (namespace `TrackHub.Router.Infrastructure.{Protocol}`, readers with matching `Protocol` properties, and a `ProviderDescriptor` declaring display name and capabilities), and the `AppSettings:Protocols` entry (**in both Web and SyncWorker**). The capability matrix and client-facing provider list build themselves from the discovered descriptors.
- **`Ping` must always perform a real provider round-trip.** Sessions are cached in `IProviderSessionStore`, so an `Init`-only ping would be a pure in-memory hit — turning the health monitor into a no-op that reports HEALTHY while the provider is down.
- **Provider errors must throw, never degrade to empty results.** A swallowed error becomes a "successful" sync of 0 positions, which silently stops position flow while Health stays green. Wialon and Navixy report failures as HTTP 200 with an error payload — both are detected and thrown.
- **The SyncWorker is single-instance by design.** `IOperatorSyncLock`, `ExecutionIntervalManager` and `IProviderSessionStore` are all in-process. Scaling it out needs a cross-instance claim (a PostgreSQL advisory lock or `SKIP LOCKED`).
- **Tenant scope follows the caller; credentials are read with the service identity.** Interactive surfaces resolve the operator through `IOperatorReader` (propagating the caller's token, so Manager applies their visibility), then read the decrypted credential through `IOperatorSystemReader`. Never widen a user's permissions to obtain credential material.
- **Provider `HttpClient`s disable auto-redirect** — an operator-configured base URL must not be able to 302-redirect the Router inward.
- `AttributesVm.Hourmeter` is in **hours**; `Mileage` has **no** conversion at the mapper boundary, so do not assume metres or kilometres without checking the provider.
- After changing any GraphQL surface, run the contract tests:

  ```bash
  dotnet test ../TrackHub.IntegrationTests/TrackHub.IntegrationTests.slnx
  ```

---

## Documentation

- **Technical** — the [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki): [Router](https://github.com/shernandezp/TrackHub/wiki/Router), [Adding a Provider](https://github.com/shernandezp/TrackHub/wiki/Adding-a-Provider), [Telemetry](https://github.com/shernandezp/TrackHub/wiki/Telemetry), [Inter-Service Communication](https://github.com/shernandezp/TrackHub/wiki/Inter-Service-Communication)
- **User** — in the app: the Help button or **F1** on any screen
- **Deployment** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
