# TrackHub Telemetry

The high-volume store: the latest position per transporter (the live map), the append-only position
history, and the operator sync runs and health checks the Router records. Operator health and sync
summaries are derived from those rows at read time. It never talks to GPS providers — that is the
[Router](../TrackHubRouter).

Wiki: [Telemetry](https://github.com/shernandezp/TrackHub/wiki/Telemetry).

## Build, run, test

From this folder:

```bash
dotnet build TrackHub.Telemetry.slnx
dotnet test TrackHub.Telemetry.slnx

dotnet run --project src/Web          # https://localhost:5008/graphql
```

Settings: `ConnectionStrings:DefaultConnection` in `src/Web/appsettings.json`, pointing at the
**same** `TrackHub` database as Manager, whose migrations create the `telemetry` schema. In
production, connect as a role with read/write on `telemetry` and read-only on the `app` scoping
tables.

## Gotchas

- **No migrations here — do not add any.** The `telemetry` tables are migrated by
  [Manager](../TrackHub.Manager); a new column is a Manager migration.
- **The `app`-schema tables are mapped read-only** so account scoping and group visibility need no
  network hop: users, groups and their memberships, transporters, devices and assignments,
  operators and account features.
- **`attributes` is a `json` column.** `json` has no equality operator, so `Distinct()`,
  `GroupBy()` or set operations over a projection that includes it fail at runtime with `42883`.
  De-duplicate with `EXISTS` or key predicates; EF InMemory will not catch it.
- **`PlatformSyncActivityReader` is deliberately unscoped.** It is a platform-wide read behind
  `[Authorize(Administrative, Read)]` returning timestamps and counts only, for the status page.
- **Feature gating:** latest-position and health writes need authorization only; history writes
  and replay reads are gated by `gps.positionHistory`.
- **Retention is not done here.** Aged history is purged by the database functions db-init installs
  (`CALL ops.run_purge()`), which the operator schedules; see `TrackHub.Deployment/INSTALL.md`.
- After changing the GraphQL surface, run `dotnet test ../TrackHub.IntegrationTests` and then the
  portal's `npm run codegen`.
