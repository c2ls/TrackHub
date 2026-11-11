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
import { useState, useCallback, useRef, useEffect } from 'react';
import type { RefObject } from 'react';
import { v4 as uuidv4 } from "uuid";
import { GoogleMap, LoadScript, Polygon, Polyline, Circle, Marker } from '@react-google-maps/api';
import UserLocation from "controls/Maps/UserLocation";
import { closePolygon, circleBoundsCorners } from 'controls/Maps/utils/coordinateUtils';
import { calculateDistance } from 'utils/distanceUtils';
import { getColor } from 'data/colors';
import { GoogleScaleControl } from 'controls/Maps/shared/ScaleControl';
import { GoogleFullscreenControl } from 'controls/Maps/shared/FullscreenControl';
import { GoogleMeasurementTool } from 'controls/Maps/shared/MeasurementTool';
import { GOOGLE_NIGHT_STYLES } from 'controls/Maps/utils/darkMapStyles';
import { MAP_CONTAINER_CLASS, DEFAULT_GOOGLE_ZOOM, GEOFENCE_FIT_PADDING } from 'controls/Maps/core/mapConfig';
import 'controls/Maps/css/map.css';
import type {
  GeofenceShape,
  MapPoint,
  MapPolygon,
  GeofenceShapeHandle,
  AddGeofenceHandler,
  AddCircleGeofenceHandler,
  SaveGeofenceHandler,
  CancelGeofenceHandler,
  EditGeofenceHandler,
  RemoveGeofenceHandler
} from 'layouts/geofencemanager/components/GeofenceEditor';

type DrawMode = 'polygon' | 'circle' | null;

const DRAFT_STYLE = { clickable: false, strokeColor: '#0B7499', strokeWeight: 2, fillColor: '#0B7499', fillOpacity: 0.15 };

/** A geofence held in the editor's mutable ref. Polygons carry `latlngs`; circles carry `center`/`radiusMeters`. */
interface EditorShape {
  id: string;
  name: string;
  shape: GeofenceShape;
  latlngs: MapPoint[];
  center?: MapPoint;
  radiusMeters?: number;
  color?: number;
}

const shapeStyle = (color?: number) => {
  const value = color === undefined ? DRAFT_STYLE.strokeColor : getColor(color);
  return { strokeColor: value, strokeWeight: 2, fillColor: value, fillOpacity: 0.25 };
};

