/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
*
*  Licensed under the Apache License, Version 2.0 (the "License").
*  You may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
*/

import { MAP_DEFAULTS } from 'api/core/endpoints';
import { useRef, useState, useEffect } from "react";
import type { RefObject } from "react";
import { MapContainer, TileLayer, Polygon, Circle } from "react-leaflet";
import UserLocation from "controls/Maps/UserLocation";
import { OSMScaleControl } from 'controls/Maps/shared/ScaleControl';
import { OSMFullscreenControl } from 'controls/Maps/shared/FullscreenControl';
import { OSMMeasurementTool } from 'controls/Maps/shared/MeasurementTool';
import { OSM_LIGHT_TILE, OSM_DARK_TILE } from 'controls/Maps/utils/darkMapStyles';
import { MAP_CONTAINER_CLASS, DEFAULT_OSM_ZOOM, GEOFENCE_FIT_PADDING } from 'controls/Maps/core/mapConfig';
import { circleBoundsCorners } from 'controls/Maps/utils/coordinateUtils';
import L from "leaflet";
import "leaflet/dist/leaflet.css";
import "leaflet-editable";
import 'controls/Maps/css/map.css';
import { v4 as uuidv4 } from "uuid";
import type {
  GeofenceShape,
  MapPolygon,
  GeofenceShapeHandle,
  AddGeofenceHandler,
  AddCircleGeofenceHandler,
  SaveGeofenceHandler,
  CancelGeofenceHandler,
  EditGeofenceHandler,
  RemoveGeofenceHandler
} from 'layouts/geofencemanager/components/GeofenceEditor';

/**
 * A geofence held in editor state. This is the single source of truth: it is
 * rendered by a declarative react-leaflet `<Polygon>`/`<Circle>`, and the
 * underlying leaflet layer instance (captured via that component's `ref`, kept
 * in {@link layersRef}) is what leaflet-editable drives when editing.
 */
interface EditorGeofence {
  id: string;
  name: string;
  shape: GeofenceShape;
  latlngs: [number, number][];
  center?: [number, number];
  radiusMeters?: number;
}

interface OSMGeofenceEditorProps {
    initialPolygons?: MapPolygon[];
    selectedPolygon?: string | null;
    handleSelected: (value: string | null) => void;
    setOpen: (open: boolean) => void;
    setIsEditing: (editing: boolean) => void;
    addRef?: RefObject<AddGeofenceHandler | null>;
    addCircleRef?: RefObject<AddCircleGeofenceHandler | null>;
    saveRef?: RefObject<SaveGeofenceHandler | null>;
    cancelRef?: RefObject<CancelGeofenceHandler | null>;
    editingRef?: RefObject<EditGeofenceHandler | null>;
    removeRef?: RefObject<RemoveGeofenceHandler | null>;
    enableScale?: boolean;
    enableFullscreen?: boolean;
    enableMeasurement?: boolean;
    darkMode?: boolean;
    height?: string;
}

