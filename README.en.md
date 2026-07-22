# TrackHub Trip Management API

## Key Features

- **Trip Lifecycle Management**: Governed status machine (`Created → InProgress → Paused → Completed / Cancelled / Aborted`) with a single transition matrix as the source of truth
- **Stop and Delivery Planning**: Ordered stops with add/update/remove/reorder operations, arrival and departure progress, skip handling, and per-stop delivery outcomes
- **Route Planning**: Route geometry and corridor buffers computed through OpenRouteService, stored as PostGIS geometries
- **Toll Estimation**: Toll station, tariff, and vehicle-class catalog with route-based cost estimation and explicit partial-coverage reporting
- **Proof of Delivery**: Signature, photo, and document capture linked to the document service with clean-scan enforcement
- **Public Tracking Links**: Anonymous, revocable, rate-limited REST endpoint for sharing a trip's progress with a recipient
- **GraphQL Interface**: Efficient, flexible querying with Hot Chocolate GraphQL server
- **Clean Architecture**: Layered architecture ensuring maintainability and testability
- **PostgreSQL + PostGIS**: Enterprise-grade spatial database capabilities using NetTopologySuite geometry (SRID 4326)

---


## Quick Start

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 14+ with PostGIS extension enabled
- TrackHub Authority Server running (for authentication)
- TrackHub Manager and Telemetry APIs reachable (master data, positions, alerts, public link grants)
- An OpenRouteService API key (for route planning)

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/shernandezp/TrackHub.TripManagement.git
   cd TrackHub.TripManagement
   ```

2. **Enable PostGIS extension** in PostgreSQL:
   ```sql
   CREATE EXTENSION postgis;
   ```

3. **Configure the database connection** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;user id=postgres;password=yourpassword;database=TrackHub;port=5432"
     }
   }
   ```

4. **Configure the routing provider** in `appsettings.json`:
   ```json
   {
     "AppSettings": {
       "Routing": {
         "Provider": "OpenRouteService",
         "BaseUrl": "https://api.openrouteservice.org",
         "ApiKey": "your-api-key",
         "Profile": "driving-hgv"
       }
     }
   }
   ```

5. **Run database migrations**:
   ```bash
   dotnet ef database update
   ```

6. **Start the application**:
   ```bash
   dotnet run --project src/Web
   ```

7. **Access GraphQL Playground** at `https://localhost:5006/graphql`

---

## Components and Resources

