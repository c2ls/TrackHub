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
import { GoogleMap, LoadScript, DrawingManager, Polygon, Circle } from '@react-google-maps/api';
import type { Libraries } from '@react-google-maps/api';
import UserLocation from "controls/Maps/UserLocation";
import { closePolygon, circleBoundsCorners } from 'controls/Maps/utils/coordinateUtils';
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

const libraries: Libraries = ['drawing'];

/** A geofence held in the editor's mutable ref. Polygons carry `latlngs`; circles carry `center`/`radiusMeters`. */
interface EditorShape {
  id: string;
  name: string;
  shape: GeofenceShape;
  latlngs: MapPoint[];
  center?: MapPoint;
  radiusMeters?: number;
}

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
  const [drawingMode, setDrawingMode] = useState<google.maps.drawing.OverlayType | null>(null);
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
  const drawingManagerRef = useRef<google.maps.drawing.DrawingManager | null>(null);
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
          }
        : {
            id: polygon.id,
            name: polygon.name,
            shape: 'polygon' as const,
            latlngs: polygon.latlngs.map(([lat, lng]) => ({ lat, lng })),
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

  const onDrawingManagerLoad = useCallback((drawingManager: google.maps.drawing.DrawingManager) => {
    drawingManagerRef.current = drawingManager;
  }, []);

  const handlePolygonComplete = (polygon: google.maps.Polygon) => {
    const path = polygon.getPath().getArray().map(latlng => ({
      lat: latlng.lat(),
      lng: latlng.lng()
    }));
    const newShape: EditorShape = {
      id: uuidv4(),
      name: `Polygon ${shapes.current.length + 1}`,
      shape: 'polygon',
      latlngs: path,
    };

    shapes.current = [...shapes.current, newShape];
    pendingRef.current = newShape;
    setOpen(true);
    setDrawingMode(null);
    polygon.setMap(null);
  };

  const handleCircleComplete = (circle: google.maps.Circle) => {
    const center = circle.getCenter();
    const newShape: EditorShape = {
      id: uuidv4(),
      name: `Circle ${shapes.current.length + 1}`,
      shape: 'circle',
      latlngs: [],
      center: { lat: center!.lat(), lng: center!.lng() },
      radiusMeters: circle.getRadius(),
    };

    shapes.current = [...shapes.current, newShape];
    pendingRef.current = newShape;
    setOpen(true);
    setDrawingMode(null);
    circle.setMap(null);
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
    setDrawingMode('polygon' as google.maps.drawing.OverlayType);
    resetDrawState();
    setIsEditing(false);
  };

  const addCircle = () => {
    setDrawingMode('circle' as google.maps.drawing.OverlayType);
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
    <LoadScript googleMapsApiKey={mapKey ?? ''} libraries={libraries}>
      <UserLocation setUserLocation={setUserLocation} />
      <GoogleMap
        onLoad={onLoad}
        mapContainerClassName={MAP_CONTAINER_CLASS}
        mapContainerStyle={{ height: height, width: "100%" }}
        center={userLocation}
        zoom={DEFAULT_GOOGLE_ZOOM}
        options={{ gestureHandling: 'greedy', styles: darkMode ? GOOGLE_NIGHT_STYLES : null }}
      >
        {window.google && window.google.maps && (
          <DrawingManager
            onLoad={onDrawingManagerLoad}
            onPolygonComplete={handlePolygonComplete}
            onCircleComplete={handleCircleComplete}
            options={{
              drawingControl: false,
              drawingMode: drawingMode,
              polygonOptions: {
                editable: true,
                draggable: true
              },
              circleOptions: {
                editable: true,
                draggable: true
              }
            }}
          />
        )}
        {shapes.current.map((shape) => (
          shape.shape === 'circle' && shape.center && shape.radiusMeters ? (
            <Circle
              key={shape.id}
              center={shape.center}
              radius={shape.radiusMeters}
              options={{
                editable: shape.name === selectedPolygon,
                draggable: shape.name === selectedPolygon,
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
                editable: shape.name === selectedPolygon,
                draggable: shape.name === selectedPolygon,
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
