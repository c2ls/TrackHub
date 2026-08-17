# TrackHub Common Library

[← Back to the landing page](README.md) · [Español](README.es.md)

TrackHubCommon is the shared foundation every TrackHub backend service builds on. It is **not packaged** — every backend service references its four projects directly.

| Project | Contents |
|---|---|
| `TrackHubCommon.Domain` | Constants, enums, cryptography extensions, localization, domain-event primitives |
| `TrackHubCommon.Application` | The custom CQRS mediator, the behavior pipeline, attributes, testing helpers |
| `TrackHubCommon.Infrastructure` | EF conventions and interceptors, the GraphQL client factory, `IdentityService` |
| `TrackHubCommon.Web` | GraphQL server registration, error filters, security scheme transformers |

The structure follows [Jason Taylor's Clean Architecture template](https://github.com/jasontaylordev/CleanArchitecture), adapted to TrackHub's needs.

Full detail: **[Common Library](https://github.com/shernandezp/TrackHub/wiki/Common-Library)** in the wiki.

---

## What it provides

- **The custom CQRS mediator** (`Common.Mediator`) — `IRequest<TResult>`, `IRequestHandler<,>`, `MediatorDispatcher : ISender, IPublisher`. **MediatR is not used and is forbidden.**
- **The behavior pipeline** — logging, validation, authorization, fail-closed tenant scoping, caching, rate limiting, unhandled-exception handling
- **The cross-service constant catalogs** — `Resources`, `Actions`, `Roles`, `Policies`, `Clients`, `FeatureKeys`, `BackgroundJobKeys`, `Reports`, and the schema/table/column/view metadata
- **Cryptography** — BCrypt for user passwords, server-certificate encryption for third-party secrets such as GPS provider credentials
- **Localization** — `ResourceLocalizer`, the single primitive for server-rendered text from `.resx`
- **EF interceptors and conventions** — audit columns, post-commit domain-event dispatch, `UseUtcTimestamps()`
- **The GraphQL client factory** — the only sanctioned way to register an inter-service client, applying timeouts, header propagation and an explicit resilience policy
- **`AddTrackHubGraphQLServer<TQuery, TMutation>`** — the single definition of the platform's GraphQL server configuration

---

## Quick start

### Prerequisites

- .NET 10 SDK

### Consuming Common

Common is **not** packaged. Every service in the monorepo references the projects directly, so
there is no version to pin and no feed to configure:

```xml
<ItemGroup>
  <ProjectReference Include="$(RepoRoot)TrackHubCommon/src/Common.Domain/Common.Domain.csproj" />
  <ProjectReference Include="$(RepoRoot)TrackHubCommon/src/Common.Application/Common.Application.csproj" />
  <ProjectReference Include="$(RepoRoot)TrackHubCommon/src/Common.Infrastructure/Common.Infrastructure.csproj" />
  <ProjectReference Include="$(RepoRoot)TrackHubCommon/src/Common.Web/Common.Web.csproj" />
</ItemGroup>
```

`$(RepoRoot)` is defined in each service's `Directory.Build.props` as the monorepo root.

Register the services in each layer's `DependencyInjection.cs`:

```csharp
// Application layer
services.AddApplicationServices(typeof(SomeHandler).Assembly);
services.AddDistributedMemoryCache();   // required — CachingBehavior resolves IDistributedCache

// Web layer
builder.Services
    .AddTrackHubGraphQLServer<Query, Mutation>(builder.Environment.IsDevelopment());
```

### Building

```bash
dotnet build TrackHub.slnx     # from the monorepo root — builds Common and every consumer
```

---

## Changing Common

Edit the code and build. That is the whole procedure.

Because consumers reference the projects rather than packages, a change to Common rebuilds every
service that uses it in the same build, and the compiler reports breakage immediately. There is no
version to bump, nothing to pack, no feed to copy into, and no NuGet cache to purge — the
`CS0117 'Resources' does not contain …` class of error caused by a stale extracted package cannot
happen any more.

Change Common and its consumers in **one commit**.

---

## Project-specific notes

- **Every consumer moves together, automatically.** All services and the ServiceContracts harness
  reference the projects, so they always compile against the current source — nothing can be pinned
  back to an older Common, and a breaking change surfaces at build time rather than after a publish.
- **`AccountScopeBehavior` is fail-closed; the default `IFeatureFlagService` is fail-open.** That is deliberate: a missing tenant scope is a security failure, while a missing feature registration is a service-configuration failure the service's own tests should catch. A service that uses `[RequireFeature]` **must** register its own `IFeatureFlagService`.
- **`AddDistributedMemoryCache()` is not optional.** `CachingBehavior` resolves `IDistributedCache` for every request type; a missing registration fails **every** request with a masked DI error.
- **Adding an authorization resource is not enough.** It must also be added to `TrackHubSecurity`'s `ApplicationDbContextInitializer` `DefaultResources`, and granted in each role's matrix — [two separate steps](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity#seeding-rules-that-bite).
- **The constant catalogs are the contract.** Resource, action, feature-key, schema and table names are never string literals at a call site — a typo becomes a silent authorization or mapping failure.
- Verifying that a constant landed in a built DLL is best done with `grep -a`; UTF-16 metadata literals defeat plain `strings`.
- Docker image builds copy the TrackHubCommon source into the build context and restore it as a project reference — there is no packing stage and no feed to pre-populate, in a deployment or locally.

---

## Documentation

- **Technical** — the [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki): [Common Library](https://github.com/shernandezp/TrackHub/wiki/Common-Library), [Architecture](https://github.com/shernandezp/TrackHub/wiki/Architecture), [Coding Standards](https://github.com/shernandezp/TrackHub/wiki/Coding-Standards)
- **Deployment** — [TrackHub.Deployment](../TrackHub.Deployment)

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
