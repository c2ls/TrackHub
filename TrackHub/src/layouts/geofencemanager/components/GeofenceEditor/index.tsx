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

import { useState } from 'react';
import type { RefObject } from 'react';
import Tooltip from '@mui/material/Tooltip';
import Icon from '@mui/material/Icon';
import { useTranslation } from 'react-i18next';
import { useArgonController } from 'context';
import OSMGeofenceEditor from 'controls/Maps/OSM/GeofenceEditor';
import GoogleGeofenceEditor from 'controls/Maps/Google/GeofenceEditor';
import MapControlStyle from 'controls/Maps/styles/MapControl';

/** A point in the imperative map-editor handle payloads. */
export interface MapPoint { lat: number; lng: number; }
/** Whether a geofence is a free-drawn polygon or a center+radius circle. */
export type GeofenceShape = 'polygon' | 'circle';
/**
 * Result of committing (save/edit) a drawn geofence shape on the map. Polygons
 * carry their ring in `latlngs`; circles carry `center` + `radiusMeters` and an
 * empty `latlngs`.
 */
export interface GeofenceShapeHandle {
  id: string;
  shape: GeofenceShape;
  latlngs: MapPoint[];
  center?: MapPoint;
  radiusMeters?: number;
}
/**
 * A geofence rendered on the map. Polygons use `latlngs` (geom as [lat, lng]
 * tuples); circles additionally set `shape: 'circle'`, `center` and
 * `radiusMeters` so they render as true circles and stay editable.
 */
export interface MapPolygon {
  id: string;
  name: string;
  latlngs: [number, number][];
  shape?: GeofenceShape;
  center?: [number, number];
  radiusMeters?: number;
}

export type AddGeofenceHandler = () => void;
export type AddCircleGeofenceHandler = () => void;
export type SaveGeofenceHandler = (name: string) => GeofenceShapeHandle;
export type CancelGeofenceHandler = () => void;
export type EditGeofenceHandler = () => GeofenceShapeHandle;
export type RemoveGeofenceHandler = (id: string) => void;

/** Refs the vendored map editors populate with imperative draw handles. */
export interface GeofenceEditorHandles {
  addRef?: RefObject<AddGeofenceHandler | null>;
  addCircleRef?: RefObject<AddCircleGeofenceHandler | null>;
  saveRef?: RefObject<SaveGeofenceHandler | null>;
  cancelRef?: RefObject<CancelGeofenceHandler | null>;
  editingRef?: RefObject<EditGeofenceHandler | null>;
  removeRef?: RefObject<RemoveGeofenceHandler | null>;
}

interface GeofenceEditorProps extends GeofenceEditorHandles {
  mapType: 'OSM' | 'Google';
  mapKey?: string | null;
  geofences?: MapPolygon[];
  selectedGeofence?: string | null;
  // Required by the underlying OSM/Google editors, which the manager always wires up.
  handleSelected: (value: string | null) => void;
  setOpen: (open: boolean) => void;
  handleAdd?: () => void;
  handleAddCircle?: () => void;
  handleEdit?: () => void;
  height?: string;
}

const GeofenceEditor = ({
    mapType,
    mapKey,
    geofences,
    selectedGeofence,
    handleSelected,
    setOpen,
    addRef,
    addCircleRef,
    saveRef,
    cancelRef,
    editingRef,
    removeRef,
    handleAdd,
    handleAddCircle,
    handleEdit,
    height = '70vh'
    }: GeofenceEditorProps) => {

  const { t } = useTranslation();
  const [controller] = useArgonController();
  const { darkMode } = controller;
  const [isEditing, setIsEditing] = useState(false);

  return (
    <MapControlStyle>
        {mapType === 'OSM' ? (
            <OSMGeofenceEditor
                initialPolygons={geofences}
                selectedPolygon={selectedGeofence}
                handleSelected={handleSelected}
                setIsEditing={setIsEditing}
                setOpen={setOpen}
                addRef={addRef}
                addCircleRef={addCircleRef}
                saveRef={saveRef}
                cancelRef={cancelRef}
                editingRef={editingRef}
                removeRef={removeRef}
                darkMode={darkMode}
                height={height}
            />
            ) : (
                mapType === 'Google' &&
                <GoogleGeofenceEditor
                    mapKey={mapKey ?? undefined}
                    initialPolygons={geofences}
                    selectedPolygon={selectedGeofence}
                    handleSelected={handleSelected}
                    setIsEditing={setIsEditing}
                    setOpen={setOpen}
                    addRef={addRef}
                    addCircleRef={addCircleRef}
                    saveRef={saveRef}
                    cancelRef={cancelRef}
                    editingRef={editingRef}
                    removeRef={removeRef}
                    darkMode={darkMode}
                    height={height}
                />
            )}
        <div className="mapcontrol">
          <Tooltip title={t('geofence.drawPolygon')} placement="left">
            <label className="mapcontrol-label" onClick={handleAdd}>
              <Icon fontSize="small" sx={{ verticalAlign: 'middle' }}>pentagon</Icon>
            </label>
          </Tooltip>
          <Tooltip title={t('geofence.drawCircle')} placement="left">
            <label className="mapcontrol-label" onClick={handleAddCircle}>
              <Icon fontSize="small" sx={{ verticalAlign: 'middle' }}>circle</Icon>
            </label>
          </Tooltip>
          {isEditing && (
            <Tooltip title={t('geofence.saveShape')} placement="left">
              <label className="mapcontrol-label" onClick={handleEdit}>
                <Icon fontSize="small" sx={{ verticalAlign: 'middle' }}>save</Icon>
              </label>
            </Tooltip>
          )}
        </div>
    </MapControlStyle>
  );
}

export default GeofenceEditor;
