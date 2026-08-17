# TrackHub Web Application

## Key Features

- **Real-Time GPS Tracking**: Live map visualization of transporters and devices with automatic position updates
- **Multi-Operator Integration**: Unified interface to manage multiple GPS providers (CommandTrack, Traccar, Flespi, GeoTab, and more)
- **Geofencing Management**: Create, edit, and monitor geographic boundaries for assets with real-time alerts
- **Role-Based Access Control**: Granular permission management with customizable policies and user groups
- **Device & Transporter Management**: Complete CRUD operations for devices, transporters, and operator credentials
- **Reporting & Analytics**: Export operational data in Excel format with customizable filters, including live positions, position history, geofence status, and geofence event history
- **Multi-Language Support**: Full internationalization with English and Spanish interfaces
- **Dark/Light Theme**: Customizable UI appearance for optimal user experience
- **Secure Authentication**: OAuth 2.0/OpenID Connect integration with PKCE flow via OpenIdDict
- **Error Handling**: React Error Boundary for crash recovery with MUI Snackbar toast notifications
- **Security Hardening**: GraphQL injection protection, request timeouts, token refresh race condition handling, and security meta headers

---

## Quick Start

### Prerequisites

- Node.js 18+
- npm or yarn
- Access to TrackHub backend services (Authority Server, Manager, Router, Security, Geofencing, Reporting APIs)

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/shernandezp/TrackHub.git
   cd TrackHub
   ```

2. **Install dependencies**:
   ```bash
   npm install
   ```

3. **Configure environment variables**:
   ```bash
   cp .env.example .env
   ```

4. **Set up HTTPS certificates** (required for OAuth):
   ```bash
   npm install --save-dev mkcert
   npx mkcert create-ca
   npx mkcert create-cert
   ```

5. **Start the development server**:
   ```bash
   npm start
   ```

6. **Open your browser** at `https://localhost:3000`

---

## Components and Resources

