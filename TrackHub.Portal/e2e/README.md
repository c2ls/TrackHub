# Portal end-to-end suite (Playwright)

The regression gate for `TrackHub.Portal`. It drives the **real** portal in a
real browser against the **real** deployed stack: it signs in through the actual
OAuth flow, opens every screen, exercises the primary action of each one, and
checks that what it did survived a reload.

There are no mocked backends. `page.route` appears in exactly three places and
only ever to *break* something on purpose — a failed `/health` probe, a forced
`FORBIDDEN`, a forced `ACCOUNT_SUSPENDED` — never to fake a success.

---

## Prerequisites

1. **The backend stack is up.**

   ```powershell
   cd TrackHub\TrackHub.Deployment
   .\scripts\local\Start-TrackHubLocal.ps1
   ```

   `TrackHub.Deployment/LOCAL-DOCKER.md` covers first-time setup (certificates,
   the `127.0.0.1 trackhub.local` hosts entry, the generated appsettings).

2. **The portal serves HTTPS.** `cert.crt` and `cert.key` must exist in
   `TrackHub.Portal/` — without them Vite silently falls back to HTTP and the
   OAuth callback fails. The setup script installs them.

3. **Browsers installed.**

   ```bash
   npm run e2e:install
   ```

The suite starts the dev server itself (`npm start`) unless `E2E_REUSE_SERVER=1`
tells it to use one that is already running.

---

## Running

```bash
npm run e2e                     # the whole suite
npm run e2e -- e2e/specs/50-geofences.spec.ts    # one file
npm run e2e -- --project=chromium-es             # the Spanish pass only
npm run e2e -- -g "creates a unit"               # by test name
npm run e2e:headed              # watch it
npm run e2e:ui                  # the Playwright UI runner
npm run e2e:report              # open the last HTML report
```

`E2E_REUSE_SERVER=1 npm run e2e` is the fast loop when `npm start` is already
running in another terminal.

A failure leaves a trace, a screenshot and a video under `test-results/`:

```bash
npx playwright show-trace test-results/<test>/trace.zip
```

---

## Configuration

Copy `e2e/.env.e2e.example` to `e2e/.env.e2e` (gitignored) and edit. **Every
value is optional** — the defaults drive the seeded local stack. Backend URLs are
never configured here: they are read from the portal's own `.env`/`.env.local`
through Vite's `loadEnv`, so the tests always talk to the same services the app
under test is pointed at.

| Variable | Effect |
|---|---|
| `E2E_BASE_URL` | Portal origin. Defaults to the origin of `REACT_APP_CALLBACK_ENDPOINT`. |
| `E2E_ADMIN_EMAIL` / `E2E_ADMIN_PASSWORD` | The signing-in administrator. Defaults to the seeded `email@mail.com` / `12345678`. |
| `E2E_MANAGER_EMAIL` / `E2E_MANAGER_PASSWORD` | An existing Manager account, for the role-gate tests. |
| `E2E_USER_EMAIL` / `E2E_USER_PASSWORD` | An existing plain-User account, for the role-gate tests. |
| `E2E_REUSE_SERVER=1` | Use an already-running `npm start` instead of launching one. |
| `E2E_RUN_ID` | Override the run marker (normally the start time in base 36). |
| `E2E_SWEEP_ALL=1` | Let the global teardown also sweep this run's own `e2e-*` rows. |

### Tests that are conditional, and why

