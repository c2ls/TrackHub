# TrackHub Web

[English](README.en.md) | [Español](README.es.md)

TrackHub is an innovative open-source application designed to unify multiple monitoring platforms into a cohesive system. Imagine having all your monitoring needs met in one place—this is the vision behind TrackHub.

Currently in development, our project aims to foster collaboration among diverse companies and developers, promoting continuous improvement and growth. TrackHub empowers organizations to centralize information about their assets and personnel, regardless of their vendors.

We believe in the strength of community collaboration to create effective and accessible tools for everyone. Contribute to TrackHub to help shape the future of monitoring solutions!

![Image](https://github.com/shernandezp/TrackHub.AuthorityServer/blob/master/src/Web/wwwroot/images/logo.png)


---

## Modules in this repository

| Service Name       | Repository Link                                             |
|-----------------------------|----------------------------------------------------|
| **Common Library**          | [TrackHubCommon](../TrackHubCommon)    |
| **Authorization Service**   | [TrackHub.AuthorityServer](../TrackHub.AuthorityServer) |
| **Security API**            | [TrackHubSecurity](../TrackHubSecurity)  |
| **Management API**          | [TrackHub.Manager](../TrackHub.Manager)  |
| **Router API**              | [TrackHubRouter](../TrackHubRouter)    |
| **Geofencing API**          | [TrackHub.Geofencing](../TrackHub.Geofencing)    |
| **Reporting API**           | [TrackHub.Reporting](../TrackHub.Reporting)    |
| **Telemetry API**           | [TrackHub.Telemetry](../TrackHub.Telemetry)    |
| **TrackHub Web**            | [TrackHub.Portal](../TrackHub.Portal) (this module) |
| **TrackHub Mobile**         | [https://github.com/shernandezp/TrackHubMobile](https://github.com/shernandezp/TrackHubMobile)   |



## Overview

TrackHub Web is the React web portal — the operator-facing UI for the platform. It talks to the Security, Management, Router, Geofencing, Reporting, and Telemetry APIs and renders the live map, replay, GPS integration and device management, geofencing, reporting, and account administration.
