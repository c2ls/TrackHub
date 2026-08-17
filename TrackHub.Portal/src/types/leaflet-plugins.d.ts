/**
 * Module augmentation for the two Leaflet plugins the Maps stack uses that ship
 * no types: `leaflet-editable` (interactive polygon drawing/editing in the
 * geofence editors) and `leaflet-fullscreen` (the OSM fullscreen control).
 *
 * This file imports `leaflet`, making it a module, so `declare module 'leaflet'`
 * *merges* with @types/leaflet rather than replacing it. Only the members the
 * Maps controls actually call are declared.
 */
import 'leaflet';

declare module 'leaflet' {
  // --- leaflet-editable ---
  class Editable {
    constructor(map: Map, options?: unknown);
    startPolygon(latlng?: LatLng): Polygon;
    startCircle(latlng?: LatLng): Circle;
    stopDrawing(): void;
  }

  interface Map {
    editTools: Editable;
  }

  interface Polygon {
    enableEdit(map?: Map): this;
    disableEdit(): this;
    editEnabled(): boolean;
  }

  interface Circle {
    enableEdit(map?: Map): this;
    disableEdit(): this;
    editEnabled(): boolean;
  }

  // --- leaflet-fullscreen ---
  namespace Control {
    interface FullscreenOptions extends ControlOptions {
      title?: string | { false: string; true: string };
      forceSeparateButton?: boolean;
      content?: string | null;
    }
    class Fullscreen extends Control {
      constructor(options?: FullscreenOptions);
    }
  }

  namespace control {
    function fullscreen(options?: Control.FullscreenOptions): Control.Fullscreen;
  }
}
