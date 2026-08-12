# Adding a GPS provider to the Router

This guide is for developers integrating a new GPS tracking platform ("provider") into
TrackHub. A provider is a self-contained class library inside the Router solution that
translates one external API into the Router's uniform contracts. Providers are **compiled in,
activated by configuration**: every provider project ships with the Router, and
`AppSettings:Protocols` decides which ones register at startup. There is no runtime plugin
loading — a misconfigured or missing provider fails **at startup**, loudly, on purpose.

Existing providers to crib from: `Traccar` (simplest — static Basic-auth header),
`Samsara`/`Flespi`/`GpsGate` (static token header), `Wialon`/`Navixy` (login → session id,
cached), `Geotab` (vendor SDK, cached session), `CommandTrack`/`Protack` (bearer token with
absolute expiry, persisted back to Manager). **Traccar is the recommended template.**

---

## 1. The contract

A provider implements up to three interfaces (`TrackHub.Router.Domain.Interfaces`), each in its
own class with an **exact, convention-bound name**:

| Class name | Interface | Used by |
|---|---|---|
| `DeviceReader` | `IExternalDeviceReader` | Device-catalog sync (SyncWorker + manual "sync now") |
| `PositionReader` | `IPositionReader` | Position sync loop + on-demand position queries |
| `ConnectivityTester` | `IConnectivityTester` | 1-minute health PING loop + operator "ping" button |

Registering at least one of the three is enough to pass startup validation, but a production
provider should implement all three.

Shared semantics:

- **`Protocol` property** — return your `ProtocolType` enum value. The registries resolve
  readers keyed by this value; a mismatch with the enum/config spelling is caught at startup.
- **`Init(CredentialTokenDto credential, CancellationToken ct)`** — called before *every* batch
  of calls (each sync cycle, each query, each ping). It receives the operator's **decrypted**
  credential (`Uri`, `Username`, `Password`, optional `Key`/`Key2`/`Token` + expirations —
  which fields carry what is a per-provider convention you define). Init must:
  - build the `HttpClient` through `ICredentialHttpClientFactory` (never `new HttpClient()` —
    the factory applies the hardened named client: auto-redirect disabled, 30 s timeout) and
    hand it to `IHttpClientService.Init`;
  - **be cheap on repeat calls** — if your provider requires a login round-trip, cache the
    session artifact in `IProviderSessionStore` (see §3). Init runs for every operator on every
    cycle; an unconditional login is the difference between N cheap in-memory hits and N
    provider logins per minute at fleet scale.
- **Readers are keyed-transient and stateless between operators.** A new reader instance is
  constructed per lookup, so instance fields set in `Init` are safe; anything shared across
  instances (the session store) must be thread-safe.
- **Errors must throw, never degrade to empty results.** If your provider reports failures as
  HTTP 200 with an error payload (Wialon, Navixy do), detect it and throw
  `InvalidOperationException` with the provider error code. A swallowed error becomes a
  "successful" sync of 0 positions — the worst failure mode in this pipeline, because it
  silently stops position flow while Health stays green. The sync pipeline converts thrown
  exceptions into a FAILED sync run + `GpsOperatorPositionSyncFailed` alert + operator backoff.

### `IPositionReader`

```csharp
Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm device, CancellationToken ct);
Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken ct);
Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm device, CancellationToken ct);
```

- The plural overload is the sync-loop hot path — prefer ONE batched provider call over
  per-device calls whenever the API allows it.
- The `from`/`to` overload serves position history; timestamps are UTC `DateTimeOffset`
  everywhere (SVD-06). Convert at the provider boundary only.
- **Units contract** for `PositionVm`: speed in **km/h**, mileage in km, temperature in °C.
  Verify what the provider actually emits against a live instance — do not guess conversions
  (see RA-04: Traccar likely emits knots; unverified conversions are worse than documented raw
  values).
- **Attributes**: the five promoted fields (ignition/satellites/mileage/hourmeter/temperature)
  are strongly typed on `AttributesVm`; every other provider signal goes in the open `Extra`
  bag built with `Domain.Helpers.AttributesExtra`. Never add a cross-service schema field for
  a provider-specific signal. Attribute naming on the wire is TrackHub's, not the provider's —
  remap vendor spellings at your boundary (e.g. CommandTrack's `hobbsMeter` →
  `[JsonPropertyName("hobbsMeter")]` on a property named `Hourmeter`).

