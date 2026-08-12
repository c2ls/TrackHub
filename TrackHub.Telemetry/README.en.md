# TrackHub Telemetry API

[← Back to the landing page](README.md) · [Español](README.es.md)

The Telemetry API owns TrackHub's **high-volume, append-heavy** data. It stores and serves positions and operator telemetry; it never talks to GPS providers — that is the [Router](https://github.com/shernandezp/TrackHubRouter)'s role.

Built on .NET 10 with a HotChocolate GraphQL endpoint, following the platform's Clean Architecture and CQRS conventions.

---

## What it owns

| Table | Purpose |
|---|---|
| `telemetry.transporter_position` | The latest-position projection — the freshest fix per transporter, backing the live map |
| `telemetry.transporter_position_history` | The append-only track store, deduplicated by an idempotency key |
| `telemetry.operator_sync_runs` | One row per device or position sync attempt: counts, result, error |
| `telemetry.operator_health_checks` | Operator connectivity and health probe results |

Operator health and sync summaries are **derived at read time** from the last two tables — the operator row carries no rollup columns.

A scheduled in-host job (`PositionRetentionPurgeService`) deletes expired history per account, honouring each account's retention days.

Full detail: **[Telemetry](https://github.com/shernandezp/TrackHub/wiki/Telemetry)** in the wiki.

---

## Quick start

### Prerequisites

- .NET 10 SDK
- PostgreSQL 14+
- The `TrackHub` database with the `telemetry` schema **already created by the Manager migrations**
- A running TrackHub AuthorityServer, for authentication
- The `TrackHubCommon.*` packages available from a local NuGet feed

### Steps

1. **Clone**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.Telemetry.git
   cd TrackHub.Telemetry
   ```

2. **Configure the database connection** in `src/Web/appsettings.json` — it must point at the **same** `TrackHub` database as the Manager:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHub;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Run**

   ```bash
   dotnet run --project src/Web
   ```

4. **Open the GraphQL endpoint** at `https://localhost:<port>/graphql`.

In production, connect with a role that has read/write on `telemetry` and **read-only** on the `app`-schema scoping tables.

---

## Project-specific notes

- **This service has no migrations of its own — do not add any.** The `telemetry` tables are created and migrated by the [Management API](https://github.com/shernandezp/TrackHub.Manager). That is why `DB_CONNECTION_TELEMETRY` must point at the same `TrackHub` database. Adding a telemetry column means adding a *Manager* migration.
- **The `app`-schema tables are mapped read-only** and excluded from migrations. They exist so the service can enforce account scoping and group visibility without a network hop per request: users, groups, user–group and transporter–group links, transporters, device assignments, devices, operators and account features.
- **`attributes` is a PostgreSQL `json` column.** `json` has no equality operator, so `Distinct()`, `GroupBy()` or set operations over an entity or projection including it fail at runtime with `42883`. De-duplicate with `EXISTS` or key-based predicates — **EF InMemory will not catch this.**
- **`PlatformSyncActivityReader` is deliberately unscoped.** It is a documented platform-wide read gated by `[Authorize(Administrative, Read)]`, returning timestamps and counts only, never an account id. It backs the public status page's SyncWorker tile.
- **Feature gating**: latest-position and health writes are core (authorization only); history writes and replay reads are gated by `gps.positionHistory`.
- **Visibility**: Administrator and Manager roles read account-wide; other users are scoped to their group membership. Service clients read on behalf of already-authorized users.
- The retention purge is an **on-work-only recorder** — an old `BackgroundJobRun` timestamp for it is the healthy steady state, not a stuck job.
- After changing any GraphQL surface, run the contract tests:

  ```bash
  dotnet test ../TrackHub.IntegrationTests/TrackHub.IntegrationTests.slnx
  ```

---

## Documentation

- **Technical** — the [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki): [Telemetry](https://github.com/shernandezp/TrackHub/wiki/Telemetry), [Database](https://github.com/shernandezp/TrackHub/wiki/Database), [Router](https://github.com/shernandezp/TrackHub/wiki/Router), [Architecture](https://github.com/shernandezp/TrackHub/wiki/Architecture)
- **User** — in the app: the Help button or **F1** on any screen
- **Deployment** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
