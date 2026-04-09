# TrackHubMobile — Fix Navigation & Complete Features

## Context

TrackHubMobile is a **.NET MAUI Blazor Hybrid** application targeting **net10.0** (iOS, Android, macOS Catalyst). It is a companion mobile app for the TrackHub GPS/fleet tracking platform.

### Solution Structure

```
TrackHubMobile.sln
├── TrackHubMobile/              # MAUI host project (platform heads, wwwroot, App.xaml)
│   ├── Views/MainPage.xaml      # Single-page host with BlazorWebView
│   ├── Shared/MainLayout.razor  # Blazor layout with sidebar NavMenu
│   ├── Main.razor               # Blazor Router (uses Microsoft.AspNetCore.Components.Routing.Router)
│   └── wwwroot/                 # Static assets: index.html, scripts/map.js, CSS, Leaflet CDN
│
└── TrackHubMobile.MauiLib/      # Razor Class Library (all business logic lives here)
    ├── Pages/                   # Routable Razor pages (Home, TransporterList, TransporterMap)
    ├── Shared/                  # Shared Razor components (NavMenu, TransporterDetail)
    ├── ViewModels/              # MVVM ViewModels using CommunityToolkit.Mvvm
    ├── Models/                  # PositionVm, AttributesVm (readonly record structs)
    ├── Services/                # Authentication (PKCE/OAuth2), Router (GraphQL), DataRefresh, Storage
    ├── Helpers/                 # GraphQLReader, TransporterHelper, LocalizationResourceManager, ToastDisplay
    ├── Interfaces/              # Service + Helper contracts
    ├── Messages/                # WeakReferenceMessenger messages (DataRefreshedMessage, ToastMessage)
    ├── Utils/                   # Constants (endpoints, OAuth config)
    └── Resources/               # AppResources.resx (EN) + AppResources.es.resx (ES)
```

### Technology Stack

| Component | Technology |
|---|---|
| Framework | .NET 10 MAUI Blazor Hybrid |
| UI Layer | Razor Components (Blazor) rendered in a native `BlazorWebView` |
| MVVM | CommunityToolkit.Mvvm (`ObservableObject`, `[ObservableProperty]`, `WeakReferenceMessenger`) |
| API | GraphQL via custom `GraphQLReader` helper with JWT Bearer auth |
| Auth | OAuth2 + PKCE via `WebAuthenticator`, tokens stored in `SecureStorage` |
| Maps | Leaflet 1.9.4 (loaded from CDN in `index.html`), interop via `IJSRuntime` |
| i18n | `.resx` resource files accessed through `ILocalizationResourceManager` (injected as `LRM`) |
| Styling | Bootstrap 5 + Font Awesome + scoped `.razor.css` files |
| Messaging | `WeakReferenceMessenger` for cross-component data refresh notifications |

### Key Patterns Already in Place

1. **ViewModel injection via primary constructors** — ViewModels are registered as singletons in DI and injected into Razor code-behind via `@inject` or constructor parameters.
2. **ActiveScreenComponentBase** — base class for pages that need auto-refresh; manages `IDataRefresh.SetScreenActive()` and listens for `NavigationManager.LocationChanged`.
3. **DataRefresh service** — runs a `Timer` that ticks every 5 seconds; every 6th tick (30s) fetches positions via `IRouter.GetDevicePositionsByUserAsync()` and broadcasts a `DataRefreshedMessage`.
4. **Separation of concerns** — Razor files contain only markup; code-behind (`.razor.cs`) handles UI logic; ViewModels handle state and data operations; Services handle external communication.
5. **Localization** — all user-facing strings use `@LRM["Key"]` in Razor and `localization["Key"]` in C#; keys are defined in `AppResources.resx` (EN) and `AppResources.es.resx` (ES).

### Data Models

```csharp
// Models/PositionVm.cs
public readonly record struct PositionVm(
    Guid TransporterId, string DeviceName, string TransporterType,
    double Latitude, double Longitude, double? Altitude,
    DateTimeOffset DeviceDateTime, double Speed, double? Course,
    int? EventId, string? Address, string? City, string? State,
    string? Country, AttributesVm? Attributes);

// Models/AttributesVm.cs
public readonly record struct AttributesVm(
    bool? Ignition, int? Satellites, double? Mileage,
    double? Hourmeter, double? Temperature);
```

### GraphQL Queries Available

1. **`devicePositionsByUser`** — returns list positions (lightweight: name, type, speed, id, dateTime, lat, lng).
2. **`devicePositionByTransporter(transporterId)`** — returns single position with full details (address, city, state, country, altitude, course, eventId, attributes).

### Current Routes

