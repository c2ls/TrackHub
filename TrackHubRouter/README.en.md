# TrackHub Router API

## Key Features

- **Multi-Provider GPS Integration**: Unified interface for 8+ GPS tracking providers (CommandTrack, Traccar, Flespi, GeoTab, GpsGate, Navixy, Samsara, Wialon)
- **Data Normalization**: Transforms diverse provider data formats into a standardized TrackHub schema
- **GraphQL & REST APIs**: Flexible querying via GraphQL and third-party integration via REST endpoints
- **Real-Time Position Tracking**: Live device position retrieval across all connected operators
- **Background Synchronization Service**: Automatic data sync to maintain local cache of device positions
- **Scalable Architecture**: Easy addition of new GPS provider integrations through modular design
- **Offline Resilience**: Local position cache ensures data availability during provider outages

---


## Quick Start

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 14+
- TrackHub Authority Server running (for authentication)
- At least one GPS provider account (e.g., Traccar, CommandTrack)

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/shernandezp/TrackHubRouter.git
   cd TrackHubRouter
   ```

2. **Configure the database and services** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "ManagerConnection": "Host=localhost;Database=trackhub_manager;Username=postgres;Password=yourpassword",
       "SecurityConnection": "Host=localhost;Database=trackhub_security;Username=postgres;Password=yourpassword"
     },
     "GraphQL": {
       "ManagerEndpoint": "https://localhost:5001/graphql"
     }
   }
   ```

3. **Configure operator credentials** in TrackHub Manager:
   - Add your GPS provider credentials through the Manager API or Web UI
   - Supported providers: CommandTrack, Traccar, Flespi, GeoTab, GpsGate, Navixy, Samsara, Wialon

4. **Start the application**:
   ```bash
   dotnet run --project src/Web
   ```

5. **Access the APIs**:
   - GraphQL Playground: `https://localhost:5001/graphql`
   - REST API Documentation: `https://localhost:5001/scalar`

---

## Components and Resources

| Component                | Description                                           | Documentation                                                                 |
|--------------------------|-------------------------------------------------------|-------------------------------------------------------------------------------|
| Hot Chocolate            | GraphQL server for .NET                               | [Hot Chocolate Documentation](https://chillicream.com/docs/hotchocolate/v13)  |
| GraphQL.Client           | HTTP client for GraphQL                               | [GraphQL.Client Documentation](https://github.com/graphql-dotnet/graphql-client)                           |
| Scalar.AspNetCore        | Integrate Scalar API for Net Core apps                | [Scalar Documentation](https://guides.scalar.com/scalar/scalar-api-references/net-integration) |
| .NET Core                | Development platform for modern applications          | [.NET Core Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview) |

---

## Overview

## Key Features

The Routing API is an essential component of TrackHub, designed to streamline and unify data integration from multiple external GPS provider services (referred to as "Operators"). It enables TrackHub to aggregate location data consistently and reliably, regardless of the source provider, by returning a standardized format that meets TrackHub’s unique requirements.
It also provides a REST endpoint for third-party applications to access the data of all units.

## Functionality

Based on configurations set in the administration panel, the Routing API:

- Unifies and processes data contracts from each GPS provider.
- Standardizes incoming data to a scalable, generic format.
- Enables TrackHub to integrate multiple provider services efficiently and consistently.

### Currently Implemented Operators

| Operator      | Documentation Link                                    | Implementation Status   | Tested    |
|---------------|-------------------------------------------------------|--------------------------|-----------|
| CommandTrack  | [CommandTrack Documentation](https://www.c2ls.co/home/documentacion-de-la-api/) | ✅ Implemented            | ✅ Tested |
| Flespi        | [Flespi Documentation](https://flespi.io/docs/)       | ✅ Implemented            | ❌ Not Tested |
| GeoTab        | [GeoTab Documentation](https://developers.geotab.com/myGeotab/guides/codeBase/usingInDotnet)       | ✅ Implemented            | ❌ Not Tested |
| GpsGate       | [GpsGate Documentation](https://support.gpsgate.com/hc/en-us/articles/360016602140-REST-API-Documentation)      | ⚠️ Partially Implemented  | ❌ Not Tested |
| Navixy        | [Navixy Documentation](https://www.navixy.com/docs/navixy-api/user-api/getting-started)       | ✅ Implemented            | ❌ Not Tested |
| Samsara       | [Samsara Documentation](https://developers.samsara.com/docs/tms-integration)       | ✅ Implemented            | ❌ Not Tested |
| Traccar       | [Traccar Documentation](https://www.traccar.org/api-reference/)      | ✅ Implemented            | ✅ Tested |
| Wialon        | [Wialon Documentation](https://help.wialon.com/en/api/user-guide)       | ✅ Implemented            | ❌ Not Tested |

## REST API
To facilitate integration with third parties, TrackHub provides a REST API with methods to retrieve unit information. This API leverages the Router API as middleware to access GPS location data for all units.

![Image](https://github.com/shernandezp/TrackHub/blob/main/src/assets/images/api.png?raw=true)

## Synchronization Service

To enhance reliability, the project includes a Synchronization Service that regularly updates device information in the local database. This service ensures that TrackHub displays the latest known position for each device, even if an external provider experiences connectivity issues or is temporarily offline.

## Future Updates

- **Expanding support for additional GPS providers**: Incorporate new protocols and providers to broaden compatibility.

## License

This project is licensed under the Apache 2.0 License. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.