interface GoogleGeofenceEditorProps {
  mapKey?: string;
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

const GoogleGeofenceEditor = ({
  mapKey,
  initialPolygons = [],
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
}: GoogleGeofenceEditorProps) => {
  // Reformatted to editor shapes by the effect below before first use.
  const shapes = useRef<EditorShape[]>([]);
  const [userLocation, setUserLocation] = useState({
    lat: MAP_DEFAULTS.lat,
    lng: MAP_DEFAULTS.lng
  });
  // The Maps API dropped its DrawingManager, so new shapes are drawn here: clicks add polygon
  // vertices (double-click or a click on the first vertex closes the ring); a circle is a click
  // for the centre and a second click for the radius.
  const [drawMode, setDrawMode] = useState<DrawMode>(null);
  const [draftPath, setDraftPath] = useState<MapPoint[]>([]);
  const [draftCenter, setDraftCenter] = useState<MapPoint | null>(null);
  const [cursor, setCursor] = useState<MapPoint | null>(null);
  const draftPathRef = useRef<MapPoint[]>([]);

  // Escape abandons a shape that is being drawn.
  useEffect(() => {
    if (!drawMode) return undefined;
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') stopDrawing();
    };
    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [drawMode]);
  // `shapes` lives in a ref (edited imperatively), so bumping this forces the
  // canvas to re-render after a re-sync reflects freshly persisted changes.
  const [, setSyncVersion] = useState(0);
  const [mapReady, setMapReady] = useState(false);
  // Guards the one-time "fit the viewport to the account's geofences" pass so a
  // refetch/re-render never yanks the map back off the area the user panned to.
  const didFitRef = useRef(false);
  const mapRef = useRef<google.maps.Map | null>(null);
  // The pending (just-drawn, not yet saved) shape.
  const pendingRef = useRef<EditorShape | null>(null);
  // Editing an existing shape.
  const editedIdRef = useRef<string | null>(null);
  const editedShapeRef = useRef<GeofenceShape | null>(null);
  const editedCoordinatesRef = useRef<MapPoint[] | null>(null);
  const editedCenterRef = useRef<MapPoint | null>(null);
  const editedRadiusRef = useRef<number | null>(null);

  // Re-sync the editor's shapes from the source list on load and whenever it
  // changes (e.g. a mutation refetched an edited radius), but only while idle:
  // never clobber a shape being drawn (pendingRef) or edited (editedIdRef).
  useEffect(() => {
    if (pendingRef.current !== null || editedIdRef.current !== null) return;
    shapes.current = initialPolygons.map(polygon => (
      polygon.shape === 'circle' && polygon.center && polygon.radiusMeters
        ? {
            id: polygon.id,
            name: polygon.name,
            shape: 'circle' as const,
            latlngs: [],
            center: { lat: polygon.center[0], lng: polygon.center[1] },
            radiusMeters: polygon.radiusMeters,
            color: polygon.color,
          }
        : {
            id: polygon.id,
            name: polygon.name,
            shape: 'polygon' as const,
            latlngs: polygon.latlngs.map(([lat, lng]) => ({ lat, lng })),
            color: polygon.color,
          }
    ));
    setSyncVersion((v) => v + 1);
  }, [initialPolygons]);

  // Fit the viewport to the combined bounds of the loaded geofences the first
  // time a non-empty set arrives. Runs once (didFitRef), never while the user is
  // drawing/editing, and leaves the MAP_DEFAULTS center untouched when empty.
  useEffect(() => {
    if (!mapReady || !mapRef.current || didFitRef.current) return;
    if (pendingRef.current !== null || editedIdRef.current !== null) return;
    if (initialPolygons.length === 0) return;

    const bounds = new window.google.maps.LatLngBounds();
    initialPolygons.forEach((polygon) => {
      if (polygon.shape === 'circle' && polygon.center && polygon.radiusMeters) {
        const [sw, ne] = circleBoundsCorners(polygon.center[0], polygon.center[1], polygon.radiusMeters);
        bounds.extend({ lat: sw[0], lng: sw[1] });
        bounds.extend({ lat: ne[0], lng: ne[1] });
      } else {
        polygon.latlngs.forEach(([lat, lng]) => bounds.extend({ lat, lng }));
      }
    });

    if (!bounds.isEmpty()) {
      mapRef.current.fitBounds(bounds, GEOFENCE_FIT_PADDING);
      didFitRef.current = true;
    }
  }, [initialPolygons, mapReady]);

  const onLoad = useCallback((map: google.maps.Map) => {
    mapRef.current = map;
    setMapReady(true);
  }, []);

  const stopDrawing = () => {
    setDrawMode(null);
    draftPathRef.current = [];
    setDraftPath([]);
    setDraftCenter(null);
    setCursor(null);
  };

  const commitShape = (shape: EditorShape) => {
    shapes.current = [...shapes.current, shape];
    pendingRef.current = shape;
    setOpen(true);
    stopDrawing();
  };

  const finishPolygon = (path: MapPoint[]) => {
    if (path.length < 3) return;
    commitShape({
      id: uuidv4(),
      name: `Polygon ${shapes.current.length + 1}`,
      shape: 'polygon',
      latlngs: path,
    });
  };

  const handleMapClick = (event: google.maps.MapMouseEvent) => {
    if (!drawMode || !event.latLng) return;
    const point = { lat: event.latLng.lat(), lng: event.latLng.lng() };
    if (drawMode === 'polygon') {
      draftPathRef.current = [...draftPathRef.current, point];
      setDraftPath(draftPathRef.current);
    } else if (!draftCenter) {
      setDraftCenter(point);
    } else {
      commitShape({
        id: uuidv4(),
        name: `Circle ${shapes.current.length + 1}`,
        shape: 'circle',
        latlngs: [],
        center: draftCenter,
        radiusMeters: calculateDistance(draftCenter.lat, draftCenter.lng, point.lat, point.lng),
      });
    }
  };

  // A double-click arrives after its own clicks, which each added a vertex at (nearly) the same
  // spot; those are folded into one. The path is read from the ref because this handler can run
  // before the render that follows the last click.
  const handleMapDoubleClick = () => {
    if (drawMode !== 'polygon') return;
    const zoom = mapRef.current?.getZoom() ?? DEFAULT_GOOGLE_ZOOM;
    const path = [...draftPathRef.current];
    while (path.length > 1) {
      const last = path[path.length - 1];
      const previous = path[path.length - 2];
      const metersPerPixel = (156543.03392 * Math.cos((last.lat * Math.PI) / 180)) / 2 ** zoom;
      if (calculateDistance(last.lat, last.lng, previous.lat, previous.lng) > metersPerPixel * 6) break;
      path.pop();
    }
    finishPolygon(path);
  };

  const handleMapMouseMove = (event: google.maps.MapMouseEvent) => {
    if (!drawMode || !event.latLng) return;
    setCursor({ lat: event.latLng.lat(), lng: event.latLng.lng() });
  };

  const save = (): GeofenceShapeHandle | undefined => {
    const pending = pendingRef.current;
    if (!pending) return;
    pendingRef.current = null;
    setOpen(false);
    if (pending.shape === 'circle') {
      return { id: pending.id, shape: 'circle', latlngs: [], center: pending.center, radiusMeters: pending.radiusMeters };
    }
    return { id: pending.id, shape: 'polygon', latlngs: closePolygon(pending.latlngs) };
  };

  const cancel = () => {
    const pending = pendingRef.current;
    if (pending) {
      shapes.current = shapes.current.filter((shape) => shape.id !== pending.id);
      setSyncVersion((v) => v + 1);
    }
    pendingRef.current = null;
  };

  const handlePolygonEdit = (id: string, polygonInstance: google.maps.Polygon) => {
    const newPath = polygonInstance.getPath().getArray().map(latlng => ({
      lat: latlng.lat(),
      lng: latlng.lng()
    }));
    editedCoordinatesRef.current = closePolygon(newPath);
    const shape = shapes.current.find((shape) => shape.id === id);
    handleSelected(shape!.name);
    setIsEditing(true);
    editedIdRef.current = id;
    editedShapeRef.current = 'polygon';
  };

  const handleCircleEdit = (id: string, circleInstance: google.maps.Circle) => {
    const center = circleInstance.getCenter();
    editedCenterRef.current = { lat: center!.lat(), lng: center!.lng() };
    editedRadiusRef.current = circleInstance.getRadius();
    const shape = shapes.current.find((shape) => shape.id === id);
    handleSelected(shape!.name);
    setIsEditing(true);
    editedIdRef.current = id;
    editedShapeRef.current = 'circle';
  };

  const handleShapeDelete = (id: string) => {
    shapes.current = shapes.current.filter(shape => shape.id !== id);
  };

  const endEditing = (): GeofenceShapeHandle | undefined => {
    const shape = shapes.current.find((shape) => shape.id == editedIdRef.current);
    let handle: GeofenceShapeHandle | undefined;
    if (shape) {
      if (editedShapeRef.current === 'circle') {
        if (editedCenterRef.current) shape.center = editedCenterRef.current;
        if (editedRadiusRef.current != null) shape.radiusMeters = editedRadiusRef.current;
        handle = { id: shape.id, shape: 'circle', latlngs: [], center: shape.center, radiusMeters: shape.radiusMeters };
      } else {
        if (editedCoordinatesRef.current) shape.latlngs = editedCoordinatesRef.current;
        handle = { id: shape.id, shape: 'polygon', latlngs: shape.latlngs };
      }
    }
    editedCoordinatesRef.current = null;
    editedCenterRef.current = null;
    editedRadiusRef.current = null;
    editedIdRef.current = null;
    editedShapeRef.current = null;
    handleSelected(null);
    return handle;
  };

  const resetDrawState = () => {
    pendingRef.current = null;
    editedIdRef.current = null;
    editedShapeRef.current = null;
    handleSelected(null);
  };

  const add = () => {
    stopDrawing();
    setDrawMode('polygon');
    resetDrawState();
    setIsEditing(false);
  };

  const addCircle = () => {
    stopDrawing();
    setDrawMode('circle');
    resetDrawState();
    setIsEditing(false);
  };

  useEffect(() => {
    if (addRef) { addRef.current = add; }
    if (addCircleRef) { addCircleRef.current = addCircle; }
    if (saveRef) { saveRef.current = save as SaveGeofenceHandler; }
    if (cancelRef) { cancelRef.current = cancel; }
    if (editingRef) { editingRef.current = endEditing as EditGeofenceHandler; }
    if (removeRef) { removeRef.current = handleShapeDelete; }
  }, [addRef, addCircleRef, saveRef, cancelRef, removeRef, editingRef, endEditing]);

  return (
    <LoadScript googleMapsApiKey={mapKey ?? ''}>
      <UserLocation setUserLocation={setUserLocation} />
      <GoogleMap
        onLoad={onLoad}
        mapContainerClassName={MAP_CONTAINER_CLASS}
        mapContainerStyle={{ height: height, width: "100%" }}
        center={userLocation}
        zoom={DEFAULT_GOOGLE_ZOOM}
        onClick={handleMapClick}
        onDblClick={handleMapDoubleClick}
        onMouseMove={handleMapMouseMove}
        options={{
          gestureHandling: 'greedy',
          styles: darkMode ? GOOGLE_NIGHT_STYLES : null,
          draggableCursor: drawMode ? 'crosshair' : undefined,
          disableDoubleClickZoom: drawMode !== null,
        }}
      >
        {drawMode === 'polygon' && draftPath.length > 0 && (
          <>
            <Polyline path={cursor ? [...draftPath, cursor] : draftPath} options={DRAFT_STYLE} />
            <Marker
              position={draftPath[0]}
              onClick={() => finishPolygon(draftPath)}
              icon={{
                path: window.google.maps.SymbolPath.CIRCLE,
                scale: 6,
                fillColor: '#ffffff',
                fillOpacity: 1,
                strokeColor: DRAFT_STYLE.strokeColor,
                strokeWeight: 2,
              }}
            />
          </>
        )}
        {drawMode === 'circle' && draftCenter && cursor && (
          <Circle
            center={draftCenter}
            radius={calculateDistance(draftCenter.lat, draftCenter.lng, cursor.lat, cursor.lng)}
            options={DRAFT_STYLE}
          />
        )}
        {shapes.current.map((shape) => (
          shape.shape === 'circle' && shape.center && shape.radiusMeters ? (
            <Circle
              key={shape.id}
              center={shape.center}
              radius={shape.radiusMeters}
              options={{
                ...shapeStyle(shape.color),
                editable: shape.name === selectedPolygon,
                draggable: shape.name === selectedPolygon,
                clickable: drawMode === null,
              }}
              onLoad={circleInstance => {
                circleInstance.addListener('center_changed', () => handleCircleEdit(shape.id, circleInstance));
                circleInstance.addListener('radius_changed', () => handleCircleEdit(shape.id, circleInstance));
              }}
            />
          ) : (
            <Polygon
              key={shape.id}
              path={shape.latlngs}
              options={{
                ...shapeStyle(shape.color),
                editable: shape.name === selectedPolygon,
                draggable: shape.name === selectedPolygon,
                clickable: drawMode === null,
              }}
              onLoad={polygonInstance => {
                polygonInstance.addListener('mouseup', () => handlePolygonEdit(shape.id, polygonInstance));
              }}
            />
          )
        ))}
        {enableScale && <GoogleScaleControl mapRef={mapRef} />}
        {enableFullscreen && <GoogleFullscreenControl mapRef={mapRef} position="TOP_LEFT" />}
        {enableMeasurement && <GoogleMeasurementTool mapRef={mapRef} position="TOP_LEFT" unit="metric" enabled={true} />}
      </GoogleMap>
    </LoadScript>
  );
};

export default GoogleGeofenceEditor;
