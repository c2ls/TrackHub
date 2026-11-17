# Portal end-to-end suite (Playwright)

The regression gate for the portal. It drives the **real** portal in a real browser against the
**real** stack: it signs in through the actual OAuth flow, opens every screen, exercises each
screen's primary action, and checks that the result survives a reload.

Nothing is mocked. `page.route` is used only to *break* something on purpose — a failed `/health`
probe, a forced `FORBIDDEN`, a forced `ACCOUNT_SUSPENDED` — never to fake a success.

## Prerequisites

1. **The backend stack is up** — `TrackHub.Deployment\scripts\local\Start-TrackHubLocal.ps1`, after the
   one-time setup in [LOCAL-DOCKER.md](../../TrackHub.Deployment/LOCAL-DOCKER.md).
2. **The portal serves HTTPS** — `cert.crt` and `cert.key` in `TrackHub.Portal/` (the setup script
   installs them). Without them Vite falls back to HTTP and the OAuth callback fails.
3. **Browsers installed** — `npm run e2e:install`.

The suite starts the dev server itself (`npm start`) unless `E2E_REUSE_SERVER=1` says one is
already running.

## Running

```bash
npm run e2e                                      # the whole suite
npm run e2e -- e2e/specs/50-geofences.spec.ts    # one file
npm run e2e -- --project=chromium-es             # the Spanish pass only
npm run e2e -- -g "creates a unit"               # by test name
npm run e2e:headed                               # watch it
npm run e2e:ui                                   # the Playwright UI runner
npm run e2e:report                               # open the last HTML report
```

A failure leaves a trace, a screenshot and a video under `test-results/`
(`npx playwright show-trace test-results/<test>/trace.zip`).

## Configuration

Copy `e2e/.env.e2e.example` to `e2e/.env.e2e` (gitignored). **Every value is optional** — the
defaults drive the seeded local stack. Backend URLs are never set here: they come from the portal's
own `.env`/`.env.local` through Vite's `loadEnv`, so the tests talk to the services the app is
pointed at.

| Variable | Effect |
|---|---|
| `E2E_BASE_URL` | Portal origin. Defaults to the origin of `REACT_APP_CALLBACK_ENDPOINT`. |
| `E2E_ADMIN_EMAIL` / `E2E_ADMIN_PASSWORD` | The signing-in administrator. Defaults to the seeded `email@mail.com` / `12345678`. |
| `E2E_MANAGER_EMAIL` / `E2E_MANAGER_PASSWORD`, `E2E_USER_EMAIL` / `E2E_USER_PASSWORD` | Existing principals for the role-gate tests. Unset, setup creates both through the real screens and teardown removes them. |
| `E2E_REUSE_SERVER=1` | Use an already-running `npm start`. |
| `E2E_RUN_ID` | Override the run marker (normally the start time in base 36). |
| `E2E_SWEEP_ALL=1` | Let the teardown also sweep this run's own `e2e-*` rows. |

Opt-in tests, skipped unless their variable is set:

| Variable | Unskips | Why it is opt-in |
|---|---|---|
| `E2E_HAS_POSITIONS=1` (+ `E2E_POSITIONS_UNTIL`) | Dashboard history replay | Positions come from real GPS providers; `E2E_POSITIONS_UNTIL` ends the replay at the newest seeded point. |
| `E2E_GPS_OPERATOR_ID` | Manual GPS synchronization | A sync needs working provider credentials. |
| `E2E_TRIP_IN_TRANSIT_ID` | Pause/resume | The trip lifecycle is GPS-driven; a browser cannot move a truck out of a zone. |
| `E2E_CREATE_ACCOUNT=1` | The feature-gate test | Toggling a feature is account-wide; the master account is left alone by default. |
| `E2E_ALLOW_GEOCODING_SWITCH=1` | Activating a geocoding provider | The active provider is platform-wide. |
| `E2E_GOOGLE_MAPS=1` | Switching the account map provider | A Maps key not authorized for the portal origin breaks the map on every screen until the setting is put back. |
| `E2E_ALLOW_ROLE_MATRIX_WRITE=1` | Granting/revoking a role permission | A role grant widens what every holder may do; the policy matrix is exercised unconditionally instead. |
| `E2E_ALLOW_PASSWORD_CHANGE=1` | Changing the administrator's password | One-way unless `E2E_ADMIN_PASSWORD` meets the password policy — the seeded `12345678` does not, so it cannot be set back. |

## How sessions work

