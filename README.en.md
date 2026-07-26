# TrackHub Reporting API

[← Back to the landing page](README.md) · [Español](README.es.md)

The Reporting API turns platform data into files. It is **REST-only** (.NET 10 Minimal APIs) and has **no database of its own** — every dataset is composed from the services that own the data.

---

## What it does

- Serves a **governed catalog of 30 reports** across six categories: Operations, GPS, Documents, Workforce, Trips and Administration
- Renders each report as an in-portal **preview**, an **Excel** export, or — where the catalog allows it — a **PDF**
- Filters every report by the caller's account features, role and group visibility
- Composes its datasets from the Management, Router, Telemetry, Geofencing and Trip Management APIs

Full detail, including the complete catalog: **[Reporting](https://github.com/shernandezp/TrackHub/wiki/Reporting)** in the wiki.

---

## Quick start

### Prerequisites

- .NET 10 SDK
- A running TrackHub AuthorityServer and Management API (the catalog lives there)
- The other producer services reachable for the reports you intend to run
- The `TrackHubCommon.*` packages available from a local NuGet feed

### Steps

1. **Clone**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.Reporting.git
   cd TrackHub.Reporting
   ```

2. **Configure the producer endpoints and limits** in `src/Web/appsettings.json`:

   ```json
   {
     "AppSettings": {
       "GraphQLManagerService": "https://localhost:5001/graphql",
       "GraphQLRouterService": "https://localhost:5003/graphql",
       "GraphQLTelemetryService": "https://localhost:5011/graphql",
       "GraphQLGeofenceService": "https://localhost:5004/graphql",
       "GraphQLTripManagementService": "https://localhost:5006/graphql",
       "Reporting": {
         "MaxExportRows": 100000,
         "MaxPdfRows": 500,
         "PreviewRows": 100
       }
     }
   }
   ```

3. **Run**

   ```bash
   dotnet run --project src/Web
   ```

4. **Call a report** with a bearer token:

   ```bash
   curl -X GET "https://localhost:<port>/api/BasicReports/live-report" \
     -H "Authorization: Bearer {your_token}" \
     -o live_report.xlsx
   ```

---

## Project-specific notes

- **The catalog lives in the Management API, not here.** Each report has an `app.reports` row carrying `Category`, `RequiredFeatureKey`, `ManagerOnly`, `SupportsPdf`, `SortOrder` and `Active`. Adding a report means adding **both** the `IReport` implementation here and the seeded catalog row there.
- **Governance is enforced twice.** Manager's `getReports` filters the list the portal shows; this service re-enforces the same metadata at execution time via `reportByCode` (cached 60 s). A hidden report invoked directly by code gets 403 or 404 — the portal filter is convenience, not the control.
- **The catalog is re-seeded on every Manager start**, so seeded metadata edits made through the admin UI revert. Only `Active` persists. That is intentional.
- **PDF refuses an over-limit dataset; it never truncates.** `MaxPdfRows` defaults to 500, and only catalog rows flagged `SupportsPdf` offer PDF at all. Excel's ceiling is `MaxExportRows` (100 000).
- **Column headers are resolved by VM property name** through the `Resources` ResourceManager — **renaming a VM property requires renaming the matching resx keys**, or the header falls back to the raw property name.
- **Preview, Excel and PDF all render from one shared `ReportDataset`** (each `IReport` supplies `GetDatasetAsync`), so there is no per-format data path to drift.
- **This service's Router, Geofence and Telemetry clients are registered `WithRetry`** — they are query-only, so a retry is safe. Every other client on the platform defaults to `NoRetry`, because GraphQL is always POST and a retried mutation is a duplicated mutation.
- Paged upstream feeds are drained at 500 rows per page with a 100 000-row defensive cap.
- **This service hosts no GraphQL server**, so the platform's GraphQL hardening (max execution depth, dev-only error detail) does not apply to it.
- PDF exports fetch account branding from Manager (60 s cache, failure-tolerant — a branding error never fails an export). The account name is rendered; logo bytes are not embedded yet.

---

## Documentation

- **Technical** — the [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki): [Reporting](https://github.com/shernandezp/TrackHub/wiki/Reporting), [Manager](https://github.com/shernandezp/TrackHub/wiki/Manager#report-catalog), [Inter-Service Communication](https://github.com/shernandezp/TrackHub/wiki/Inter-Service-Communication)
- **User** — in the app: the Help button or **F1** on any screen
- **Deployment** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