### `IExternalDeviceReader`

```csharp
Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm device, CancellationToken ct);
Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken ct);
Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken ct);
```

The parameterless overload returns the provider's full device catalog; it feeds the
device-sync loop that reconciles Manager's catalog (guarded per-operator by
`IOperatorSyncLock`, cached by `IDeviceCatalogCache`).

### `IConnectivityTester`

```csharp
Task Ping(CredentialTokenDto credential, CancellationToken ct);
```

`Ping` **must perform a real provider round-trip** — `Init` + one minimal authenticated call
(list one device, fetch server info). With session caching, `Init` alone may be a pure
in-memory hit and would turn the health monitor into a no-op that reports HEALTHY while the
provider is down. Success/failure of every ping and device-sync attempt is recorded as a
health observation (`DEVICE_SYNC` HEALTHY/OFFLINE) that drives the operator Health status in
the portal.

---

## 2. The four alignment points

Registration (`ProtocolRegistrationExtensions`, called from `AddCommonContext`) resolves
everything by convention. Four names must line up (case-insensitively):

| # | Where | What |
|---|---|---|
| 1 | `TrackHubCommon` → `Common.Domain/Enums/ProtocolType.cs` | The enum value (e.g. `Wialon = 8`) |
| 2 | Provider `.csproj` | `AssemblyName` + `RootNamespace` = `TrackHub.Router.Infrastructure.{Protocol}` |
| 3 | Reader classes | `Protocol` property returns the enum value; class names exactly `DeviceReader` / `PositionReader` / `ConnectivityTester` in the root namespace |
| 4 | `AppSettings:Protocols` (Web + SyncWorker `appsettings.json`) | The enum spelling listed |

Any mismatch fails at startup with a message naming the protocol: unknown enum value,
missing assembly, or an assembly with no matching reader types. An operator whose
`ProtocolTypeId` has no registered reader gets `ProtocolNotSupportedException` (naming the
protocol) instead of a masked error.

> Casing may drift between config, enum, and namespace (`GeoTab` ↔ `Geotab`) — resolution is
> case-insensitive — but keep new providers consistent; the folder name is the only place
> history allows divergence (`Protack` folder builds the `Protrack` assembly).

`Mettax = 10` is **reserved** in the enum with no provider assembly; configuring it throws at
startup until someone builds the project.

---

## 3. Session & token caching (`IProviderSessionStore`)

The singleton `IProviderSessionStore` (registered in `AddCommonContext`) keeps one session
artifact per credential so repeated `Init` calls don't re-authenticate:

```csharp
bool TryGet(CredentialTokenDto credential, out string session);
void Set(CredentialTokenDto credential, string session, TimeSpan timeToLive, bool sliding = true);
void Invalidate(Guid credentialId);
```

Entries are fingerprinted by the **full credential value** — a rotated password/token is
automatically a cache miss, never a stale session. Pick the pattern that matches your
provider's auth model:

| Auth model | Pattern | Example |
|---|---|---|
| Static header (Basic auth, permanent API token) | No caching needed — `Init` is already free | Traccar, Samsara, Flespi, GpsGate |
| Login → inactivity-timeout session | `TryGet` in `Init`; on miss login + `Set` with a **sliding** TTL below the provider's inactivity timeout; on an invalid-session error mid-call: `Invalidate` + re-login + **retry once**, then throw | Wialon (sid, 4 min sliding), Navixy (hash, 30 min sliding) |
| Vendor SDK with session id | Cache the SDK session id; on a hit construct the SDK client with password **and** session id so the SDK self-heals an expired session; persist the current id after successful calls | Geotab |
| Bearer token with absolute expiry | Durable copy already lives on the Manager credential (`ICredentialWriter.UpdateTokenAsync`); `Set` with a **non-sliding** TTL of `expiry − now − margin` as an in-process fast path | CommandTrack, Protack |

Rules regardless of pattern:

- The retry-once-on-invalid-session lives in the provider base class (`WialonReaderBase.PostAsync`,
  `NavixyReaderBase.PostNavixyAsync`) so readers stay oblivious. Copy that shape.
- Never let a cached session weaken error semantics: a second failure after re-login must
  throw, and `Ping` must still round-trip (§1).
- The store is in-process, matching the single-instance SyncWorker deployment (same as
  `IOperatorSyncLock` / `ExecutionIntervalManager`). If the worker is ever scaled out, session
  caching degrades gracefully (each instance logs in once) — no correctness issue.

