/**
 * Typed environment variables (CRA-era REACT_APP_ prefix, kept via Vite's
 * envPrefix-less define shim in vite.config.ts). Source code reads these
 * through process.env.*; the references are statically replaced at build
 * time. Centralized typed access arrives with src/api/core/endpoints.ts
 *.
 */
declare namespace NodeJS {
  interface ProcessEnv {
    readonly NODE_ENV: 'development' | 'production' | 'test';
    readonly REACT_APP_CLIENT_ID: string;
    readonly REACT_APP_AUTHORIZATION_ENDPOINT: string;
    readonly REACT_APP_TOKEN_ENDPOINT: string;
    readonly REACT_APP_CALLBACK_ENDPOINT: string;
    readonly REACT_APP_REVOKE_TOKEN_ENDPOINT: string;
    readonly REACT_APP_LOGOUT_ENDPOINT: string;
    readonly REACT_APP_MANAGER_ENDPOINT: string;
    readonly REACT_APP_ROUTER_ENDPOINT: string;
    readonly REACT_APP_SECURITY_ENDPOINT: string;
    readonly REACT_APP_GEOFENCING_ENDPOINT: string;
    readonly REACT_APP_REPORTING_ENDPOINT: string;
    readonly REACT_APP_TELEMETRY_ENDPOINT: string;
    readonly REACT_APP_DEFAULT_LAT: string;
    readonly REACT_APP_DEFAULT_LNG: string;
  }
}