| Variable | Unskips | Why it is opt-in |
|---|---|---|
| `E2E_HAS_POSITIONS=1` | Dashboard history replay | Positions come from the Router sync workers and real GPS providers; with no recorded points there is no replay to assert. |
| `E2E_GPS_OPERATOR_ID` | The manual GPS synchronisation | A sync needs working provider credentials; it cannot be simulated. |
| `E2E_TRIP_IN_TRANSIT_ID` | The pause/resume exception verbs | The trip lifecycle is GPS-driven (spec 11a); a browser cannot make a truck leave a zone. |
| `E2E_CREATE_ACCOUNT=1` | The feature-gate test | Toggling a feature is account-wide; the shared master account is not changed by default. |
| `E2E_ALLOW_GEOCODING_SWITCH=1` | Activating a geocoding provider | The active provider is platform-wide, affecting every account. |
| `E2E_GOOGLE_MAPS=1` | Switching the account map provider | The flip lands on Google, and a Maps key that is not authorized for the portal origin answers `RefererNotAllowedMapError` — a broken map on every screen until the setting is put back. |
| `E2E_ALLOW_ROLE_MATRIX_WRITE=1` | Granting/revoking a ROLE permission | A role grant widens what every holder may do. The POLICY matrix — additive grants that cannot dent the mandatory baseline — is exercised unconditionally instead. |
| `E2E_ALLOW_PASSWORD_CHANGE=1` | Changing the administrator's password | One-way door unless `E2E_ADMIN_PASSWORD` itself meets the platform policy (8+ chars, upper, lower, digit): the Security validator refuses to set a weaker one back, and the seeded `12345678` is weaker. |
| `E2E_MANAGER_*` / `E2E_USER_*` | The role-gate tests | Optional. Unset, the setup project creates both principals through the real Account Management screens and the teardown removes them. |
| `E2E_SCANNER=1` | reserved | The document scanner's final state; the upload surface is currently unreachable (see findings). |

---

## How sessions work

The portal keeps its access token **in memory** and its refresh token in React
state, so a saved storage state restores no session by itself. What it does
restore is the AuthorityServer's own cookie, and that is enough: the portal's
`/authorize` round trip then completes with no login form and the callback mints
a fresh token pair.

Consequences the suite is built around:

- The `setup` project types credentials **once per role** and writes
  `e2e/.auth/{admin,manager,user}.json` plus the captured tokens. Nothing else
  ever types a password — `AuthContext.login()` allows 3 attempts per 30 s.
- **A deep link cannot be typed.** Every document load re-runs the OAuth round
  trip, and the callback navigates to `/dashboard` unconditionally, dropping the
  requested route. `Shell.open(key)` therefore lands on the dashboard and then
  *clicks* the sidenav entry, which is a client-side navigation.
- `Shell.reloadTo(key)` is how "did it really persist?" is asked: it reloads
  (dropping the in-memory token) and comes back through the dashboard.

To refresh the stored sessions, delete `e2e/.auth/` — the `setup` project
recreates it on the next run (it already clears stale states each run).

---

## Test data

Everything a test creates carries `e2e-<runId>-<n>` in the field a human reads,
where `runId` is the run's start time in base 36. Two things follow: a run can
find exactly its own rows, and a row's **age** is derivable from its name.

- Each test registers the undo for a row as soon as the row exists, so a later
  assertion failure still releases it (`fixtures/data.ts`).
- The global teardown deletes the principals the setup created and then sweeps
  `e2e-*` rows left by earlier, crashed runs (`fixtures/sweep.ts`).
- Trips are released with **delete → abort → cancel** in that order: delete is
  refused once a trip has history, abort is refused from `Created`, and cancel is
  the only verb that retires a queued trip that already acquired events
  (spec 11b). A leftover `Created` trip silently absorbs its unit's arming slot.
- **A cancelled trip's row is permanent.** Once a trip has history the backend
  answers `TRIP_HAS_HISTORY` to every delete, through any surface. The test that
  exercises the cancel verb therefore leaves a `Cancelled` row behind. It is
  inert — arming only ever considers `Created` trips — but it is the one thing
  the suite creates and cannot take back.

---

## Layout

