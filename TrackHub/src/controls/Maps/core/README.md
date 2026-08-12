# Map core abstraction (`controls/Maps/core`)

Provider-agnostic contract for map feature layers, introduced as a strangler
over the duplicated OSM/Leaflet and Google component pairs. **All new map
layers are written once against this contract**; the existing
`GeneralMap`/`TripsMap` components keep their behavior and simply render these
layers inside the current OSM/Google map hosts.

## Contract

- `MapProviderContext` — React context carrying `{ provider: 'OSM' | 'Google' }`.
  The map host components (`OSMClusteredMap`, `GoogleClusteredMap`,
  `OSMTripsMap`, `GoogleTripsMap`) wrap their children with the stable
  `OSM_PROVIDER` / `GOOGLE_PROVIDER` values. Use the `useMapProvider()` hook to
  read the active provider.
- Feature layers (`PoiLayer`, `TrailLayer`, `PlaybackMarker`) are thin
  dispatchers: `index.js` reads the provider from context and renders the
  matching adapter (`OSM.js` or `Google.js`). Consumers only ever import the
  dispatcher, never an adapter.

### Layers

| Layer | Props | Behavior |
|---|---|---|
| `PoiLayer` | `pois: PointOfInterestVm[]` | Colored SVG pin per POI (color per `data/colors.js` convention), name tooltip, popup with name/description/address/type label. |
| `TrailLayer` | `points: {lat, lng}[]`, `color?` | Dashed polyline of the last N received points for the selected unit. |
| `PlaybackMarker` | `position: {lat, lng, course?}` | Single marker positioned imperatively per playback tick (no React re-mount per frame). |

## Adding a provider adapter (e.g. MapLibre GL)

1. Add the provider name to `MAP_PROVIDERS` and a stable context value in
   `MapProviderContext.js`.
2. Create the map host component that renders the base map and wraps its
   children in `<MapProviderContext.Provider value={MAPLIBRE_PROVIDER}>`.
3. For each layer directory, add a `MapLibre.js` adapter implementing the same
   props, and extend the dispatcher `index.js` with the new provider case.
4. Feature code (dashboard, replay, POIs) does not change — it keeps rendering
   the dispatcher components.

Adapters must be side-effect-clean: create native objects in `useEffect`,
remove them in the cleanup, and never leak listeners between renders.
