# TrackHub Telemetry API

[English](README.en.md) | [Español](README.es.md)

TrackHub is an innovative open-source application designed to unify multiple monitoring platforms into a cohesive system. Imagine having all your monitoring needs met in one place—this is the vision behind TrackHub.

Currently in development, our project aims to foster collaboration among diverse companies and developers, promoting continuous improvement and growth. TrackHub empowers organizations to centralize information about their assets and personnel, regardless of their vendors.

We believe in the strength of community collaboration to create effective and accessible tools for everyone. Contribute to TrackHub to help shape the future of monitoring solutions!

![Image](../TrackHub.AuthorityServer/src/Web/wwwroot/images/logo.png)

---

## Overview

The Telemetry API owns TrackHub's high-volume, append-heavy data: the latest-position projection that backs the live map, the append-only position history used for replay and reporting, operator sync runs, and operator health checks.

It stores and serves this data; it never talks to GPS providers — that is the Router's role.

---

## Documentation

| | |
|---|---|
| **Technical documentation** | The [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki) — start with [Telemetry](https://github.com/shernandezp/TrackHub/wiki/Telemetry) and [Database](https://github.com/shernandezp/TrackHub/wiki/Database) |
| **User documentation** | In the app — the Help button or **F1** on any screen (English and Spanish) |
| **Deployment** | [TrackHub.Deployment](../TrackHub.Deployment) |

---

## Modules in this repository

| Repository | Purpose |
|---|---|
| [TrackHubCommon](../TrackHubCommon) | Shared framework, referenced by project |
| [TrackHub.AuthorityServer](../TrackHub.AuthorityServer) | Authorization service (OAuth 2.0 / OpenID Connect) |
| [TrackHubSecurity](../TrackHubSecurity) | Security API — users, roles, policies, permissions |
| [TrackHub.Manager](../TrackHub.Manager) | Management API — master data |
| [TrackHubRouter](../TrackHubRouter) | Router API and SyncWorker — GPS provider integration |
| [TrackHub.Telemetry](../TrackHub.Telemetry) | Telemetry API — positions, history, operator health |
| [TrackHub.Geofencing](../TrackHub.Geofencing) | Geofencing API |
| [TrackHub.TripManagement](../TrackHub.TripManagement) | Trip Management API |
| [TrackHub.Reporting](../TrackHub.Reporting) | Reporting API |
| [TrackHub.Portal](../TrackHub.Portal) | Web portal (React) |
| [TrackHubMobile](https://github.com/shernandezp/TrackHubMobile) | Mobile application |
| [TrackHub.IntegrationTests](../TrackHub.IntegrationTests) | Cross-service GraphQL contract tests |
| [TrackHub.Deployment](../TrackHub.Deployment) | Docker deployment for the whole stack |

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
