# TrackHub Portal

The web portal: a React 19 + TypeScript 7 single-page application built with Vite 8, served in
production as static files by nginx. It is also where the **user documentation** lives — the
in-app help topics under `public/help/{en,es}/` ship with every build.

Wiki: [Frontend](https://github.com/shernandezp/TrackHub/wiki/Frontend).

## Build, run, test

Requires Node.js 22. From this folder:

```bash
npm ci
npm start              # https://localhost:3000 (npm run dev is the same)
npm run typecheck && npm test && npm run build     # the CI gate
npm run lint
npm run codegen        # regenerate typed GraphQL documents from schemas/*.graphql
npm run help:check     # validate the help topics without building
npm run e2e            # Playwright suite against a running stack, see e2e/README.md
```

`.env` points every endpoint at the local Docker stack (`https://trackhub.local:8443`, see
[LOCAL-DOCKER.md](../TrackHub.Deployment/LOCAL-DOCKER.md)); put machine-specific overrides in
`.env.local`. `.env.production.template` lists the keys a deployment sets.

**HTTPS is required** — the OAuth callback is `https://localhost:3000/authentication/callback`.
Vite serves HTTPS only when `cert.crt` and `cert.key` exist in this folder; without them it falls
back to HTTP silently and sign-in fails. `TrackHub.Deployment/scripts/local/Setup-TrackHubLocal.ps1`
installs a certificate that covers both `localhost` and `trackhub.local`. Both files are
gitignored.

## Gotchas

- **Components never touch the network.** Calls go `src/api/<backend>/<domain>Operations.ts`
  (documents via the generated `graphql()` tag) → `src/api/<backend>/<domain>.ts` (typed functions
  that throw `ApiError`) → `src/queries/<domain>.ts` (TanStack Query hooks owning cache keys and
  invalidation). Endpoint URLs live only in `src/api/core/endpoints.ts`.
- **Values travel as GraphQL variables**, never interpolated into documents.
- **Backend drift is a compile error.** The contract suite exports each producer's SDL to
  `schemas/<service>.graphql`; after a backend GraphQL change run
  `dotnet test ../TrackHub.IntegrationTests`, then `npm run codegen`.
- **`src/` is TypeScript only.** `allowJs` is off and ESLint errors on any new `.js`/`.jsx` there.
  `typescript-eslint` is installed but disabled until it supports TypeScript 7; `tsc` is the gate.
- **Use the controls' exported prop types.** Never add local prop-slice interfaces or
  `as unknown as` casts at call sites; widen the control's own type instead. Theme extensions live
  in `src/types/mui-theme.d.ts`.
- **`process.env.REACT_APP_*` is a permanent build-time define** in `vite.config.ts`, read only by
  `src/api/core/endpoints.ts`. It is off in test mode, where suites assign env at runtime.
  `REACT_APP_*` values are baked into the bundle, so changing one means rebuilding.
- **i18n keys are compile-checked.** Add every key to both `src/locales/en.json` and `src/locales/es.json`.
- **Escape everything interpolated into a map popup** with `escapeHtml` (`src/utils/htmlUtils.ts`):
  Leaflet popups and the Google InfoWindow assign via `innerHTML`.
- **`datetime-local` ⇄ UTC goes through `toDateTimeLocalInput` / `fromDateTimeLocalInput`**
  (`src/utils/dateUtils.ts`). A helper that skips the offset shift passes under `TZ=UTC`, which is
  what CI runs, so assert the round-trip property, never a literal.
- **Calendar defaults and day filters come from `useAccountCalendar()`**, computed in the
  account's IANA zone, never from the browser's `new Date()` parts.
- **Help content is validated on every `start`/`dev`/`build`** by `scripts/build-help.mjs`:
  language parity, id equals filename, `screens:` ↔ `routes.tsx` in both directions, `topic:`
  links, no raw HTML, assets present. Adding or renaming a screen without updating a topic's
  frontmatter fails the build.
- **`/status` must work without a token.** Its anonymous fetches (`api/core/healthProbe.ts` and the
  visible-announcements call) are the only exceptions to the API layering, reached only through
  `src/queries/platformStatus.ts`.
- **`RouteDefinition.principalTypes` defaults to `[User]`.** A route for staff or public-link
  principals must set `public: true`, or they bounce between it and `/dashboard`.
- Reporting is REST (`api/BasicReports`); Manager's document REST base is `documents`, with no
  `api/` prefix.