| Route | Page | Status |
|---|---|---|
| `/` | `Home.razor` (Dashboard) | Works but hardcoded English labels |
| `/listview` | `TransporterList.razor` | Works, basic table |
| `/mapview` | `TransporterMap.razor` | Stub — only shows hardcoded markers |

---

## Tasks (Ordered by Priority)

### 1. Fix DataRefresh Service (Critical — prerequisite for all data-driven features)

**Current state**: `DataRefresh.cs` uses a `System.Threading.Timer` that ticks every **5 seconds** but only actually fetches data every 6th tick (30s). This design has several issues that must be fixed.

**Problems to fix**:

| Issue | Detail |
|---|---|
| **Wasteful timer** | Timer fires every 5s just to increment a counter. Use `TimeSpan.FromSeconds(30)` as the period directly and remove the counter. |
| **Fire-and-forget async** | `Tick()` calls `_ = TickAsync()`, discarding the `Task`. If `RefreshDataAsync` throws after the `catch`, it's silently lost. |
| **No reentrancy guard** | If a GraphQL request takes >30s (slow/unstable mobile network), the next tick starts a second concurrent request. Overlapping requests waste bandwidth, can cause race conditions on the `Transporters` property, and may hammer the server. Add a `SemaphoreSlim(1,1)` or a simple `bool _isRefreshing` flag to skip ticks while a request is in flight. |
| **Thread-unsafe counter** | `_counter++` is accessed from the `ThreadPool` timer callback without synchronization. With the timer simplification this goes away, but any remaining shared state should use `Interlocked` or a lock. |
| **Shared mutable state** | `Transporters` is a public settable property modified by `DataRefresh` internally AND externally by `TransporterListViewModel` and `TransporterMapViewModel` (`dataRefresh.Transporters = Transporters`). This creates a race: a manual page refresh can overwrite data right before the timer broadcasts. `DataRefresh` should own the data exclusively; pages should read from the messenger broadcast. |
| **Missing cancellation check** | `RefreshDataAsync` doesn't check `cancellationToken.IsCancellationRequested` before starting the HTTP call. When the screen is deactivated and the token is cancelled, an in-flight request may still complete and broadcast stale data. |
| **DisposeAsync is not awaitable** | The method is marked `async` implicitly via `ValueTask` but doesn't `await` anything and doesn't ensure a running timer callback has completed before disposal. Use `Timer.DisposeAsync()` (.NET 8+) or wait on a completion signal. |

**Refactored approach** (keep it simple, no webhooks):

```
Timer period = 30 seconds (configurable)
On tick:
  1. If _isRefreshing → skip (reentrancy guard)
  2. Set _isRefreshing = true
  3. Try: fetch positions, broadcast via messenger
  4. Finally: _isRefreshing = false
```

**Acceptance criteria**:
- Timer fires directly at the desired interval (no counter arithmetic).
- Only one HTTP request is in flight at a time.
- Cancellation is respected — when the screen or app goes inactive, any in-flight request is cancelled and no broadcast occurs.
- `Transporters` property is only written by `DataRefresh`; remove external setters from `TransporterListViewModel` and `TransporterMapViewModel`.
- The service disposes cleanly without orphaned callbacks.

---

### 2. Fix Post-Login Navigation & Side Menu (Critical)

**Problem**: After migrating from .NET 8 to .NET 10, the login flow completes successfully (OAuth2 PKCE tokens are obtained and stored) but the `BlazorWebView` does not navigate from the login state to the main page (`/`). Additionally, the `NavMenu` sidebar links (`href=""`, `href="mapview"`, `href="listview"`) do not trigger page navigation.

**Investigation hints**:
- `MainPage.xaml` binds `StartPath="{Binding StartPath}"` but `MainViewModel` (which is the `BindingContext`) extends `BaseViewModel("Home")` and has no `StartPath` property — this likely resolves to `null` or empty, which may have worked in .NET 8 but may not in .NET 10.
- `Main.razor` uses `<Router AppAssembly="@GetType().Assembly">` — since pages live in the **separate** `TrackHubMobile.MauiLib` assembly, the router may not discover them. In .NET 10, `AdditionalAssemblies` may be required.
- The `NavMenu` links use `<NavLink href="mapview">` but pages are decorated with `@page "/mapview"` (with leading slash). Verify the Blazor router resolution.
- After login, `MainViewModel.InitializeAsync()` calls `authService.LoginAsync()` which returns after tokens are stored, but nothing triggers a navigation or page re-render.

**Acceptance criteria**:
- After successful OAuth login, the app automatically navigates to the Dashboard (`/`).
- Tapping NavMenu items (`Home`, `Map View`, `List View`) navigates to the corresponding page.
- Logout returns to the login state.
- Navigation works consistently on both Android and iOS.

---

### 3. Complete the Map Page (`/mapview`)

