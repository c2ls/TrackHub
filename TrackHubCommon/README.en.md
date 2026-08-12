# TrackHub Common Library

[← Back to the landing page](README.md) · [Español](README.es.md)

TrackHubCommon is the shared foundation every TrackHub backend service builds on. It ships as **four local NuGet packages**, not project references.

| Package | Contents |
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
- A local NuGet feed — the packages are **not** published to nuget.org

### Consuming the packages

Add the package references through your repository's `Directory.Packages.props`, then reference them per project:

```xml
<ItemGroup>
  <PackageReference Include="TrackHubCommon.Domain" />
  <PackageReference Include="TrackHubCommon.Application" />
  <PackageReference Include="TrackHubCommon.Infrastructure" />
  <PackageReference Include="TrackHubCommon.Web" />
</ItemGroup>
```

Register the services in each layer's `DependencyInjection.cs`:

```csharp
// Application layer
services.AddApplicationServices(typeof(SomeHandler).Assembly);
services.AddDistributedMemoryCache();   // required — CachingBehavior resolves IDistributedCache

// Web layer
builder.Services
    .AddTrackHubGraphQLServer<Query, Mutation>(builder.Environment.IsDevelopment());
```

### Building the packages

```bash
dotnet build
```

**Use `dotnet build`, not `dotnet pack`.** The projects set `GeneratePackageOnBuild=true`, so packages are produced during build. `dotnet pack` can package a **stale** DLL, or fail with NU5026 after a clean — it does not reliably recompile.

---

## Repacking after a change

When any `TrackHubCommon.*` project changes — a new constant, a new behavior, a contract change — the packages must be rebuilt and every consumer bumped.

1. **Bump `<Version>`** in `Directory.Build.props`. It is the source of truth, applied in lockstep to all four packages.
2. **`dotnet build`** (see above).
3. **Copy the `.nupkg` files** from each `src/Common.*/bin/Debug/` to the local feed and to `NugetPackages/`.
4. **Purge the global cache** when repacking the *same* version:

   ```bash
   rm -rf ~/.nuget/packages/trackhubcommon.*/<version>
   ```

   Otherwise consumers restore the previously extracted copy and you get confusing `CS0117 'Resources' does not contain …` errors against code you just wrote.
5. **Bump and restore every consumer.**

---

## Project-specific notes

- **Every consumer must move together, and none may be pinned back.** The eight service repositories track the version through their `Directory.Packages.props` — and **the ServiceContracts harness tracks it through a direct `PackageReference`** in `TrackHub.ServiceContracts.Harness.csproj`. It has no `Directory.Packages.props`, so a props-only sweep misses it and the contract suite then fails to restore.
- **`AccountScopeBehavior` is fail-closed; the default `IFeatureFlagService` is fail-open.** That is deliberate: a missing tenant scope is a security failure, while a missing feature registration is a service-configuration failure the service's own tests should catch. A service that uses `[RequireFeature]` **must** register its own `IFeatureFlagService`.
- **`AddDistributedMemoryCache()` is not optional.** `CachingBehavior` resolves `IDistributedCache` for every request type; a missing registration fails **every** request with a masked DI error.
- **Adding an authorization resource is not enough.** It must also be added to `TrackHubSecurity`'s `ApplicationDbContextInitializer` `DefaultResources`, and granted in each role's matrix — [two separate steps](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity#seeding-rules-that-bite).
- **The constant catalogs are the contract.** Resource, action, feature-key, schema and table names are never string literals at a call site — a typo becomes a silent authorization or mapping failure.
- Verifying that a constant landed in a built DLL is best done with `grep -a`; UTF-16 metadata literals defeat plain `strings`.
- Docker image builds pack these packages automatically in a `common` stage, so a deployment does not need a pre-populated feed. A local `dotnet ef` run does.

---

## Documentation

- **Technical** — the [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki): [Common Library](https://github.com/shernandezp/TrackHub/wiki/Common-Library), [Architecture](https://github.com/shernandezp/TrackHub/wiki/Architecture), [Coding Standards](https://github.com/shernandezp/TrackHub/wiki/Coding-Standards)
- **Deployment** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
