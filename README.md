# TrackHub Management API

[English](README.en.md) | [Español](README.es.md)

TrackHub is an innovative open-source application designed to unify multiple monitoring platforms into a cohesive system. Imagine having all your monitoring needs met in one place—this is the vision behind TrackHub.

Currently in development, our project aims to foster collaboration among diverse companies and developers, promoting continuous improvement and growth. TrackHub empowers organizations to centralize information about their assets and personnel, regardless of their vendors.

We believe in the strength of community collaboration to create effective and accessible tools for everyone. Contribute to TrackHub to help shape the future of monitoring solutions!

![Image](https://github.com/shernandezp/TrackHub.AuthorityServer/blob/master/src/Web/wwwroot/images/logo.png)

---

## Overview

The Management API is TrackHub's master-data service. It owns accounts, users and groups; operators, devices, transporters and device–transporter assignments; points of interest and geocoding providers; account features and branding; documents and the driver workforce; alert events, notification rules and deliveries; platform announcements; audit events; and the device-synchronization command surface.

High-volume position and operator-telemetry data is **not** here — it belongs to the [Telemetry API](https://github.com/shernandezp/TrackHub.Telemetry).

---

## Documentation

| | |
|---|---|
| **Technical documentation** | The [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki) — start with [Manager](https://github.com/shernandezp/TrackHub/wiki/Manager), [Architecture](https://github.com/shernandezp/TrackHub/wiki/Architecture) and [Database](https://github.com/shernandezp/TrackHub/wiki/Database) |
| **User documentation** | In the app — the Help button or **F1** on any screen (English and Spanish) |
| **Deployment** | [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment) |

---

## Project Repositories

| Repository | Purpose |
|---|---|
| [TrackHubCommon](https://github.com/shernandezp/TrackHubCommon) | Shared library, distributed as local NuGet packages |
| [TrackHub.AuthorityServer](https://github.com/shernandezp/TrackHub.AuthorityServer) | Authorization service (OAuth 2.0 / OpenID Connect) |
| [TrackHubSecurity](https://github.com/shernandezp/TrackHubSecurity) | Security API — users, roles, policies, permissions |
| [TrackHub.Manager](https://github.com/shernandezp/TrackHub.Manager) | Management API — master data |
| [TrackHubRouter](https://github.com/shernandezp/TrackHubRouter) | Router API and SyncWorker — GPS provider integration |
| [TrackHub.Telemetry](https://github.com/shernandezp/TrackHub.Telemetry) | Telemetry API — positions, history, operator health |
| [TrackHub.Geofencing](https://github.com/shernandezp/TrackHub.Geofencing) | Geofencing API |
| [TrackHub.TripManagement](https://github.com/shernandezp/TrackHub.TripManagement) | Trip Management API |
| [TrackHub.Reporting](https://github.com/shernandezp/TrackHub.Reporting) | Reporting API |
| [TrackHub](https://github.com/shernandezp/TrackHub) | Web portal (React) |
| [TrackHubMobile](https://github.com/shernandezp/TrackHubMobile) | Mobile application |
| [TrackHub.IntegrationTests](https://github.com/shernandezp/TrackHub.IntegrationTests) | Cross-service GraphQL contract tests |
| [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment) | Docker deployment for the whole stack |

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