The portal keeps its access token in memory and its refresh token in React state, so a saved
storage state restores no session by itself. It does restore the AuthorityServer's cookie, which is
enough: the `/authorize` round trip completes without a login form and the callback mints a fresh
token pair.

- The `setup` project types credentials **once per role** and writes `e2e/.auth/{admin,manager,user}.json`.
  Nothing else types a password — `AuthContext.login()` allows 3 attempts per 30 s. Delete
  `e2e/.auth/` to force new sessions.
- **A deep link cannot be typed.** Every document load re-runs the OAuth round trip and the
  callback lands on `/dashboard`. `Shell.open(key)` lands there and then *clicks* the sidenav entry.
- `Shell.reloadTo(key)` asks "did it really persist?": it reloads, dropping the in-memory token, and
  comes back through the dashboard.

## Test data

Everything a test creates carries `e2e-<runId>-<n>` in its human-readable field, so a run finds
exactly its own rows and a row's age is derivable from its name.

- Each test registers the undo for a row as soon as the row exists (`fixtures/data.ts`), so a later
  failure still releases it.
- The global teardown removes the principals setup created and sweeps `e2e-*` rows left by crashed
  runs (`fixtures/sweep.ts`).
- Trips are released with **delete → abort → cancel**: delete is refused once a trip has history,
  abort is refused from `Created`, and cancel retires a queued trip that already has events. A
  leftover `Created` trip silently takes its unit's arming slot.
- **A cancelled trip's row is permanent** (`TRIP_HAS_HISTORY` on every delete). The cancel test
  leaves one `Cancelled` row behind; it is inert.

## Layout

```
e2e/
  .env.e2e.example     template (.env.e2e is gitignored)
  fixtures/            env, real sign-in, API seeding/cleanup client, names + cleanup registry,
                       sweep, i18n, status-page services, merged `test` export
  pages/               shell, shared section control, dialogs, CRUD flow, and the screens with
                       logic of their own (dashboard, geofences, reports, trips, profile)
  setup/auth.setup.ts  the `setup` project
  specs/               one file per screen; specs/support/ holds helpers
  global-teardown.ts
```

## Adding a screen

1. **Give its sections a key.** `sectionKey="…"` on `TableAccordion` renders
   `data-testid="section-<key>"`, `-header` and `-add`. The shared dialogs already carry
   `dialog-form`, `dialog-confirm`, `dialog-message`, `dialog-dynamic-table`, `dialog-help`; table rows
   carry `row-<entityId>`.
2. **Name the test as a requirement**: *"a manager creates a unit and sees it in the fleet list
   after reload"*.
3. **Select by role and localized name** — `getByRole('button', { name: t('generic.add') })`. The `t`
   fixture follows the project's language, so one body runs in `chromium` and `chromium-es`.
4. **Assert the visible outcome and the reload** (`CrudFlow.reload(shell, screen)`); a closed dialog
   proves nothing was stored.
5. **Register the cleanup as soon as the row exists**, with an API fallback for rows the UI only
   deactivates (staff, notification rules, document types, toll classes).
6. **Never wait on network idle.** Wait on the UI state you expect.

### MUI specifics

- `Select` opens a listbox in a portal (`page.getByRole('option', …)`); its first option is a disabled
  placeholder, which `FormDialog.selectFirst(id)` skips.
- `Switch` has role `switch`, not `checkbox`.
- Icon-only buttons have no accessible name (MUI marks `Icon` `aria-hidden`), hence the pagination
  `data-testid`s.
- Section tables page ten rows at a time in the browser; `Section.findRowAnyPage` walks them —
  `findRow` sees the rendered page only.
- The configurator drawer is always mounted, so a page-wide `getByRole('button', { name: 'Save' })`
  can match it. Scope to the card.
- Saves confirm through the app's snackbar (`getByRole('alert')`).

### Preconditions that are easy to get wrong

- **Units are group-scoped.** Trip unit pickers are fed by `transporterLookupByUser`, so a unit
  created through the API without group membership is never offered. Use `api.visibleTransporters()`.
- **A staff member is assignable only to a trip on a unit it is linked to**; anything else is
  `TRIP_STAFF_NOT_ASSIGNABLE`. Seed with `api.createStaffMember(name, transporterId)` and plan the
  trip on that unit.
- **Assert inside the panel, not the page.** A picker keeps showing what it selected whether or not
  the write was accepted; scope to `panel-assignment`.

**A portal defect fails the run.** No test is pinned with `test.fail()` or `test.fixme()`, and none
asserts broken behaviour "for now" — a pinned test counts as passed and paints the report green.