const OSMGeofenceEditor = ({
    initialPolygons,
    selectedPolygon,
    handleSelected,
    setOpen,
    setIsEditing,
    addRef,
    addCircleRef,
    saveRef,
    cancelRef,
    editingRef,
    removeRef,
    enableScale = true,
    enableFullscreen = true,
    enableMeasurement = true,
    darkMode = false,
    height = '70vh'
}: OSMGeofenceEditorProps) => {
  const mapRef = useRef<L.Map | null>(null);
  // Underlying leaflet layer instance for each rendered geofence, keyed by id,
  // captured from the declarative <Polygon>/<Circle> `ref`. This is what
  // leaflet-editable's enableEdit/disableEdit operate on — there is no second,
  // imperative copy of a geofence layer.
  const layersRef = useRef<Map<string, L.Polygon | L.Circle>>(new Map());
  // The transient layer that leaflet-editable creates while the user draws a NEW
  // shape (before it is committed to `geofences` state). Removed after save/cancel.
  const drawingLayerRef = useRef<L.Polygon | L.Circle | null>(null);
  const editedGeofenceRef = useRef<string | null>(null);
  // Guards the one-time "fit the viewport to the account's geofences" pass so a
  // refetch/re-render never yanks the map back off the area the user panned to.
  const didFitRef = useRef(false);
  const [geofences, setGeofences] = useState<EditorGeofence[]>([]);
  const [userLocation, setUserLocation] = useState({
    lat: MAP_DEFAULTS.lat,
    lng: MAP_DEFAULTS.lng
  });

  const registerLayer = (id: string, instance: L.Polygon | L.Circle | null) => {
    if (instance) {
      layersRef.current.set(id, instance);
    } else {
      layersRef.current.delete(id);
    }
  };

  useEffect(() => {
    if (!mapRef.current) return;

    const map = mapRef.current;
    if (map) {
      map.editTools = new L.Editable(map);

      map.on("editable:drawing:commit", (e) => {
        const { layer } = e as L.LeafletEvent & { layer: L.Polygon | L.Circle };
        drawingLayerRef.current = layer;
        setOpen(true);
      });
    }
    return () => {
      if (map) {
        map.off("editable:drawing:commit");
      }
    };
  }, [mapRef.current]);

  useEffect(() => {
    if (!selectedPolygon) return;
    const geofence = geofences.find((geofence) => geofence.name === selectedPolygon);
    // The selected geofence may not be in this editor's local set yet (the sync
    // effect below has not populated it). Silently no-op in that case.
    if (!geofence) return;
    selectPolygon(geofence.id);
  }, [selectedPolygon]);

  // Re-sync the editor's geofences from the source list. Runs on the initial
  // load and again whenever the source changes (e.g. a mutation refetched an
  // edited radius), but only while idle: an in-progress draw (drawingLayerRef)
  // or edit (editedGeofenceRef) is never wiped. With a single declarative source
  // this is just a state replace — react-leaflet reconciles the layers by key.
  useEffect(() => {
    if (!mapRef.current) return;
    if (editedGeofenceRef.current !== null || drawingLayerRef.current !== null) return;

    setGeofences((initialPolygons ?? []).map((polygon): EditorGeofence => {
      if (polygon.shape === 'circle' && polygon.center && polygon.radiusMeters) {
        return {
          id: polygon.id, name: polygon.name, shape: 'circle', latlngs: [],
          center: polygon.center, radiusMeters: polygon.radiusMeters,
        };
      }
      return { id: polygon.id, name: polygon.name, shape: 'polygon', latlngs: polygon.latlngs };
    }));
  }, [initialPolygons, mapRef.current]);

  // Fit the viewport to the combined bounds of the loaded geofences the first
  // time a non-empty set arrives. Runs once (didFitRef), never while the user is
  // drawing/editing, and leaves the MAP_DEFAULTS center untouched when empty.
  useEffect(() => {
    const map = mapRef.current;
    if (!map || didFitRef.current) return;
    if (editedGeofenceRef.current !== null || drawingLayerRef.current !== null) return;
    const polygons = initialPolygons ?? [];
    if (polygons.length === 0) return;

    const bounds = L.latLngBounds([]);
    polygons.forEach((polygon) => {
      if (polygon.shape === 'circle' && polygon.center && polygon.radiusMeters) {
        const [sw, ne] = circleBoundsCorners(polygon.center[0], polygon.center[1], polygon.radiusMeters);
        bounds.extend(sw);
        bounds.extend(ne);
      } else {
        polygon.latlngs.forEach((latlng) => bounds.extend(latlng));
      }
    });

    if (bounds.isValid()) {
      map.fitBounds(bounds, { padding: [GEOFENCE_FIT_PADDING, GEOFENCE_FIT_PADDING] });
      didFitRef.current = true;
    }
  }, [initialPolygons, mapRef.current]);

  const handlePolygonClick = (id: string) => {
    selectPolygon(id);
  };

  const selectPolygon = (id: string) => {
    const geofence = geofences.find((geofence) => geofence.id === id);
    if (!geofence) return;
    const layer = layersRef.current.get(id);
    if (!layer) return;
    if (layer.editEnabled()) {
      setIsEditing(false);
      layer.disableEdit();
      editedGeofenceRef.current = null;
    } else {
      disableEditing();
      setIsEditing(true);
      layer.enableEdit();
      editedGeofenceRef.current = id;
      handleSelected(geofence.name);
    }
  };

  const disableEditing = () => {
    if (editedGeofenceRef.current) {
      const layer = layersRef.current.get(editedGeofenceRef.current);
      if (layer) {
        layer.disableEdit();
      }
      editedGeofenceRef.current = null;
    }
  };

  const endEditing = (): GeofenceShapeHandle | undefined => {
    const map = mapRef.current;
    if (!map) return;
    const geofence = geofences.find((geofence) => geofence.id === editedGeofenceRef.current);
    let handle: GeofenceShapeHandle | undefined;
    if (geofence) {
      const layer = layersRef.current.get(geofence.id);
      if (geofence.shape === 'circle' && layer instanceof L.Circle) {
        const center = layer.getLatLng();
        const radius = layer.getRadius();
        layer.disableEdit();
        // Push the edited geometry back into state so the declarative <Circle>
        // stays in sync (react-leaflet updates the layer to match).
        setGeofences((prev) => prev.map((g) => g.id === geofence.id
          ? { ...g, center: [center.lat, center.lng], radiusMeters: radius }
          : g));
        setIsEditing(false);
        handle = { id: geofence.id, shape: 'circle', latlngs: [], center: { lat: center.lat, lng: center.lng }, radiusMeters: radius };
      } else if (layer instanceof L.Polygon) {
        const coordinates = layer.getLatLngs()[0] as L.LatLng[];
        layer.disableEdit();
        // Ensure the polygon ring is closed (first === last).
        const closed = [...coordinates];
        if (closed.length > 0 && !closed[0].equals(closed[closed.length - 1])) {
          closed.push(closed[0]);
        }
        setGeofences((prev) => prev.map((g) => g.id === geofence.id
          ? { ...g, latlngs: closed.map((c): [number, number] => [c.lat, c.lng]) }
          : g));
        setIsEditing(false);
        handle = { id: geofence.id, shape: 'polygon', latlngs: closed.map(c => ({ lat: c.lat, lng: c.lng })) };
      }
    }

    map.editTools.stopDrawing();
    editedGeofenceRef.current = null;
    return handle;
  };

  const add = () => {
    const map = mapRef.current!;
    map.editTools.startPolygon();
    disableEditing();
    setIsEditing(false);
  };

  const addCircle = () => {
    const map = mapRef.current!;
    map.editTools.startCircle();
    disableEditing();
    setIsEditing(false);
  };

  const save = (name: string): GeofenceShapeHandle => {
    const map = mapRef.current!;
    const layer = drawingLayerRef.current!;
    const id = uuidv4();
    if (layer instanceof L.Circle) {
      const center = layer.getLatLng();
      const radius = layer.getRadius();
      // The drawing layer was created imperatively by leaflet-editable; drop it
      // now that the committed geofence is rendered declaratively below.
      map.editTools.stopDrawing();
      layer.disableEdit();
      map.removeLayer(layer);
      drawingLayerRef.current = null;
      setGeofences((prev) => [...prev, {
        id, name, shape: 'circle', latlngs: [], center: [center.lat, center.lng], radiusMeters: radius
      }]);
      setOpen(false);
      return { id, shape: 'circle', latlngs: [], center: { lat: center.lat, lng: center.lng }, radiusMeters: radius };
    }
    const latlngs = (layer as L.Polygon).getLatLngs()[0] as L.LatLng[];
    // Ensure the polygon is closed
    const closed = [...latlngs];
    if (closed.length > 0 && !closed[0].equals(closed[closed.length - 1])) {
      closed.push(closed[0]);
    }
    map.editTools.stopDrawing();
    layer.disableEdit();
    map.removeLayer(layer);
    drawingLayerRef.current = null;
    setGeofences((prev) => [...prev, {
      id, name, shape: 'polygon', latlngs: closed.map((c): [number, number] => [c.lat, c.lng])
    }]);
    setOpen(false);
    return { id, shape: 'polygon', latlngs: closed.map(c => ({ lat: c.lat, lng: c.lng })) };
  };

  const cancel = () => {
    const map = mapRef.current!;
    if (drawingLayerRef.current) {
      map.removeLayer(drawingLayerRef.current);
    }
    drawingLayerRef.current = null;
  };

  const remove = (id: string) => {
    const layer = layersRef.current.get(id);
    if (layer && layer.editEnabled()) {
      layer.disableEdit();
    }
    if (editedGeofenceRef.current === id) {
      editedGeofenceRef.current = null;
    }
    // Removing from state unmounts the declarative component, which removes the
    // leaflet layer and (via the ref callback) drops it from layersRef.
    setGeofences((prev) => prev.filter((geofence) => geofence.id !== id));
  };

  useEffect(() => {
    if (addRef) { addRef.current = add; }
    if (addCircleRef) { addCircleRef.current = addCircle; }
    if (saveRef) { saveRef.current = save; }
    if (cancelRef) { cancelRef.current = cancel; }
    if (editingRef) { editingRef.current = endEditing as EditGeofenceHandler; }
    if (removeRef) { removeRef.current = remove; }
  }, [addRef, addCircleRef, saveRef, cancelRef, editingRef, endEditing]);

  const tile = darkMode ? OSM_DARK_TILE : OSM_LIGHT_TILE;

  return (
    <div className={MAP_CONTAINER_CLASS} style={{ height, width: "100%" }}>
      <UserLocation setUserLocation={setUserLocation} />
      <MapContainer
        center={userLocation}
        zoom={DEFAULT_OSM_ZOOM}
        style={{ height: "100%", width: "100%" }}
        ref={mapRef}
        doubleClickZoom={false}>
        <TileLayer
          key={darkMode ? 'dark' : 'light'}
          url={tile.url}
          attribution={tile.attribution}
          className={tile.className}
        />
        {geofences.map((geofence) => (
          geofence.shape === 'circle' && geofence.center && geofence.radiusMeters ? (
            <Circle
              key={geofence.id}
              ref={(instance: L.Circle | null) => { registerLayer(geofence.id, instance); }}
              center={geofence.center}
              radius={geofence.radiusMeters}
              eventHandlers={{ click: () => handlePolygonClick(geofence.id) }}
            />
          ) : (
            <Polygon
              key={geofence.id}
              ref={(instance: L.Polygon | null) => { registerLayer(geofence.id, instance); }}
              positions={geofence.latlngs}
              eventHandlers={{ click: () => handlePolygonClick(geofence.id) }}
            />
          )
        ))}
        {enableScale && <OSMScaleControl position="bottomleft" imperial={false} />}
        {enableFullscreen && <OSMFullscreenControl position="topleft" />}
        {enableMeasurement && <OSMMeasurementTool position="topleft" unit="metric" enabled={true} />}
      </MapContainer>
    </div>
  );
};

export default OSMGeofenceEditor;
