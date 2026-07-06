# TrackHub Telemetry API

## Key Features

- **Latest-Position Projection**: Stores the freshest fix per transporter that backs the live map
- **Position History**: Append-only track store with idempotent batch append and reverse-geocoded address write-back
- **Operator Sync Runs**: One record per device/position sync attempt (counts, result, error)
- **Operator Health Checks**: Connectivity/health probes; the operator health and sync summary is derived from these tables at read time
- **Retention Purge**: Scheduled in-host background job that deletes expired history per account, honoring each account's retention days
- **Group-Scoped Visibility**: Administrator/Manager roles read account-wide; other users are scoped to their group membership
- **Schema-per-Owner**: Owns the `telemetry` schema with read-only, cross-schema access to the `app`-schema scoping tables
- **GraphQL Interface**: Efficient, flexible querying with Hot Chocolate GraphQL server
- **Clean Architecture**: Layered architecture ensuring maintainability and testability

---

## Quick Start

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 14+
- TrackHub Authority Server running (for authentication)

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/shernandezp/TrackHub.Telemetry.git
   cd TrackHub.Telemetry
   ```

2. **Configure the database connection** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHub;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Start the application**:
   ```bash
   dotnet run --project src/Web
   ```

4. **Access GraphQL Playground** at `https://localhost:5001/graphql`

> The telemetry tables live in the `telemetry` schema and are created/migrated by the Manager service; the Telemetry API maps and serves them. In production, connect with a role that has read/write on `telemetry` and read-only on the `app`-schema scoping tables.

---

## Components and Resources

| Component                | Description                                           | Documentation                                                                 |
|--------------------------|-------------------------------------------------------|-------------------------------------------------------------------------------|
| Hot Chocolate            | GraphQL server for .NET                               | [Hot Chocolate Documentation](https://chillicream.com/docs/hotchocolate/v13)  |
| .NET Core                | Development platform for modern applications          | [.NET Core Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview) |
| Postgres                 | Relational database management system                 | [Postgres Documentation](https://www.postgresql.org/)                         |

---

## Overview

The **TrackHub Telemetry API** owns TrackHub's high-volume, append-heavy position and operator-telemetry data. It stores and serves this data; it never talks to GPS providers (that is the Router's role). It adheres to the project's **Clean Architecture** principles, using **GraphQL** for API interactions and **Postgres** for storage.

---

## Entities

### Telemetry (schema `telemetry`, owned)

- **TransporterPosition**: The latest position projection — the freshest fix per transporter.
- **TransporterPositionHistory**: The append-only position track store, deduplicated by an idempotency key.
- **OperatorSyncRun**: One row per device/position sync attempt, with device and position counts, result, and error.
- **OperatorHealthCheck**: Operator connectivity/health probe results.

### Scoping (schema `app`, read-only)

Minimal, read-only projections of the master-data tables the service needs for account scoping and group visibility: users, groups, user–group and transporter–group links, transporters, device–transporter assignments, devices, operators, and account features.

---

## GraphQL Operations

### Queries

- **transporterPositionByOperator**: Latest positions for an operator, scoped to the caller's visibility.
- **positionHistory**: Stored position history, filtered by account/transporter/device.
- **positionHistoryRange**: Replay read over a time range (ordered, point-capped), gated by `gps.positionHistory`.
- **operatorSyncRuns**: Recorded sync-run telemetry.
- **operatorHealth**: Current operator health snapshot, derived from the health-check and sync-run tables.
- **operatorHealthHistory**: Recent health-check records for an operator.
- **operatorHealthSummary**: Aggregated uptime/latency/failure counts over a lookback window.

### Mutations

- **bulkTransporterPosition**: Upsert the latest-position projection (freshest fix per transporter).
- **appendPositionHistory** / **appendPositionHistoryBatch**: Append history rows (idempotent; feature-gated by `gps.positionHistory`).
- **persistResolvedAddress**: Write reverse-geocoded address back onto stored position rows.
- **recordOperatorSyncRun**: Record a sync-run attempt.
- **recordOperatorHealth**: Record an operator health check.
- **purgeExpiredPositionHistory**: Delete history older than a cutoff for an account.

### Why GraphQL?

The use of **GraphQL** enables efficient, customizable queries, letting clients request only the data they need to minimize bandwidth and enhance app performance.

## License

This project is licensed under the Apache 2.0 License. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
