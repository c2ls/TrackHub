# TrackHub Web

[English](README.en.md) | [Español](README.es.md)

TrackHub is an innovative open-source application designed to unify multiple monitoring platforms into a cohesive system. Imagine having all your monitoring needs met in one place—this is the vision behind TrackHub.

Currently in development, our project aims to foster collaboration among diverse companies and developers, promoting continuous improvement and growth. TrackHub empowers organizations to centralize information about their assets and personnel, regardless of their vendors.

We believe in the strength of community collaboration to create effective and accessible tools for everyone. Contribute to TrackHub to help shape the future of monitoring solutions!

![Image](https://github.com/shernandezp/TrackHub.AuthorityServer/blob/master/src/Web/wwwroot/images/logo.png)

---

## Overview

TrackHub Web is the React portal — the operator-facing user interface for the platform. It talks to the Security, Management, Router, Telemetry, Geofencing, Trip Management and Reporting APIs, and renders the live map and replay, GPS integration and device management, geofencing, trip planning and tracking, documents and workforce, reporting, account administration, and the public status page.

It is also where **user documentation lives**: the contextual help topics under `public/help/` ship with every portal build.

---

## Documentation

| | |
|---|---|
| **Technical documentation** | The [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki) — start with [Frontend](https://github.com/shernandezp/TrackHub/wiki/Frontend), [Technology](https://github.com/shernandezp/TrackHub/wiki/Technology) and [User Permissions Overview](https://github.com/shernandezp/TrackHub/wiki/User-Permissions-Overview) |
| **User documentation** | In the app — the Help button or **F1** on any screen (English and Spanish) |
| **Deployment** | [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment) |

---

## Modules in this repository

| Module | Purpose |
|---|---|
| [TrackHubCommon](../TrackHubCommon) | Shared library, consumed as a ProjectReference |
| [TrackHub.AuthorityServer](../TrackHub.AuthorityServer) | Authorization service (OAuth 2.0 / OpenID Connect) |
| [TrackHubSecurity](../TrackHubSecurity) | Security API — users, roles, policies, permissions |
| [TrackHub.Manager](../TrackHub.Manager) | Management API — master data |
| [TrackHubRouter](../TrackHubRouter) | Router API and SyncWorker — GPS provider integration |
| [TrackHub.Telemetry](../TrackHub.Telemetry) | Telemetry API — positions, history, operator health |
| [TrackHub.Geofencing](../TrackHub.Geofencing) | Geofencing API |
| [TrackHub.TripManagement](../TrackHub.TripManagement) | Trip Management API |
| [TrackHub.Reporting](../TrackHub.Reporting) | Reporting API |
| [TrackHub.Portal](../TrackHub.Portal) | Web portal (React) — this module |
| [TrackHub.IntegrationTests](../TrackHub.IntegrationTests) | Cross-service GraphQL contract tests |
| [TrackHub.Deployment](../TrackHub.Deployment) | Docker deployment for the whole stack |
| [TrackHubMobile](https://github.com/shernandezp/TrackHubMobile) | Mobile application (separate repository) |

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