**Current state**: `TransporterMap.razor` renders `<div id="map">` and the code-behind calls `JS.InvokeVoidAsync("initMap", positions)` with two hardcoded positions. `map.js` creates a basic Leaflet map with simple markers.

**Required implementation**:

#### 3a. Map ViewModel Integration
- Connect `TransporterMapViewModel` to the page (it already has `RefreshDataAsync` and `Transporters` property but is not used).
- The page should inherit `ActiveScreenComponentBase` to participate in the `DataRefresh` auto-refresh cycle (every 30 seconds).
- Also listen for `DataRefreshedMessage` to update markers in real time.

#### 3b. Leaflet JS Interop (`wwwroot/scripts/map.js`)
Rewrite `map.js` as a proper module with the following functions callable from C#:

| JS Function | Purpose |
|---|---|
| `initMap(positions)` | Initialize the Leaflet map, add tile layer, render initial markers with clustering |
| `updateMarkers(positions)` | Clear existing markers and re-add with updated positions (for auto-refresh) |
| `focusSingleUnit(position)` | Center map on a single unit and open its popup (used from Units list action) |
| `destroyMap()` | Clean up map instance and event listeners (called on page dispose) |

Each position passed to JS should include: `{ lat, lng, name, speed, dateTime, transporterType, course, address, city, state, ignition, transporterId }`.

#### 3c. Marker Clustering
- Add Leaflet.markercluster plugin (CDN: `https://unpkg.com/leaflet.markercluster@1.5.3/dist/`). Add both the JS and CSS to `index.html`.
- Use `L.markerClusterGroup()` to group nearby markers.

#### 3d. Info Popups
Each marker popup should display:
- Unit name (bold)
- Transporter type
- Speed (Km/h)
- Last report time (use relative format: "5 minutes ago")
- Address (if available)
- Ignition status (if available)

#### 3e. Auto-Fit Bounds
- On initial load and when markers change, fit the map bounds to show all markers.
- If only one marker exists, center on it at a reasonable zoom level.

#### 3f. Marker Icons
- Use directional markers that rotate based on `course` value.
- Use different colors for moving (green) vs stopped (red) vs offline (gray — no report in 2+ hours).

**File organization**:
- `wwwroot/scripts/map.js` — all Leaflet interop functions
- `Pages/TransporterMap.razor` — markup only (map container + optional loading indicator)
- `Pages/TransporterMap.razor.cs` — Blazor lifecycle, JS interop calls, ViewModel binding
- `ViewModels/TransporterMapViewModel.cs` — data fetching and state

---

### 4. Enhance the Units List (`/listview`)

**Current state**: Shows a basic HTML table with Name, Last Update, Speed. Selecting a row renders `TransporterDetail` component below. No actions are available.

**Required implementation**:

#### 4a. Expandable Row Design
Replace the split-container layout with an expandable row pattern:
- Tapping a row expands it inline to show additional details and action buttons.
- Tapping again or tapping another row collapses it.
- The expanded section should show:
  - Full address (address, city, state, country)
  - Transporter type
  - Last report date/time (full format)
  - Speed with unit
  - Ignition status, mileage, temperature (from attributes — fetched via `GetDeviceAsync`)

#### 4b. Action Buttons (in expanded row)
Each expanded row should include these action buttons:

| Action | Behavior |
|---|---|
| 📍 Open in Google Maps | Open device coordinates in Google Maps via deep link: `https://www.google.com/maps?q={lat},{lng}` using `Browser.Default.OpenAsync()` |
| 🗺️ Show on Map | Navigate to `/mapview?transporterId={id}` — the map page should accept this query parameter and focus on the single unit |
| 📤 Share via WhatsApp | Share location via WhatsApp deep link: `https://wa.me/?text=...` with unit name, coordinates, Google Maps link, and timestamp |
| 📋 View Details | Navigate to a new page `/transporter/{transporterId}` that shows all available unit information in a dedicated full-screen view |

#### 4c. New TransporterDetailPage (`/transporter/{transporterId}`)
Create a new routable page (separate from the existing `TransporterDetail` shared component):
- Full-screen dedicated view for a single unit
- Fetches complete data via `IRouter.GetDeviceAsync(transporterId)`
- Displays all fields: name, type, coordinates, altitude, speed, course, address, city, state, country, event, datetime, and all attributes
- Include the same action buttons (Google Maps, WhatsApp share)
- Include a mini-map showing the unit's position
- Back navigation button

#### 4d. Search/Filter
- Add a search box at the top of the list to filter units by name.
- The filter should work client-side on the already-loaded data.

