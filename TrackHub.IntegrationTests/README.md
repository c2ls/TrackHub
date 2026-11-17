# TrackHub.IntegrationTests

In-process tests for the GraphQL contracts between services. One `dotnet test` gives a red/green
signal for every service-to-service call — no Docker, no database, no running services.

- **Contract tests** validate every query a consumer ships (the production `internal const`,
  exposed through `InternalsVisibleTo`) against the producer's real, in-process schema. A renamed
  or removed field, argument or input property fails the test naming that call.
- **Round-trip tests** run the consumer's real reader or writer against the producer's real
  resolvers over an in-process `IGraphQLClient`, with only the mediator behind the resolvers faked.
  They catch what validation cannot: enum, UUID and date coercion, casing and field mapping.
- The suite also writes each producer's SDL to `TrackHub.Portal/schemas/<service>.graphql`, which the
  portal's `npm run codegen` checks every frontend operation against.

Wiki: [Testing Strategy](https://github.com/shernandezp/TrackHub/wiki/Testing-Strategy).

## Run

```bash
dotnet test TrackHub.IntegrationTests.slnx     # from this folder
```

The projects reference the services by relative path, so a plain clone is all it needs. Run it
after any change to a GraphQL surface or to a client reader/writer.

| Project | Purpose |
|---|---|
| `src/TrackHub.ServiceContracts.Harness` | `InProcessGraphQLClient`, the client factory, and the producer schema/executor builder, which reuses the production `AddTrackHubGraphQLServer` configuration |
| `tests/TrackHub.ServiceContracts.Tests` | `ContractTests/` and `RoundTripTests/` per producer/consumer pair, plus the SDL export |

## Gotchas

- **Never re-type a query string in a test.** Reference the production `internal const`, or the test
  validates a copy that has already drifted.
- **Enum values travel in variables and are invisible to contract validation.** Every enum literal
  a client sends needs a round-trip case asserting it coerces into the producer's enum.
- A new inter-service call needs a contract test; a critical or complex flow also gets a round trip.
- Commit the regenerated `TrackHub.Portal/schemas/*.graphql` with the change that produced them.