| Component                | Description                                           | Documentation                                                                 |
|--------------------------|-------------------------------------------------------|-------------------------------------------------------------------------------|
| Hot Chocolate            | GraphQL server for .NET                               | [Hot Chocolate Documentation](https://chillicream.com/docs/hotchocolate/v13)  |
| .NET Core                | Development platform for modern applications          | [.NET Core Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview) |
| Postgres                 | Relational database management system                 | [Postgres Documentation](https://www.postgresql.org/)                         |
| OpenRouteService         | Routing provider used for route geometry and ETAs     | [OpenRouteService Documentation](https://openrouteservice.org/dev/#/api-docs) |

---

## Overview

The **TrackHub Trip Management API** provides services for planning, dispatching, and tracking trips. It adheres to the project's **Clean Architecture** principles, leveraging **GraphQL** for API interactions and **Postgres** for database management. Its tables live in the `trip` schema of the shared `TrackHub` database and its capabilities are gated by the `trip-management` account feature.

### Key Features

The API offers the following functionalities:
- Planning trips with ordered stops, deliveries, and driver/transporter assignments.
- Route planning and corridor generation through an external routing provider.
- Toll catalog administration and route-based toll cost estimation.
- Detecting stop arrivals, departures, delays, and route deviations from telemetry positions.
- Capturing proof of delivery and sharing read-only public tracking links.
- Supplying the datasets behind the trip reports served by the Reporting API.

---

## Entities

### Trip Management

- **Trip**: The dispatch unit, carrying its code, external reference, schedule, status, and the account it belongs to.
- **TripStop**: An ordered stop on a trip with its location, planned window, ETA, and progression status (`Pending`, `Arrived`, `Departed`, `Skipped`).
- **Delivery**: A delivery attached to a stop, with its outcome (`Pending`, `Delivered`, `PartiallyDelivered`, `Rejected`).
- **ProofOfDelivery**: The signature, photo, or document evidence recorded when a delivery is closed out.
- **TripAssignment**: Links a trip to the driver and transporter executing it, tracked as `Active`, `Ended`, or `Cancelled`.
- **RoutePlan**: The planned route geometry and corridor buffer for a trip, produced by a provider (`OpenRouteService` or `Manual`) and stored as PostGIS geometry.
- **TripEvent**: The append-only log of everything that happened on a trip, recording the source (`Portal`, `Driver`, `Detection`, `Job`, `ServiceClient`).
- **TripShare**: A revocable public tracking link for a trip, backed by a public link grant in the Management API.
- **TripDocument**: A document attached to a trip or delivery (signature, photo, manifest, bill of lading, receipt).
- **TollStation**, **TollTariff**, **TollVehicleClass**, **TransporterTollClass**: The toll catalog and the per-transporter class assignment used for cost estimation.
- **VwUser**, **VwVisibleTransporter**: Views used to scope trip data to the calling user's account and groups.

---

## GraphQL Operations

### Mutations

- **createTrip**, **updateTrip**, **deleteTrip**: Trip CRUD.
- **assignTrip**: Assigns a driver and transporter to a trip.
- **planTripRoute**: Requests a route plan from the routing provider and stores its geometry and corridor.
- **startTrip**, **pauseTrip**, **resumeTrip**, **completeTrip**, **cancelTrip**, **abortTrip**: Lifecycle transitions, validated against the transition matrix.
- **addTripStop**, **updateTripStop**, **removeTripStop**, **reorderTripStops**: Stop planning.
- **recordStopArrival**, **recordStopDeparture**, **skipStop**: Stop progression.
- **createDelivery**, **updateDelivery**, **updateDeliveryOutcome**, **deleteDelivery**: Delivery management.
- **recordProofOfDelivery**: Records the proof-of-delivery evidence for a delivery.
- **shareTrip**, **revokeTripShare**: Issues and revokes public tracking links.
- **processTripPositions**: Processes transporter positions to detect arrivals, departures, delays, and corridor deviations.
- **importTrips**, **updateTripStatus**: Service-client integration entry points for external dispatch systems.
- **createTollVehicleClass**, **updateTollVehicleClass**, **deactivateTollVehicleClass**, **createTollStation**, **updateTollStation**, **deactivateTollStation**, **createTollTariff**, **updateTollTariff**, **deleteTollTariff**, **importTollCatalog**, **setTransporterTollClass**: Toll catalog administration.

### Queries

- **trips**: Paged trip list for the calling user's account, with filtering.
- **tripDetail**: A single trip with its stops, deliveries, assignment, and route plan.
- **activeTrips**: Trips currently in progress, for the live map.
- **tripTimeline**: Paged event log for a trip.
- **tripRouteReplay**: Planned route and recorded positions for replay.
- **tripReportData**, **tripStopReportData**, **tripTollReportData**, **tripPodReportData**: Paged report datasets consumed by the Reporting API.
- **tollStations**, **tollStationDetail**, **tollVehicleClasses**, **transporterTollClasses**: Toll catalog reads.
- **estimateTolls**: Estimates toll cost over a route for a given vehicle class.

### REST Endpoints

- **GET `~/public/trips/{publicLinkGrantId}`**: Anonymous public tracking endpoint. Rate limited per client IP and deliberately uncached so revocations take effect immediately and every resolution is audited.
- **GET `/health`**: Health probe, including a database context check.

---

## Background Services

| Service                      | Job Key                  | Interval   | Purpose                                                                 |
|------------------------------|--------------------------|------------|-------------------------------------------------------------------------|
| `TripEtaRefreshService`      | `trip-eta-refresh`       | 5 minutes  | Recomputes stop ETAs for in-progress trips and raises delay events       |
| `TripScheduleReminderService`| `trip-schedule-reminder` | 15 minutes | Flags trips whose scheduled start is due but that have not started yet   |

Both jobs record a run only when they did work, so an old run record for these keys is the healthy steady state rather than a stuck job.

---

## Configuration

| Key                                        | Purpose                                                      |
|--------------------------------------------|--------------------------------------------------------------|
| `ConnectionStrings:DefaultConnection`      | PostgreSQL connection for the `trip` schema                  |
| `AuthorityServer:ClientId` (`trip_client`) | Service client used for service-to-service calls             |
| `AppSettings:GraphQLIdentityService`       | Security API GraphQL endpoint                                |
| `AppSettings:GraphQLManagerService`        | Management API GraphQL endpoint                              |
| `AppSettings:GraphQLTelemetryService`      | Telemetry API GraphQL endpoint                               |
| `AppSettings:Routing`                      | Routing provider settings (provider, base URL, API key, profile, rate limit, timeout, max waypoints) |
| `AllowedCorsOrigins`                       | Origins allowed to call the API from the browser             |

The portal reaches this service through `REACT_APP_TRIPMANAGEMENT_ENDPOINT`; other backend services reach it through `AppSettings:GraphQLTripManagementService`. In development the service listens on `https://localhost:5006` and `http://localhost:5007`.

### Why GraphQL?

The use of **GraphQL** enables efficient, customizable queries, letting clients request only the data they need to minimize bandwidth and enhance app performance. With GraphQL, applications can retrieve specific details about trips, stops, deliveries, or tolls, optimizing both operational efficiency and user experience.

## License

This project is licensed under the Apache 2.0 License. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
