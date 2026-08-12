# Authorization Service - TrackHub

[English](README.en.md) | [Español](README.es.md)

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
| **TrackHub Web**            | [https://github.com/shernandezp/TrackHub](https://github.com/shernandezp/TrackHub)          |




## Overview

The Authorization Service is the OpenIddict-based identity and OAuth2/OpenID Connect token server. It authenticates users and service clients and issues the access tokens (audience `trackhub_api`) that every other TrackHub service validates.
