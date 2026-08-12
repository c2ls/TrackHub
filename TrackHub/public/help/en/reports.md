---
id: reports
title: Reports
description: Pick a report from the catalog, set its filters, then preview it on screen or export to Excel or PDF.
category: operation
screens: [reports]
related: [dashboard-trips-replay, units-devices]
tags: [reports, export, excel, pdf, preview, filters]
order: 40
---

# Reports

Reports turn your fleet data — positions, geofence activity, GPS integration health, documents, and account administration — into a table you can preview on screen or download. Which reports you see, and the data inside them, depend on your account features, role, and groups, so you may see fewer than the full catalog. See [Roles and permissions](topic:roles-and-permissions).

## Opening Reports

Open **Reports** from the left-hand menu. The screen has the report catalog on the left and, once you pick a report, its filters (and any preview) on the right.

## Choosing a report

The catalog groups reports into collapsible categories — **Operations**, **GPS**, **Documents**, and **Administration**. The first category is expanded for you; click a category heading to expand or collapse it.

Each report is a card showing its name, a one-line description, and format badges: every report offers **XLSX** (Excel), and reports that also support PDF show a **PDF** badge. Click a report card to select it; its filter panel opens on the right. Nothing is selected until you click a report.

## The report catalog

The reports TrackHub can offer include the following. The exact list is driven by your account and permissions.

### Operations

- **Units Report** – latest known position of every unit in your account.
- **Position Record** – historical position records for a unit over a date range.
- **Units in Geofence** – units currently located inside a geofence.
- **Geofence Events** – geofence entry and exit events for a unit over a date range.

### GPS

- **GPS Provider Health Summary** – uptime, latency, and failure summary per GPS provider.
- **GPS Provider Sync History** – per-run synchronization history for GPS providers.
- **GPS Sync Statistics** – daily synchronization statistics aggregated by provider.
- **GPS Synchronized Device Inventory** – full inventory of synchronized GPS devices and their assignment.
- **GPS Recently Added Devices** – GPS devices first detected within the selected window.
- **GPS Unassigned Devices** – synchronized GPS devices not yet assigned to a unit.
- **GPS Ignored Devices** – synchronized GPS devices marked as ignored.
- **GPS Assignment History** – history of device-to-unit assignments over a date range.
- **GPS Latest Position Freshness** – how recently each device last reported a position.
- **GPS Position History** – detailed position history for a unit or device.

### Documents

- **Expiring Documents** – active documents expiring within the selected number of days.
- **Missing Required Documents** – units missing one or more required document types.
- **Document Share Activity** – public-link share activity and access counts for documents.
- **Document Upload Volume** – document upload counts by category over a date range.

### Administration

- **Accounts by Status** – accounts grouped by lifecycle status.
- **Feature Enablement Matrix** – enabled features and tiers across all accounts.
- **Group Membership Export** – group membership of users and units for your account.

## Filling in the filters

The filter panel shows only the fields a report actually needs; a report that needs nothing shows just the action buttons. Possible fields are:

- **Unit** – restricts the report to a single unit (a picker).
- **Operator** – restricts the report to one GPS operator (a picker).
- **Device ID** – a device identifier, entered as free text.
- **Status** – an account lifecycle status (used by the Accounts by Status report).
- **From** / **To** – the start and end date-and-time of the reporting period.
- **Max Rows** – a cap on how many rows to return.
- **Within Days** – a rolling window in days (for "recently" or "expiring" reports).
- **Lookback Hours** – how many hours back to summarise.

Reports without a defined filter set default to a **From** / **To** date range.

## Preview, Excel, and PDF

The filter panel has up to three action buttons:

- **Preview** – runs the report and shows the results in a table on the right, with a **Total rows** count. If the preview is capped, a notice tells you it is limited to the first rows shown — narrow your filters or export to Excel for the full dataset.
- **Export Excel** – downloads the report as an Excel (`.xlsx`) file named after the report.
- **Export PDF** – downloads a formatted PDF. This button appears only for reports that support PDF (the ones with a **PDF** badge in the catalog).

Open Excel files in Excel, Google Sheets, or any spreadsheet app. If an export would be very large, tighten the date range or the unit selection and run it again — exports are subject to a server-side row limit from your plan.
