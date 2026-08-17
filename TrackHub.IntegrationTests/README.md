# TrackHub.IntegrationTests

In-process integration tests for the GraphQL contracts that couple the TrackHub services together (Router, Manager, Telemetry, Security, Geofence, Reporting). One `dotnet test` gives a deterministic red/green signal for every service-to-service call — no Docker, no database, no running services required.

## What is guaranteed

- **Layer A — contract validation**: every GraphQL query string a consumer ships (the exact `internal const` production sends, exposed via `InternalsVisibleTo`) is validated against the producer's real, in-process-built schema. A renamed or removed field, argument, or input property fails the matching test with a message naming the call.
- **Layer B — round-trip execution**: for the critical/complex flows, the consumer's real reader/writer executes against the producer's real resolvers over an in-process `IGraphQLClient`; only the mediator (`ISender`) behind the resolvers is faked. This catches serialization drift Layer A cannot: enum/UUID/DateTime coercion, casing, and field-to-property mapping on both sides.

Covered pairs: Router→Manager, Router→Telemetry, Router→Geofence, Reporting→Manager, Reporting→Telemetry, Reporting→Router, Reporting→Geofence, Manager→Security, Manager→Router, Security→Manager.

## Layout

| Project | Purpose |
|---|---|
| `src/TrackHub.ServiceContracts.Harness` | Test-support library: `InProcessGraphQLClient` (an `IGraphQLClient` over a producer `IRequestExecutor`), the client factory, and the producer schema/executor builder that reuses the production `AddTrackHubGraphQLServer` configuration. |
| `tests/TrackHub.ServiceContracts.Tests` | Contract + round-trip tests for all producer/consumer pairs (Manager, Telemetry, Security, Router, Geofence). |

## Prerequisites

The projects reference the service source by **relative path**. Every service lives in this
repository, so a plain clone is all that is needed — nothing to check out side by side, and no
local `TrackHubCommon` feed, since Common is referenced as a project like everything else.

## Run

```bash
dotnet test TrackHub.IntegrationTests.slnx
```

Runs in seconds. Run it after any edit to a service's GraphQL surface or to a reader/writer client — a failure names the exact broken call.