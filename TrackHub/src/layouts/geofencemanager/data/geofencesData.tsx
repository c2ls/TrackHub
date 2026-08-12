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

import { useEffect, useMemo, useState, useContext } from "react";
import type { Dispatch, ReactNode, SetStateAction } from "react";
import { useTranslation } from 'react-i18next';
import Icon from "@mui/material/Icon";
import ArgonButton from "components/ArgonButton";
import ArgonTypography from "components/ArgonTypography";
import { getGeofence } from "api/geofencing/geofencing";
import type { Geofence, GeofenceDtoInput } from "api/geofencing/geofencing";
import { ApiError, notifyApiError } from 'api/core/errors';
import {
  useGeofencesByAccount,
  useAllGeofences,
  useCreateGeofence,
  useUpdateGeofence,
  useDeleteGeofence,
} from 'queries/geofences';
import { getGeofenceType } from 'data/geofenceTypes';
import { getColor } from 'data/colors';
import { toCamelCase } from 'utils/stringUtils';
import { LoadingContext } from 'LoadingContext';
import { useAuth } from "AuthContext";
import type { MapPoint, MapPolygon, GeofenceShape } from 'layouts/geofencemanager/components/GeofenceEditor';

/** Server-side filters + paging state for the geofence list. */
export interface GeofenceListParams {
  skip: number;
  take: number;
  type?: number | null;
  active?: boolean | null;
  search?: string | null;
}

/**
 * Dialog/form state for a geofence. Merges an API {@link Geofence} with UI-only
 * fields the editor adds (`new`, `coordinates`, `shape`) before submit.
 * `type`/`color` may arrive as strings from the select controls, hence the
 * widened unions.
 */
export interface GeofenceFormValues {
  geofenceId?: string;
  name?: string;
  description?: string | null;
  type?: number | string;
  color?: number | string;
  active?: boolean;
  new?: boolean;
  /** Whether the shape being saved is a polygon or a center+radius circle. */
  shape?: GeofenceShape;
  /** Points drawn on the map ({lat,lng}) for the non-new save path. */
  coordinates?: MapPoint[];
  geom?: { srid: number; coordinates: { latitude: number; longitude: number }[] };
  circleCenter?: { latitude: number; longitude: number } | null;
  circleRadiusMeters?: number | string | null;
  alertOnEntry?: boolean;
  alertOnExit?: boolean;
  dwellThresholdMinutes?: number | string | null;
}

/** A column descriptor consumed by the `Table` control. */
export interface GeofenceColumn { name: string; title?: string; align?: "left" | "right" | "center"; width?: string; }

/**
 * Name cell for the geofence list: regular weight (only the header is bold) and
 * truncated with an ellipsis inside its fixed-width column so a long name never
 * overlaps the adjacent columns in the narrow side panel. The full name shows on
 * hover via the native title tooltip. Exposes `name` so the `Table` still reads
 * the selected value for row click-to-select/zoom.
 */
function GeofenceNameCell({ name }: { name: string }) {
  return (
    <ArgonTypography
      variant="button"
      fontWeight="regular"
      color="text"
      title={name}
      sx={{
        display: "block",
        maxWidth: "100%",
        overflow: "hidden",
        textOverflow: "ellipsis",
        whiteSpace: "nowrap",
      }}
    >
      {name}
    </ArgonTypography>
  );
}

/**
 * Secondary text cell for the geofence list (type/color columns): mirrors the
 * shared `Table` control's default cell rendering (button variant, regular
 * weight, secondary color) so the grid reads like the dashboard's tables.
 */
function GeofenceTextCell({ children }: { children: ReactNode }) {
  return (
    <ArgonTypography variant="button" fontWeight="regular" color="secondary">
      {children}
    </ArgonTypography>
  );
}
/** A rendered table row for the geofence list. */
export interface GeofenceRow {
  [key: string]: unknown;
  name: ReactNode;
  type: ReactNode;
  color: ReactNode;
  action: ReactNode;
  id: string;
}
export interface GeofenceTableData {
  geofences: MapPolygon[];
  columns: GeofenceColumn[];
  rows: GeofenceRow[];
}

export interface UseGeofencesTableData {
  data: GeofenceTableData;
  /** Total number of geofences matching the current list filters (server count). */
  totalCount: number;
  open: boolean;
  confirmOpen: boolean;
  onGet: (geofenceId: string) => Promise<Geofence>;
  onSave: (geofence: GeofenceFormValues) => Promise<void>;
  onDelete: (geofenceId: string) => Promise<void>;
  setOpen: Dispatch<SetStateAction<boolean>>;
  setConfirmOpen: Dispatch<SetStateAction<boolean>>;
}

// Map an API geofence to the shape the map editor renders (true circle when the
// circle fields are set, polygon ring otherwise).
function toMapPolygon(item: Geofence): MapPolygon {
  const latlngs = item.geom.coordinates.map((coord): [number, number] => [coord.latitude, coord.longitude]);
  if (item.circleCenter && item.circleRadiusMeters) {
    return {
      id: item.geofenceId,
      name: item.name,
      latlngs,
      shape: 'circle',
      center: [item.circleCenter.latitude, item.circleCenter.longitude],
      radiusMeters: item.circleRadiusMeters,
    };
  }
  return { id: item.geofenceId, name: item.name, latlngs, shape: 'polygon' };
}