```
e2e/
  .env.e2e.example    committed template (.env.e2e is gitignored)
  fixtures/
    env.ts            portal env (via Vite loadEnv) + E2E_* knobs
    auth.ts           the real sign-in, token capture, storage states
    api.ts            seeding/cleanup client — never replaces a UI assertion
    data.ts           unique-name factory + per-test cleanup registry
    sweep.ts          global sweep of abandoned e2e-* rows
    i18n.ts           resolves the real locale bundles, so one test runs in EN and ES
    services.ts       the services the status page probes
    index.ts          the merged `test` export
  pages/
    shell.ts          sidenav, navbar, configurator, announcements
    tableAccordion.ts the shared section control (expand, search, paginate, rows)
    dialogs.ts        FormDialog / ConfirmDialog / MessageDialog / allocator / matrix
    crud.ts           the create → verify → edit → reload → delete shape
    dashboard.ts      fleet rows, map focus, the three map toggles, tabs
    geofences.ts      the list beside the map, the properties route, map focus
    reports.ts        the catalog read from the API, filter card, choose a report
    trips.ts          board rows, planning a trip, planner stops, trip release
    profile.ts        the three cards and the settings load/save round trips
  setup/auth.setup.ts the `setup` project
  specs/              one file per screen (see below)
  specs/support/      helpers that are not tests
```

---

## Adding a screen

1. **Give its sections a key.** Pass `sectionKey="…"` to `TableAccordion`; the
   control renders `data-testid="section-<key>"`, `-header` and `-add`. Do not
   add ids to individual dialogs — the shared dialogs already carry
   `dialog-form`, `dialog-confirm`, `dialog-message`, `dialog-dynamic-table`,
   `dialog-help`, and table rows carry `row-<entityId>`.
2. **Write the test as a requirement.** The name should read like one: *"a
   manager creates a unit and sees it in the fleet list after reload"*.
3. **Select by role and localized name.** `getByRole('button', { name:
   t('generic.add') })`. The `t` fixture is bound to the project's language, so
   the same body runs in `chromium` and `chromium-es`.
4. **Assert the visible outcome AND the reload.** A closed dialog is not proof
   that anything was stored — `CrudFlow.reload(shell, screen)` then re-assert.
5. **Register the cleanup as soon as the row exists**, with an API fallback for
   rows the UI cannot delete (staff, notification rules, document types and
   toll classes are deactivated, not deleted).
6. **Never wait on network idle.** Wait on the UI state you expect.

### MUI specifics you will hit

- `Select` opens a listbox in a portal: `page.getByRole('option', …)`, and the
  first option is a disabled placeholder — `FormDialog.selectFirst(id)` skips it.
- `Switch` has role **`switch`**, not `checkbox`.
- Icon-only buttons have **no accessible name** (MUI marks `Icon` `aria-hidden`),
  which is why the pagination controls carry `data-testid`.
- Tables inside a section page **ten rows at a time in the browser**;
  `Section.findRowAnyPage` walks them.
- The configurator drawer is always mounted, so a page-wide
  `getByRole('button', { name: 'Save' })` can match it. Scope to the card.
- Saves confirm through the app's own snackbar (`getByRole('alert')`), not a
  native dialog.

---

## Specs

