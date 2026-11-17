# TrackHub Manager

The master-data service, and the one most other services read from: accounts and their features,
settings and branding; users and groups; transporters, devices and their assignments; GPS
operators and their encrypted credentials; geocoding providers and points of interest; documents;
staff and workforce; alert events, notification rules and deliveries; announcements, audit events,
background job runs, public links and the report catalog. It owns the `app` and `map` schemas of the
`TrackHub` database **and the DDL of the `telemetry` schema** that [Telemetry](../TrackHub.Telemetry)
serves.

Wiki: [Manager](https://github.com/shernandezp/TrackHub/wiki/Manager),
[Database](https://github.com/shernandezp/TrackHub/wiki/Database).

## Build, run, test

From this folder:

```bash
dotnet build TrackHub.Manager.slnx
dotnet test TrackHub.Manager.slnx

# creates app, map and telemetry
dotnet ef database update --project src/Infrastructure/ManagerDB --startup-project src/Web

# report catalog, transporter types, the master account and administrator
dotnet run --project src/DBInitializer

dotnet run --project src/Web          # https://localhost:5004/graphql
```

Settings: `ConnectionStrings:DefaultConnection` in `src/Web/appsettings.json` and
`src/DBInitializer/appsettings.json`. The `TrackHub` database needs the `postgis` extension once
Geofencing or TripManagement migrate into it.

## Gotchas

- **Telemetry tables are migrated here.** Adding a `telemetry` column means adding a Manager
  migration; Telemetry has none of its own.
- **`ApplicationDbContext` is `NoTracking` by default.** A row fetched for mutation must be attached
  (or queried `AsTracking()`), or `SaveChangesAsync` silently discards the change. EF InMemory
  defaults to tracking, so a unit test only catches this when its context mirrors production.
- **Do not add `Common.Domain.Enums` to the Infrastructure `GlobalUsings`.** Its `TransporterType`
  collides with the table entity of the same name; import it per file.
- **`transporter_position.attributes` is a `json` column.** `json` has no equality operator, so
  `Distinct()`, `GroupBy()` or set operations over a projection that includes it fail at runtime
  with `42883`. EF InMemory will not catch it.
- **The report catalog is re-seeded on every `DBInitializer` run** from `IReportCatalogContribution`
  files. Code owns the seeded metadata; only `Active` survives an admin edit.
- **Manager → Router uses a 120 s client timeout** because manual sync awaits the provider fetch;
  every other client uses 30 s.
- **The announcements REST endpoint is anonymous and bypasses the mediator.** It is output-cached
  (60 s) and rate-limited per client IP, which is why the service trusts forwarded headers from the
  proxy.
- **Localized text never lives in the database.** Notification defaults come from `.resx`; only
  user-authored templates and announcements are stored, per language.
- After changing the GraphQL surface, run `dotnet test ../TrackHub.IntegrationTests` and then the
  portal's `npm run codegen`.