// Builds a clean GeofenceDtoInput from the dialog values, dropping the UI-only
// fields (new, coordinates, accountId, ...) the schema does not accept. Sends
// EITHER geom (polygon) OR circleCenter+circleRadiusMeters (circle), never both.
function toGeofenceInput(geofence: GeofenceFormValues): GeofenceDtoInput {
  const dwell =
    geofence.dwellThresholdMinutes === '' || geofence.dwellThresholdMinutes == null
      ? null
      : Number(geofence.dwellThresholdMinutes);
  const base = {
    geofenceId: geofence.geofenceId as string,
    name: geofence.name as string,
    description: geofence.description ?? null,
    type: Number(geofence.type),
    color: Number(geofence.color),
    active: !!geofence.active,
    alertOnEntry: !!geofence.alertOnEntry,
    alertOnExit: !!geofence.alertOnExit,
    dwellThresholdMinutes: dwell,
  };
  if (geofence.shape === 'circle' && geofence.circleCenter && geofence.circleRadiusMeters) {
    return {
      ...base,
      circleCenter: {
        latitude: geofence.circleCenter.latitude,
        longitude: geofence.circleCenter.longitude,
      },
      circleRadiusMeters: Number(geofence.circleRadiusMeters),
    };
  }
  return {
    ...base,
    geom: {
      srid: geofence.geom!.srid,
      coordinates: geofence.geom!.coordinates.map(coord => ({
        latitude: coord.latitude,
        longitude: coord.longitude,
      })),
    },
  };
}

function useGeofencesTableData(
  handleEditClick: (rowData: GeofenceFormValues) => void,
  handleDeleteClick: (geofenceId: string) => void,
  listParams: GeofenceListParams
): UseGeofencesTableData {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const { setLoading } = useContext(LoadingContext);
  const { isAuthenticated } = useAuth();

  // Full list feeds the map editor: all shapes stay visible/selectable while the
  // table pages independently. Drains every server page so accounts with >500
  // geofences are not silently truncated on the map.
  const allGeofencesQuery = useAllGeofences(false, {}, { enabled: isAuthenticated });
  const allGeofences = allGeofencesQuery.data ?? [];
  // Server-paged, filtered list feeds the table.
  const pagedQuery = useGeofencesByAccount(false, listParams, { enabled: isAuthenticated });
  const pageItems = pagedQuery.data?.items ?? [];
  const totalCount = pagedQuery.data?.totalCount ?? 0;
  const createGeofence = useCreateGeofence();
  const updateGeofence = useUpdateGeofence();
  const deleteGeofence = useDeleteGeofence();

  // Keep the global spinner UX for the initial load / invalidation refetch.
  useEffect(() => {
    setLoading(allGeofencesQuery.isFetching || pagedQuery.isFetching);
  }, [allGeofencesQuery.isFetching, pagedQuery.isFetching, setLoading]);

  const onSave = async (geofence: GeofenceFormValues) => {
    setLoading(true);
    try {
      const input = toGeofenceInput(geofence);
      if (geofence.new) {
        await createGeofence.mutateAsync(input);
      } else {
        await updateGeofence.mutateAsync({ geofenceId: geofence.geofenceId as string, geofence: input });
      }
      setOpen(false);
    } catch (error) {
      // ApiErrors from the mutation are already surfaced by the global toast
      // (MutationCache.onError). A non-ApiError here is a programming bug (e.g.
      // building the input) that the toast never sees, so log and toast it
      // instead of swallowing it. Keep the dialog open so the user can retry.
      if (!(error instanceof ApiError)) {
        if (process.env.NODE_ENV !== 'production') console.error(error);
        notifyApiError(error);
      }
    } finally {
      setLoading(false);
    }
  };

  const onDelete = async (geofenceId: string) => {
    setLoading(true);
    try {
      await deleteGeofence.mutateAsync(geofenceId);
      setConfirmOpen(false);
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDelete = (geofenceId: string) => {
    handleDeleteClick(geofenceId);
    setConfirmOpen(true);
  };

  // Imperative single-geofence read for the map editor. Throws on failure
  // (surfaced by the global toast) — the caller decides what to do next.
  const onGet = async (geofenceId: string): Promise<Geofence> => getGeofence(geofenceId);

  const buildTableData = (mapSource: Geofence[], rowSource: Geofence[]): GeofenceTableData => ({
    geofences: mapSource.map(toMapPolygon),
    columns: [
      { name: "name", title:t('geofence.name'), align: "left", width: "42%" },
      { name: "type", title:t('geofence.type'), align: "left", width: "30%" },
      { name: "color", title:t('geofence.color'), align: "left", width: "16%" },
      { name: "action", title:t('generic.action'), align: "center", width: "12%" },
      { name: "id" }
    ],
    rows: rowSource.map(geofence => ({
      name: <GeofenceNameCell name={geofence.name} />,
      type: <GeofenceTextCell>{t(`geofenceTypes.${toCamelCase(getGeofenceType(geofence.type))}` as 'geofenceTypes.office')}</GeofenceTextCell>,
      color: <GeofenceTextCell>{t(`colors.${getColor(geofence.color).toLowerCase()}` as 'colors.red')}</GeofenceTextCell>,
      action: (
        <ArgonButton
          variant="text"
          color="error"
          onClick={() => handleOpenDelete(geofence.geofenceId)}>
          <Icon>delete</Icon>
        </ArgonButton>
      ),
      id: geofence.geofenceId
    })),
  });

  const data = useMemo(
    () => buildTableData(allGeofences, pageItems),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [allGeofences, pageItems, t]
  );

  return {
    data,
    totalCount,
    open,
    confirmOpen,
    onGet,
    onSave,
    onDelete,
    setOpen,
    setConfirmOpen };
}

export default useGeofencesTableData;
