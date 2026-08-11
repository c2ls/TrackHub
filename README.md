# TrackHub Trip Management API

[English](README.en.md) | [Español](README.es.md)

TrackHub is an innovative open-source application designed to unify multiple monitoring platforms into a cohesive system. Imagine having all your monitoring needs met in one place—this is the vision behind TrackHub.

Currently in development, our project aims to foster collaboration among diverse companies and developers, promoting continuous improvement and growth. TrackHub empowers organizations to centralize information about their assets and personnel, regardless of their vendors.

We believe in the strength of community collaboration to create effective and accessible tools for everyone. Contribute to TrackHub to help shape the future of monitoring solutions!

![Image](https://github.com/shernandezp/TrackHub.AuthorityServer/blob/master/src/Web/wwwroot/images/logo.png)

---

## Overview

The Trip Management API owns trips, stops, deliveries, route plans, toll estimation and public tracking links. It plans routes through OpenRouteService, captures proof of delivery, and drives the trip lifecycle from `Created` through to a terminal status.

**The lifecycle is zero-touch.** A trip starts itself when its vehicle reaches the origin zone and closes itself when its route is done; loading, transit and unloading are measured from zone entry and exit rather than from a button. Users plan, the system measures, and the manual lifecycle commands survive as dispatcher overrides for dead GPS and corrections. `autoLifecycle: false` on the account turns the whole of it off.

> **Deploying a schema change here:** the `trip` schema has no `__EFMigrationsHistory` — it is hand-managed. Run `tools/sql/add-zero-touch-lifecycle.sql`; it ends in `ROLLBACK` so the first run is a dry run. Its one-vehicle-one-trip guard will fire on any database predating spec 11a and will name the offending vehicles. Republish TripManagement, Reporting and the portal together.

---

## Documentation

| | |
|---|---|
| **Technical documentation** | The [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki) — start with [Trip Management](https://github.com/shernandezp/TrackHub/wiki/Trip-Management) and [Database](https://github.com/shernandezp/TrackHub/wiki/Database) |
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
