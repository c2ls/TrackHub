# TrackHub Web

[← Back to the landing page](README.md) · [Español](README.es.md)

TrackHub Web is the **React portal** — the operator-facing user interface for the whole platform. It is a React 19 + TypeScript 7 single-page application built with Vite 8 and tested with Vitest 4, served in production as static files by nginx.

It is also where **user documentation lives**: the contextual help topics under `public/help/` ship with every portal build.

---

## What it provides

- **Real-time GPS tracking** — a live map of transporters and devices, with automatic position updates, replay and trip segmentation
- **Multi-operator integration** — one interface for every connected GPS provider, with device management, manual sync, connectivity ping and health
- **Geofencing** — a polygon and circle editor on both map providers, with server-paged lists and dashboard overlays
- **Trip management** — a dispatch board for trips, stops, deliveries, route plans, tolls, proof of delivery and public tracking links
- **Documents and workforce** — versioned uploads with signatures and sharing; the driver registry, qualifications and assignments
- **Alerts and notifications** — the in-app feed, notification rules, subscriptions and templates
- **Reporting** — the governed report catalog with in-app preview and Excel/PDF export
- **Administration** — accounts, users, groups, permissions, features and service clients
- **Public status page** — `/status` renders with no sign-in, reporting per-service health and platform announcements
- **In-app contextual help** — Help button or **F1** on every screen, with a browsable index and client-side search, in English and Spanish
- **Bilingual UI** (EN/ES) and a dark/light theme

Full detail: **[Frontend](https://github.com/shernandezp/TrackHub/wiki/Frontend)** in the wiki.

---

## Quick start

### Prerequisites

- Node.js 20+
- Access to the TrackHub backend services — AuthorityServer, Security, Manager, Router, Telemetry, Geofencing, TripManagement and Reporting

### Steps

1. **Clone and install**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.git
   cd TrackHub
   npm install
   ```

2. **Configure the environment.** Edit `.env` (the development defaults point at `https://localhost`); `.env.production.template` is the deployment reference.

3. **Set up HTTPS certificates.** OAuth requires HTTPS, and the callback is registered at `https://localhost:3000/...`:

   ```bash
   npx mkcert create-ca
   npx mkcert create-cert
   ```

   This produces `ca.key` / `ca.crt` and `cert.key` / `cert.crt` in the project root. Vite picks them up automatically when both `cert.crt` and `cert.key` exist. They are gitignored — never commit them. Your browser will warn about the self-signed certificate; that is expected in development.

4. **Run**

   ```bash
   npm run dev
   ```

   Open `https://localhost:3000`.

### Scripts

| Command | Purpose |
|---|---|
| `npm run dev` (or `npm start`) | Development server — also runs the help validator first |
| `npm run build` | `tsc --noEmit` then a production build — also runs the help validator first |
| `npm run typecheck` | TypeScript only |
| `npm test` / `npm run test:watch` | Vitest |
| `npm run codegen` | Regenerate typed GraphQL documents from `schemas/*.graphql` |
| `npm run help:check` | Validate the help authoring contract without building |
| `npm run lint` | ESLint |

**The gate is `npm run typecheck && npm test && npm run build`.**

---

## Project-specific notes

- **Components never touch the network.** Every call goes through three layers: `src/api/<backend>/<domain>Operations.ts` (GraphQL documents via the generated `graphql()` tag) → `src/api/<backend>/<domain>.ts` (typed functions that throw `ApiError`) → `src/queries/<domain>.ts` (TanStack Query hooks owning cache keys and invalidation). Endpoint URLs live **only** in `src/api/core/endpoints.ts`.
- **Values travel as GraphQL variables only.** There is no string interpolation of user input into documents — the old `formatValue` escaping helper is gone.
- **Backend drift is a compile error.** The contract test suite exports each producer's SDL to `schemas/<service>.graphql`; `npm run codegen` validates every portal operation against them. After any backend GraphQL change: run the contract tests, then `npm run codegen`.
- **`src/` is 100% TypeScript.** `allowJs` is off and an ESLint guard errors on any new `.js` or `.jsx` file under `src/`.
- **Argon components and controls export real prop types** — import and use them directly. Never re-introduce local prop-slice interfaces or `as unknown as` boundary casts at call sites; if a control lacks a prop you must pass, widen the **control's** exported prop type. Theme extensions live in `src/types/mui-theme.d.ts`.
- **The `vite.config.ts` `process.env.REACT_APP_*` define shim is permanent by decision** — all reads centralize in `api/core/endpoints.ts`, and keeping the CRA convention means existing `.env` files and deployment docs stay valid. It is disabled in test mode, because suites assign env at runtime.
- **i18n keys are compile-checked.** Add every key to **both** `locales/en.json` and `locales/es.json`; dynamic keys cast at the key expression only.
- **Escape everything interpolated into a map popup.** Leaflet `bindPopup`/`bindTooltip` and the Google InfoWindow assign their argument via `innerHTML`, so React's escaping does not apply — use `escapeHtml` (`src/utils/htmlUtils.ts`). Transporter names are account-editable free text and addresses come from a third-party geocoder.
- **`datetime-local` ⇄ UTC goes through `toDateTimeLocalInput` / `fromDateTimeLocalInput`** (`src/utils/dateUtils.ts`). The control holds *local* wall time, so the instant shifts in both directions. A helper that skips the shift round-trips correctly only under `TZ=UTC` — which is exactly what dev boxes and CI run. Assert the round-trip property, never a literal.
- **Help content is validated at build time.** `scripts/build-help.mjs` runs on `predev` and `prebuild` and checks language parity, id-equals-filename, `screens:` ↔ `routes.tsx` **in both directions**, `topic:` link targets, absence of raw HTML, and asset existence. Adding or renaming a screen without updating the topic frontmatter **fails the build**.
- **`/status` must keep working with no token.** Its two anonymous fetches (`api/core/healthProbe.ts` and `getVisibleAnnouncements`) are the only sanctioned exceptions to the api-layering rule; both are documented in their file headers and reached only through `src/queries/platformStatus.ts`.
- **`RouteDefinition.principalTypes` defaults to `[User]`.** A route reachable by driver or public-link principals must set `public: true` explicitly, or those principals bounce between it and `/dashboard` forever.
- **`typescript-eslint` is disabled** until it supports TypeScript 7. `tsc` is the TypeScript lint gate.
- Reporting is REST-only (`api/BasicReports`), and the Manager documents REST base is `~/documents` — **no `api/` prefix**.

---

## A note on setup

TrackHub's goal is to standardize and simplify the integration of different monitoring providers, but its setup, deployment and maintenance require intermediate to advanced knowledge of .NET and React.

Across this application and the backend services, the repository contains passwords, certificates, environment variables and other development secrets. **This is intentional** — it lets a new development environment stand up without manual secret plumbing. Production deployments must override all of them.

---

## Documentation

- **Technical** — the [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki): [Frontend](https://github.com/shernandezp/TrackHub/wiki/Frontend), [Technology](https://github.com/shernandezp/TrackHub/wiki/Technology), [User Permissions Overview](https://github.com/shernandezp/TrackHub/wiki/User-Permissions-Overview), [Coding Standards](https://github.com/shernandezp/TrackHub/wiki/Coding-Standards)
- **User** — in the app: the Help button or **F1** on any screen. The source topics live in `public/help/{en,es}/`.
- **Deployment** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
