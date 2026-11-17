# TrackHubCommon

The shared framework every backend service builds on. It is not packaged: each service references
the projects directly (`$(RepoRoot)TrackHubCommon/src/<Project>/<Project>.csproj`, with `RepoRoot`
set by the root `Directory.Build.props`), so there is no version to bump and no feed to configure.

| Project | Contents |
|---|---|
| `Common.Domain` | Constant catalogs (`Resources`, `Actions`, `Roles`, `Policies`, `Clients`, `FeatureKeys`, `BackgroundJobKeys`, `Reports`, schema/table metadata), enums, cryptography, localization, domain-event primitives |
| `Common.Application` | The CQRS behavior pipeline (logging, validation, authorization, tenant scope, feature flags, rate limiting), attributes |
| `Common.Infrastructure` | EF conventions and interceptors, the GraphQL client factory, `IdentityService`, the Serilog sink |
| `Common.Web` | `AddTrackHubGraphQLServer<TQuery, TMutation>`, error filters, forwarded-header trust, security scheme transformers |
| `Common.Pdf` | The PDF document builder Reporting renders with |

Wiki: [Common Library](https://github.com/shernandezp/TrackHub/wiki/Common-Library).

## Build and test

From this folder:

```bash
dotnet build TrackHub.Common.slnx
dotnet test TrackHub.Common.slnx
```

A change here rebuilds every consumer, so finish with `dotnet build TrackHub.slnx` from the
repository root and change Common and its consumers in one commit.

## Gotchas

- **The mediator is the `Common.Mediator` package** (`IRequest<T>`, `IRequestHandler<,>`,
  `ISender`, `IPublisher`), referenced by `Common.Domain`. MediatR is not used and must not be
  added.
- **The status and feature gates have no fail-open default.** A service whose pipeline enforces
  `[RequireFeature]` must register its own `IFeatureFlagService` (and every service an
  `IAccountOperationalStatusService`); a missing registration stops the host at startup.
- **`AccountScopeBehavior` is fail-closed**: a request without a resolvable tenant scope is
  rejected.
- **Adding an authorization resource here is half the job.** It must also be seeded and granted in
  TrackHubSecurity's `IRbacSeedContribution` files, see
  [Seeding rules that bite](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity#seeding-rules-that-bite).
- **The constant catalogs are the contract.** Resource, action, feature, schema and table names are
  never string literals at a call site — a typo is a silent authorization or mapping failure.
- **Registries are `static partial class`**, so additions land in separate partial files.
- **Register inter-service clients only through `AddGraphQLClient`**, which applies timeouts, header
  propagation and an explicit resilience policy (no retry unless the client is query-only).
- Docker image builds copy the `TrackHubCommon/` source into the build context and compile it as a
  project reference.