---

## 4. Step-by-step

1. **Enum value** — add `{Protocol} = N` to `ProtocolType` in **TrackHubCommon**
   (`src/Common.Domain/Enums/ProtocolType.cs`), then repack/bump the Common packages and
   update consumers per the repack workflow (`dotnet build`, purge the global package cache,
   bump `Directory.Packages.props` in consuming repos). This is additive and non-breaking.
2. **Project** — create `src/Infrastructure/{Protocol}/{Protocol}.csproj`:

   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <RootNamespace>TrackHub.Router.Infrastructure.{Protocol}</RootNamespace>
       <AssemblyName>TrackHub.Router.Infrastructure.{Protocol}</AssemblyName>
     </PropertyGroup>
     <ItemGroup>
       <ProjectReference Include="..\..\Domain\Domain.csproj" />
     </ItemGroup>
   </Project>
   ```

   (TargetFramework/nullable/warnings-as-errors come from `Directory.Build.props`.)
3. **Implement** — `{Protocol}ReaderBase` (Init + auth + session caching + shared HTTP
   helpers), `DeviceReader`, `PositionReader`, `ConnectivityTester`, `Models/` (provider DTOs,
   `internal`), `Mappers/` (static extension methods to `DeviceVm`/`PositionVm` — mapping is
   manual, no AutoMapper).
4. **Wire the build** — add the project to `TrackHub.Router.slnx` and a `ProjectReference` in
   `src/Infrastructure/Common/Common.csproj` (that reference is what puts your DLL in the
   Web/SyncWorker output).
5. **Activate** — add the protocol name to `AppSettings:Protocols` in **both**
   `src/Web/appsettings.json` and `src/SyncWorker/appsettings.json` (deployed environments:
   `AppSettings__Protocols__N` env vars, see `TrackHub.Deployment`).
6. **Portal** — add `{ value: N, label: '{PROTOCOL}' }` to `TrackHub/src/data/protocolTypes.ts`
   (values mirror the enum — this list feeds the operator dialog's protocol select).
7. **Tests** — add `{Protocol}DeviceReaderTests` / `{Protocol}PositionReaderTests` in
   `tests/Intfrastructure.UnitTests` deriving from `DeviceReaderTestsBase<T>` /
   `PositionReaderTestsBase<T>` (mocks for `ICredentialHttpClientFactory`,
   `IHttpClientService`, `IProviderSessionStore` are provided). Cover: happy-path mapping,
   empty results, provider-error payloads (must throw), and — if you cache sessions — the
   re-login-and-retry path.
8. **Verify** — `dotnet build TrackHub.Router.slnx && dotnet test TrackHub.Router.slnx`, then
   start the Web project: startup throws if any alignment point is off. Finally create an
   operator with the new protocol and use **Ping** + **Sync now** in the portal against a live
   account.

No Security/Manager permission work is needed — provider calls go outward with the operator's
credential, not through the service-client grant system.

---

## 5. Runtime behavior you inherit (nothing to build)

- **Concurrency**: accounts sync concurrently under the global operator gate
  (`AppSettings:MaxConcurrentOperatorSyncs`, default 10). Two same-protocol operators in one
  scope get separate reader + `HttpClientService` instances.
- **Failure handling**: a thrown provider error → FAILED sync run (with your message) +
  `GpsOperatorPositionSyncFailed` alert + exponential backoff (1→30 min) until first success.
- **Device catalog cache**: 60 s (`AppSettings:DeviceCatalogCacheSeconds`), invalidated by
  device sync — your `GetDevicesAsync` is not called every position cycle.
- **Health**: every ping/device-sync attempt records a HEALTHY/OFFLINE observation.
- **Rate limiting/auth**: manual sync and ping are authorized + rate-limited at the
  command layer (`Resources.Credentials`/`Actions.Custom`); your reader never checks callers.
- **HTTP hardening**: the shared named client disables auto-redirect (an operator-configured
  base URL cannot 302 the Router inward) and enforces the 30 s timeout.

## 6. Documentation & context upkeep

When the provider lands, update `TrackHub.wiki/Router.md` (supported protocols table) and
`system-context/architecture.md` if you introduced a new pattern; regenerate the machine
catalogs (`tools/Tools.McpExtractor`) per `system-context` maintenance.