**File organization**:
- `Pages/TransporterList.razor` + `.razor.cs` — list with expandable rows
- `Pages/TransporterDetailPage.razor` + `.razor.cs` — new full detail page (route: `/transporter/{transporterId}`)
- `Shared/TransporterDetail.razor` + `.razor.cs` — reusable detail component (keep existing, enhance)
- `Shared/TransporterActions.razor` + `.razor.cs` — reusable action buttons component
- `ViewModels/TransporterListViewModel.cs` — add search filter logic
- `ViewModels/TransporterDetailViewModel.cs` — existing, reuse

---

### 5. Enhance the Dashboard (`/`)

**Current state**: Shows 4 cards (Total, In Movement, Offline, Speeding) with hardcoded English labels and basic counts from `HomeViewModel`.

**Required enhancements**:

#### 5a. Localize All Labels
Replace hardcoded strings with `@LRM["Key"]` references. Add these keys to both `.resx` files:
- `Dashboard`, `TotalUnits`, `UnitsInMovement`, `UnitsOffline`, `UnitsSpeeding`
- `ActiveUnits`, `UnitsWithIgnitionOn`, `StoppedUnits`

#### 5b. Additional Metrics
Add these derived metrics (all computable from the existing `PositionVm` collection):

| Metric | Calculation |
|---|---|
| Active Units | Units with `DeviceDateTime` within the last 30 minutes |
| Stopped Units | Units with `Speed == 0` and `DeviceDateTime` within last 30 minutes (active but not moving) |
| Ignition On | Units where `Attributes?.Ignition == true` |
| Avg Speed (moving) | Average speed of units where `Speed > 0` |
| Unit Type Breakdown | Count by `TransporterType` (e.g., "Car: 5, Truck: 3") |

#### 5c. Dashboard Layout
- Use a responsive card grid (2 columns on mobile).
- Each card should have an icon, a label, and a large number.
- Use color coding: green for positive/active, red for alerts (speeding, offline), blue for neutral.
- Add the navigation buttons at the bottom (already exist but need localization).

#### 5d. Pull-to-Refresh
- Allow the user to manually trigger a data refresh from the Dashboard.

**File organization**:
- `Pages/Home.razor` + `.razor.cs` — dashboard layout
- `ViewModels/HomeViewModel.cs` — add new computed properties

---

### 6. Add About Page

Create a new tab/page for "About" information.

#### 6a. Route and Navigation
- Route: `/about`
- Add to `NavMenu.razor` with appropriate icon and localized label (`@LRM["About"]`)
- Add `About` key to both `.resx` files

#### 6b. Content
- App name: "TrackHubMobile"
- Version: read from `IAppInfo.VersionString` and `IAppInfo.BuildString`
- Description: brief app description (localized)
- Author / Copyright notice
- Links: project repository, license info
- Technology credits (Leaflet, .NET MAUI, etc.)
- Device info: `IAppInfo.PackageName`, platform, OS version

**File organization**:
- `Pages/About.razor` + `.razor.cs`
- `ViewModels/AboutViewModel.cs`

---

## Architecture Rules

1. **No business logic in Razor files** — Razor files (`.razor`) contain only markup and `@inject` directives. All event handlers, data transformations, and state management go in the code-behind (`.razor.cs`) or the ViewModel.

2. **Separate pages and components** — Create dedicated `.razor` files for distinct UI sections. Do not build monolithic views. Use `Shared/` components for reusable pieces.

3. **ViewModel per page** — Each page gets its own ViewModel. Shared/reusable ViewModels are acceptable for shared components. Register all ViewModels in `MauiProgram.cs`.

4. **Follow existing multilingual pattern** — Every user-facing string must use `@LRM["Key"]` in Razor or `localization["Key"]` in C#. Add keys to **both** `AppResources.resx` and `AppResources.es.resx`.

5. **DI registration** — All new services, ViewModels, and helpers must be registered in `MauiProgram.cs`. Use `AddSingleton` for stateful services and ViewModels; use `AddTransient` for pages that need fresh `NavigationManager` instances per navigation.

6. **File size discipline** — No single `.razor`, `.cs`, or `.js` file should exceed ~200 lines. Split into components, partials, or helper functions.

7. **JS interop** — Keep JavaScript in `wwwroot/scripts/`. Use `IJSRuntime.InvokeVoidAsync` / `InvokeAsync<T>` for C# → JS calls. Use `[JSInvokable]` static methods for JS → C# callbacks if needed.

8. **Dark/Light theme** — Respect `IAppInfo.RequestedTheme` for dynamic styling (existing pattern uses `AppTheme.Dark` switch in code-behind).

9. **Error handling** — Use `try/catch` in ViewModels for service calls. Display errors via `WeakReferenceMessenger` + `ToastMessage` (existing pattern in `DataRefresh` and `GraphQLReader`).

10. **Dispose pattern** — Pages that register event handlers or timers must implement `IDisposable` and clean up in `Dispose()` (existing pattern in `ActiveScreenComponentBase`).
