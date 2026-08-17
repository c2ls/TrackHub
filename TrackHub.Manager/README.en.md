# TrackHub Management API

[← Back to the landing page](README.md) · [Español](README.es.md)

The Management API is TrackHub's **master-data service** — the largest backend service in the platform, and the one most other services read from.

Built on .NET 10 with a HotChocolate GraphQL endpoint, following the platform's Clean Architecture and CQRS conventions.

---

## What it owns

| Module | Contents |
|---|---|
| **Accounts** | Accounts and lifecycle status, settings, features, branding, support grants |
| **Identity (portal side)** | Users, user settings, groups, user–group and transporter–group membership |
| **Assets** | Transporters, transporter types, devices, transporter–device assignments |
| **GPS integration** | Operators, encrypted provider credentials, the device-synchronization command surface |
| **Geospatial reference** | Geocoding providers, points of interest |
| **Documents** | Documents, versions, types, signatures, sharing, retention |
| **Workforce** | Drivers, qualifications, driver–transporter assignment history |
| **Alerts & notifications** | Alert events, notification rules, deliveries, templates, subscriptions |
| **Platform** | Announcements, audit events, background job runs, public link grants, the report catalog |

It owns the `app` and `map` schemas, plus the **DDL for the `telemetry` schema** that the Telemetry service serves. High-volume position data itself belongs to Telemetry.

Full detail: **[Manager](https://github.com/shernandezp/TrackHub/wiki/Manager)** in the wiki.

---

## Quick start

### Prerequisites

- .NET 10 SDK
- PostgreSQL 14+ (with PostGIS, since the `TrackHub` database is shared with the Geofencing and TripManagement schemas)
- A running TrackHub AuthorityServer, for authentication

### Steps

1. **Clone**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.git
   cd TrackHub/TrackHub.Manager
   ```

2. **Configure the database connection** in `src/Web/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHub;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Apply migrations** — this creates the `app`, `map` **and `telemetry`** schemas:

   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Web
   ```

4. **Seed initial data** (report catalog, reference data):

   ```bash
   dotnet run --project src/DBInitializer
   ```

5. **Run**

   ```bash
   dotnet run --project src/Web
   ```

6. **Open the GraphQL endpoint** at `https://localhost:<port>/graphql`.

---

## Project-specific notes

- **This service migrates the `telemetry` schema.** The Telemetry service has no migrations of its own and must point at the same `TrackHub` database. Adding a telemetry table means adding a Manager migration.
- **`ApplicationDbContext` is `NoTracking` by default.** A row fetched for mutation must be `Attach`ed, or its changes are silently discarded at `SaveChangesAsync`. EF InMemory defaults to `TrackAll`, so a unit test will not catch the omission.
- **Do not add `Common.Domain.Enums` to the Infrastructure `GlobalUsings`.** Its `TransporterType` collides with the `Infrastructure.Entities` table entity of the same name — import it per file instead.
- **`transporter_position.attributes` is a PostgreSQL `json` column.** `json` has no equality operator, so `Distinct()`, `GroupBy()` or set operations over an entity or projection including it fail at runtime with `42883`. EF InMemory will not catch this.
- **The report catalog is re-seeded on every start.** Code is the source of truth for the seeded metadata — a SuperAdministrator's edits to Description, Category, RequiredFeatureKey, ManagerOnly, SupportsPdf or SortOrder revert on restart. Only `Active` persists. That is intentional.
- **Manager → Router uses a 120 s client timeout**, because the manual-sync dispatch awaits the provider fetch. Every other client is 30 s.
- **The announcements REST endpoint is anonymous and bypasses the mediator** — the pipeline's behaviours all assume a principal. It runs behind 60 s output caching and a per-client-IP rate limit, which is why the service also runs `UseForwardedHeaders`.
- **Localized text never lives in the database.** Notification default texts come from `.resx` resources; account-authored template overrides and announcement text are user content, stored per language.
- After changing any GraphQL surface, run the contract tests:

  ```bash
  dotnet test ../TrackHub.IntegrationTests/TrackHub.IntegrationTests.slnx
  ```

---

## Documentation

- **Technical** — the [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki): [Manager](https://github.com/shernandezp/TrackHub/wiki/Manager), [Architecture](https://github.com/shernandezp/TrackHub/wiki/Architecture), [Database](https://github.com/shernandezp/TrackHub/wiki/Database), [Inter-Service Communication](https://github.com/shernandezp/TrackHub/wiki/Inter-Service-Communication), [Coding Standards](https://github.com/shernandezp/TrackHub/wiki/Coding-Standards)
- **User** — in the app: the Help button or **F1** on any screen
- **Deployment** — [TrackHub.Deployment](../TrackHub.Deployment)

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