| File | Covers |
|---|---|
| `00-auth` | Real OAuth sign-in, wrong password, sign off, deep links, `/status` signed out |
| `01-shell` | Sidenav, contextual help, notification bell, mini toggle, configurator, announcements, unknown routes, the auth error page |
| `10-dashboard` | Units tab (cards, filters, map, side list, refresh counter) and the Positions/replay tab |
| `20-system-admin` | Accounts (create, edit, the status state machine), clients, service-client permissions, transporter types, geocoding providers, the toll catalog and its CSV import, the role and policy matrices, account features, support grants |
| `30-manage-admin-account` | Account record, branding, feature entitlements |
| `31-manage-admin-fleet` | Devices, units, groups + allocators, points of interest, staff, staff credentials/qualifications/assignments |
| `32-manage-admin-access` | Users, the password dialog, the role and policy allocators, lockout and unlock |
| `33-manage-admin-alerts` | Notification rules, alert subscriptions, templates, alert events, deliveries |
| `34-manage-admin-documents` | Document library, expirations, document types, the embedded document panel (upload, share, remove), public links |
| `35-manage-admin-operations` | Audit trail, background jobs |
| `40-gps-integration` | Provider dashboard, operators (offered providers vs the Router catalog), credentials, synchronized devices + manual registration, assignments, retention, manual sync |
| `50-geofences` | Map drawing (circle), properties, validation, activation, filters, map focus, delete |
| `60-trips` | Dispatch board, planning (origin sources, trip type, route reuse), stops, deliveries, proof of delivery, assignment, timeline, overrides, bulk upload, toll classes |
| `61-trip-tracking-public` | The anonymous `/track` page: a real shared link, and the invalid/unknown/revoked states |
| `70-reports` | Catalog, catalog-driven filters, preview, Excel and PDF export |
| `80-profile` | Profile card, password dialog, language, appearance preferences, memberships |
| `90-platform-status` | Signed-out tiles, refresh, a forced outage, background jobs, announcement management |
| `95-gates` | Role and feature gates, FORBIDDEN, FEATURE_DISABLED, ACCOUNT_SUSPENDED, an unrecoverable session |
| `99-i18n` | The same flows in Spanish, with an exact "no raw keys" check |

### Preconditions that are easy to get wrong

Two of these cost a full re-run each, so they are worth knowing before writing a
test that seeds data:

- **Units are group-scoped.** Every unit picker on the trip screen is fed by
  `transporterLookupByUser`, so a unit created through the API without group
  membership is invisible to the signed-in user and never offered. Use
  `api.visibleTransporters()` and pick one the user can already see.
- **A staff member is assignable only to a trip on a unit it is linked to.**
  `ValidateStaffAssignment` matches the staff member's default transporter (or a live
  assignment) against the trip's; anything else is refused with
  `TRIP_STAFF_NOT_ASSIGNABLE`, the toast is easy to miss, and the panel simply
  keeps saying "No staff member assigned yet". Seed the staff with
  `api.createStaffMember(name, transporterId)` and plan the trip on that unit.
- **Assert inside the panel, not the page.** The picker keeps showing the name
  it selected whether or not the write was accepted, so a page-wide
  `getByText(name)` passes on a refusal. Scope to `panel-assignment`.
- **Client-paged sections need `findRowAnyPage`.** `findRow` looks at the
  rendered page only; a section that was short enough yesterday silently stops
  finding its row once another test seeds a few more.

---

### Deviations from spec 30 §7, and why

- **§3 page objects.** Most screens are driven through the SHARED controls they
  are actually built from (`tableAccordion`, `dialogs`, `crud`) rather than one
  object per screen; a per-screen file exists only where a screen has logic of
  its own. Nine near-identical wrappers around `Section` would be indirection,
  not abstraction.
- **§7.5 "protocol types must match `protocolTypes.ts`".** That file no longer
  exists — the portal carries no local protocol list. The picker is fed by the
  Router's `ProviderCapabilityCatalog`, so the test compares the offered options
  against that catalog instead, and asserts a reserved (capability-less) entry is
  never offered.
- **§7.12 "revoke the token via the API mid-test".** The refresh grant is failed
  at the network boundary instead. Revoking the shared administrator's refresh
  token would invalidate the session every other test in the worker signs in
  with; the portal path under test (`handleRefreshToken` → auth error → restart
  login) is identical either way.
- **Not covered.** Branding logo upload and its reflection in the shell, the
  >500-geofence page drain, the report truncation notice (needs data), the
  document panel's version/signature surfaces, and per-feature sidenav proof for
  all six feature keys (one is proven; the rest need `E2E_CREATE_ACCOUNT`).

**A portal defect fails the run.** No test is pinned with `test.fail()` or
`test.fixme()`, and none asserts the broken behaviour "for now": every assertion
states what the screen is supposed to do, so a defect surfaces as a failure in
the Playwright report and nowhere else. A pinned test counts as passed in the
summary and paints green in the HTML report — the customer would be the one to
find it.
