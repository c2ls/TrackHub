# TrackHub Telemetry API

TrackHub is an innovative open-source application designed to unify multiple monitoring platforms into a cohesive system. Imagine having all your monitoring needs met in one place—this is the vision behind TrackHub.

Currently in development, our project aims to foster collaboration among diverse companies and developers, promoting continuous improvement and growth. TrackHub empowers organizations to centralize information about their assets and personnel, regardless of their vendors.

We believe in the strength of community collaboration to create effective and accessible tools for everyone. Contribute to TrackHub to help shape the future of monitoring solutions!

![Image](https://github.com/shernandezp/TrackHub.AuthorityServer/blob/master/src/Web/wwwroot/images/logo.png)


---

## Project Repositories

| Service Name       | Repository Link                                             |
|-----------------------------|----------------------------------------------------|
| **Common Library**          | [https://github.com/shernandezp/TrackHubCommon](https://github.com/shernandezp/TrackHubCommon)    |
| **Authorization Service**   | [https://github.com/shernandezp/TrackHub.AuthorityServer](https://github.com/shernandezp/TrackHub.AuthorityServer) |
| **Security API**            | [https://github.com/shernandezp/TrackHubSecurity](https://github.com/shernandezp/TrackHubSecurity)  |
| **Management API**          | [https://github.com/shernandezp/TrackHub.Manager](https://github.com/shernandezp/TrackHub.Manager)  |
| **Router API**              | [https://github.com/shernandezp/TrackHubRouter](https://github.com/shernandezp/TrackHubRouter)    |
| **Geofencing API**          | [https://github.com/shernandezp/TrackHub.Geofencing](https://github.com/shernandezp/TrackHub.Geofencing)    |
| **Reporting API**           | [https://github.com/shernandezp/TrackHub.Reporting](https://github.com/shernandezp/TrackHub.Reporting)    |
| **Telemetry API**           | [https://github.com/shernandezp/TrackHub.Telemetry](https://github.com/shernandezp/TrackHub.Telemetry)    |
| **Trip Management API**     | [https://github.com/shernandezp/TrackHub.TripManagement](https://github.com/shernandezp/TrackHub.TripManagement)    |
| **TrackHub Web**            | [https://github.com/shernandezp/TrackHub](https://github.com/shernandezp/TrackHub)          |
| **TrackHub Mobile**         | [https://github.com/shernandezp/TrackHubMobile](https://github.com/shernandezp/TrackHubMobile)      |


---

## Overview

The Telemetry API is the GraphQL service that owns TrackHub's high-volume, append-heavy position and operator-telemetry data. It stores and serves this data; it never talks to GPS providers (that is the Router's role).

It includes:

- **Latest position projection** (`transporter_position`) — the freshest fix per transporter that backs the live map.
- **Position history** (`transporter_position_history`) — the append-only track store used for replay and reporting, with idempotent batch append and reverse-geocoded address write-back.
- **Operator sync runs** (`operator_sync_runs`) — one record per device/position sync attempt (counts, result, error).
- **Operator health checks** (`operator_health_checks`) — connectivity/health probes; the operator health and sync summary is derived from these tables at read time.
- **Retention purge** — a scheduled in-host background job that deletes expired history per account according to each account's `gps.positionHistory` retention days.

## Boundary

- These tables live in the database schema `telemetry`. The service connects with a role that has read/write on `telemetry` and **read-only** on the `app`-schema scoping tables it needs (users, groups, transporters, assignments, devices, operators, account features) so it can enforce group visibility and account scoping without a network hop.
- Visibility mirrors the rest of the platform: Administrator/Manager roles read account-wide, other users are scoped to their group membership; service clients read on behalf of already-authorized users.
- Feature gating is unchanged: latest-position and health writes are core (authorized only); history writes are gated by the `gps.positionHistory` feature.

## Architecture

Clean Architecture (Domain / Application / Infrastructure / Web) with the shared `Common.Mediator` pipeline and a HotChocolate GraphQL endpoint at `/graphql`. Authentication uses the platform AuthorityServer (audience `trackhub_api`); authorization is enforced per operation via the shared `[Authorize]` / `[RequireFeature]` attributes.