| Component                | Description                                           | Documentation                                                                 |
|--------------------------|-------------------------------------------------------|-------------------------------------------------------------------------------|
| Argon Dashboard 2 MUI - v3.0.1             | React JS and MUI based template        | [Creative Tim Documentation](https://www.creative-tim.com/product/argon-dashboard-material-ui)                           |
| React JS 18.3.1               | Library for web and native user interfaces     | [React JS Documentation](https://react.dev/) |

---

## Application Overview

TrackHub Application is a web client developed with React, based on the Argon template by [Creative Tim](https://www.creative-tim.com/). This client serves as the user interface for TrackHub Services, allowing users to manage various system aspects such as accounts, operators, devices, carriers, users, and permissions. Additionally, it provides tools for real-time geographical data visualization, such as the location of GPS devices, with corresponding labeling.

---

## Login Page
*The authentication flow is initiated by the login page, which points to the Authority Server of TrackHub as the authentication method for the web client.* 

![Image](https://github.com/shernandezp/TrackHub/blob/main/src/assets/images/login.png?raw=true)

---

## Main Dashboard
*Once logged in, users are presented with the main dashboard, which includes a map displaying real-time data related to device locations and other geographical information. This web client communicates with TrackHub services using GraphQL endpoints for efficient querying and manipulation of data*

![Image](https://github.com/shernandezp/TrackHub/blob/main/src/assets/images/dashboard.png?raw=true)
![Image](https://github.com/shernandezp/TrackHub/blob/main/src/assets/images/trips.png?raw=true)

---

## Geofences
*TrackHub allows users to create, update, and delete geofences, enabling real-time monitoring of units based on their geographical location.*

![Image](https://github.com/shernandezp/TrackHub/blob/main/src/assets/images/geofence.png?raw=true)

---

## Settings Management Screen
*The settings screen allows administrators to manage system data, such as user accounts, permissions, and operator settings.*

![Image](https://github.com/shernandezp/TrackHub/blob/main/src/assets/images/manage.png?raw=true)

---

## Reports
*Export available unit data in Excel format.*

![Image](https://github.com/shernandezp/TrackHub/blob/main/src/assets/images/reports.png?raw=true)

---

## Main Layers:

1. **Reusable UI Components (components)**  
   The components directory contains reusable UI components used throughout the application. These components are designed to be modular and can be easily integrated into different parts of the application.

2. **Global State Management with Context API (context)**  
   The context directory contains context providers that manage the global state across the application. Context providers are used to share state and functions between components without manually passing props down at each level. The Context API helps to avoid "prop drilling," especially in larger applications, improving maintainability and scalability.

3. **Custom Controls and Specific UI (controls)**  
   The controls directory contains custom controls and UI elements specific to the application's requirements. These controls are built on top of the base components and provide additional functionality and styling.

4. **Data Management and Transformation (data)**  
   The data directory contains files related to data, including mock data, data models, and data transformation functions. This layer is responsible for managing the application's data and ensuring it is in the correct format for use by the components.

5. **Layout Structure and Organization (layout)**  
   The layouts directory contains layout components that define the structure and organization of different sections of the application. Layout components are used to create consistent layouts and navigation across the application.

6. **Multi-language Support (locales)**  
   The locales directory contains localization files that provide support for multiple languages. This layer is responsible for managing translations and ensuring the application can be used in different languages.

7. **Business Services and API Calls (services)**  
   The services directory contains service files that handle data fetching and business logic. Services are responsible for making API calls, processing data, and providing functions that can be used by components and context providers.

8. **Utility Functions for the Application (utils)**  
   The utils directory contains utility functions that provide common functionality used throughout the application. These functions are designed to be reusable and can be easily integrated into different parts of the application.

---

## Environment Variables

The `.env` file contains the following environment variables used in the application:

- **`REACT_APP_DEFAULT_LAT=4.624335`**
- **`REACT_APP_DEFAULT_LNG=-74.063644`**
  These two variables define the default map center when the user denies location permissions in the browser.

- **`REACT_APP_CLIENT_ID=web_client`**  
  Defines the client ID for the web client application. This value is used for authentication and identification during API calls and user login.

- **`REACT_APP_AUTHORIZATION_ENDPOINT=https://localhost/Identity/authorize`**  
  Specifies the endpoint for user authorization. This URL is used during the authentication process when the client requests a token.

- **`REACT_APP_TOKEN_ENDPOINT=https://localhost/Identity/token`**  
  Defines the endpoint to exchange an authorization code for an access token. This URL is used by the client to request an OAuth token after successful user authorization.

- **`REACT_APP_CALLBACK_ENDPOINT=https://localhost:3000/authentication/callback`**  
  Specifies the callback URL where the authentication provider will redirect the user after successful login. This is where the access token will be sent.

- **`REACT_APP_REVOKE_TOKEN_ENDPOINT=https://localhost/Identity/revoke`**  
  Defines the endpoint used to revoke an access token. This is typically used during logout or when the token is no longer needed.

- **`REACT_APP_LOGOUT_ENDPOINT=https://localhost/Identity/logout`**  
  Specifies the logout endpoint. This URL is used to log the user out and terminate the active session with the authentication provider.

- **`REACT_APP_MANAGER_ENDPOINT=https://localhost/Manager/graphql`**  
  Defines the GraphQL endpoint for managing the application. This endpoint is used to interact with the backend system for administrative functions like managing users, devices, and services.

- **`REACT_APP_ROUTER_ENDPOINT=https://localhost/Router/graphql`**  
  Specifies the GraphQL endpoint for routing-related tasks. It handles the routing and location data, enabling interaction with GPS and mapping services.

- **`REACT_APP_SECURITY_ENDPOINT=https://localhost/Security/graphql`**  
  Defines the GraphQL endpoint for security-related operations, such as authentication, access control, and securing resources within the application.

- **`REACT_APP_GEOFENCING_ENDPOINT=https://localhost/Geofence/graphql`**
  Specifies the GraphQL endpoint for geofencing-related operations. This endpoint is used to manage geofences in the system.

- **`REACT_APP_REPORTING_ENDPOINT=https://localhost/Reporting/`**
  Defines the REST endpoint for reporting-related operations. This endpoint retrieves Excel files in binary format based on user requests.

These environment variables are critical for configuring various aspects of the application, including authentication, API calls, and system management.

---

## HTTPS Setup for Local Development

The React application is configured to run with HTTPS enabled. To set up SSL certificates for local development:

### Step 1: Install mkcert via npm
```bash
npm install --save-dev mkcert
```

### Step 2: Generate certificates
```bash
npx mkcert create-ca
npx mkcert create-cert
```

This will create the following files in your project root:
- `ca.key` and `ca.crt` (Certificate Authority)
- `cert.key` and `cert.crt` (SSL certificate for localhost)

### Step 3: Start the application
```bash
npm start
```

The application will run on `https://localhost:3000` using the generated certificates.

**Note:** The certificate files are already configured in `.gitignore` and should not be committed to the repository. You may see browser warnings about self-signed certificates - you can safely proceed for local development.

## Notes  
While TrackHub's goal is to standardize and simplify the code to unify different monitoring providers (operators), its setup, deployment, and maintenance require an intermediate to advanced knowledge of .NET Core and React.

Not only for this application but for all services in general, the code includes passwords, certificates, environment variables, and some secrets. This information is provided to facilitate and speed up the setup of a new development environment. However, all these configurations and secrets must be properly managed in production environments.

## License

This project is licensed under the Apache 2.0 License. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.



