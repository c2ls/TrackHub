# TrackHub Reporting

Turns platform data into files. A REST-only service (.NET Minimal APIs) with **no database**: each
of its 30 reports composes its dataset from Manager, Router, Telemetry, Geofencing and Trip
Management, and renders it as an in-portal preview, an Excel export or — where the catalog allows —
a PDF.

Wiki: [Reporting](https://github.com/shernandezp/TrackHub/wiki/Reporting) (the full catalog),
[Manager → Report catalog](https://github.com/shernandezp/TrackHub/wiki/Manager#report-catalog).

## Build, run, test

From this folder:

```bash
dotnet build TrackHub.Reporting.slnx
dotnet test TrackHub.Reporting.slnx

dotnet run --project src/Web          # https://localhost:5012
```

Settings in `src/Web/appsettings.json`: the producer endpoints
`AppSettings:GraphQL{Manager,Router,Telemetry,Geofence,TripManagement}Service`, the service identity
under `AuthorityServer` (`reporting_client`, used to read the account's time zone), and
`AppSettings:Reporting` (`MaxExportRows` 100000, `MaxPdfRows` 500, `PreviewRows` 100).

A report is requested with a bearer token:

```bash
curl -X POST "https://<host>/api/BasicReports" \
  -H "Authorization: Bearer <token>" -H "Content-Type: application/json" \
  -d '{"reportCode":"LiveReport","filters":{"language":"en","values":{}}}' \
  -o live-report.xlsx
```

`"format":"pdf"` asks for a PDF; `POST /api/BasicReports/preview` returns the JSON preview.

## Gotchas

- **The catalog lives in Manager.** Each report has an `app.reports` row (`Category`,
  `RequiredFeatureKey`, `ManagerOnly`, `SupportsPdf`, `SortOrder`, `Active`, filter definitions).
  A new report needs both the `IReport` here and its catalog contribution in Manager's
  `DBInitializer`.
- **Governance is enforced twice.** Manager filters the list the portal shows, and this service
  re-checks the catalog row at execution time, so a hidden report called directly gets 403 or 404.
- **PDF refuses an over-limit dataset; it never truncates.** Only rows flagged `SupportsPdf` offer
  PDF at all.
- **Column headers are resource keys named after the VM property.** Renaming a property means
  renaming its `.resx` keys, or the header falls back to the raw name.
- **Preview, Excel and PDF all render from one `ReportDataset`** per report, so there is no
  per-format data path.
- **The Geofence, Telemetry and Trip Management clients retry; clients default to no retry**,
  because GraphQL is always POST and a retried mutation is a duplicated one. These three are
  query-only.
- **Filter names are a contract with the Manager catalog** (`FilterNames`). A name the catalog row
  does not declare is a filter the portal never renders.
- This service hosts no GraphQL server, so the platform's GraphQL hardening does not apply to it.
