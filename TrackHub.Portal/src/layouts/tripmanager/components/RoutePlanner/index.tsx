/**
* Copyright (c) 2026 Sergio Hernandez. All rights reserved.
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

import { useMemo, useState } from 'react';
import type { DragEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { usePermissions } from 'context/permissions';
import { PermissionResources, PermissionActions } from 'constants/permissions';
import Grid from '@mui/material/Grid';
import Icon from '@mui/material/Icon';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import ArgonBox from 'components/ArgonBox';
import ArgonButton from 'components/ArgonButton';
import ArgonBadge from 'components/ArgonBadge';
import ArgonTypography from 'components/ArgonTypography';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import RouteMap from 'controls/Maps/RouteMap';
import type { MapProvider } from 'controls/Maps/core/MapProviderContext';
import type { RoutePoint, RouteStop, RouteTollStation, MapPoi } from 'controls/Maps/core/mapTypes';
import { stopColor } from 'controls/Maps/core/RouteLayer/routeStyles';
import type { RoutePlan, TripStop } from 'api/tripManagement/trips';
import type { PointOfInterestLookup } from 'api/manager/pointsOfInterest';

/** Default corridor half-width offered before a trip has a plan of its own. */
export const DEFAULT_CORRIDOR_METERS = 300;

interface RoutePlannerProps {
  stops: TripStop[];
  routePlan?: RoutePlan | null;
  pois: PointOfInterestLookup[];
  mapType?: MapProvider;
  mapKey?: string | null;
  darkMode?: boolean;
  /** True while the dispatcher is choosing a point for a new stop. */
  placing: boolean;
  onMapClick: (point: RoutePoint) => void;
  onAddStop: () => void;
  onEditStop: (tripStopId: string) => void;
  onRemoveStop: (tripStopId: string) => void;
  onReorder: (orderedStopIds: string[]) => void;
  onPlanRoute: (corridorMeters: number) => void;
  planning: boolean;
  /** Read-only once the trip has left the planning stage. */
  editable: boolean;
}

/**
 * Map route planner: the trip's stops on the map, the planned route and its
 * corridor, drag-reorderable stop list, and the ORS plan trigger with a
 * corridor-width control.
 *
 * The map is provider-agnostic throughout — it renders `RouteMap`, which
 * dispatches to the OSM or Google host, and every overlay comes from
 * `controls/Maps/core/RouteLayer`.
 */
function RoutePlanner({
  stops,
  routePlan,
  pois,
  mapType,
  mapKey,
  darkMode = false,
  placing,
  onMapClick,
  onAddStop,
  onEditStop,
  onRemoveStop,
  onReorder,
  onPlanRoute,
  planning,
  editable,
}: RoutePlannerProps) {
  const { t } = useTranslation();
  // Removing a stop is Trips/Delete, which the default User role does not hold.
  const { can } = usePermissions();
  const canRemoveStops = can(PermissionResources.Trips, PermissionActions.Delete);
  const [corridorMeters, setCorridorMeters] = useState<number | string>(
    routePlan?.corridorMeters ?? DEFAULT_CORRIDOR_METERS
  );
  const [dragIndex, setDragIndex] = useState<number | null>(null);

  const ordered = useMemo(
    () => [...stops].sort((a, b) => a.sequence - b.sequence),
    [stops]
  );

  const mapStops = useMemo<RouteStop[]>(
    () =>
      ordered.map((stop) => ({
        id: stop.tripStopId,
        sequence: stop.sequence,
        name: stop.name,
        lat: stop.latitude,
        lng: stop.longitude,
        status: stop.status,
        address: stop.address,
        etaAt: stop.etaAt,
      })),
    [ordered]
  );

  const routeLine = useMemo<RoutePoint[]>(
    () =>
      (routePlan?.geometry?.coordinates ?? []).map((point) => ({
        lat: point.latitude,
        lng: point.longitude,
      })),
    [routePlan]
  );

  const corridorRing = useMemo<RoutePoint[]>(
    () =>
      (routePlan?.corridor?.coordinates ?? []).map((point) => ({
        lat: point.latitude,
        lng: point.longitude,
      })),
    [routePlan]
  );

  const tollMarkers = useMemo<RouteTollStation[]>(
    () =>
      (routePlan?.tollStations ?? []).map((station) => ({
        id: station.tollStationId,
        name: station.name,
        lat: station.latitude,
        lng: station.longitude,
        hasTariff: station.hasTariff,
        amount: station.amount,
        currency: station.currency,
        roadName: station.roadName,
      })),
    [routePlan]
  );

  const mapPois = useMemo<MapPoi[]>(
    () =>
      pois.map((poi) => ({
        name: poi.name,
        latitude: poi.latitude,
        longitude: poi.longitude,
        color: poi.color ?? 0,
        type: poi.type,
        description: poi.description,
        address: poi.address,
        active: poi.active,
      })),
    [pois]
  );

  /** Commits a new order to the server as an explicit id sequence. */
  const move = (from: number, to: number) => {
    if (to < 0 || to >= ordered.length || from === to) return;
    const next = [...ordered];
    const [moved] = next.splice(from, 1);
    next.splice(to, 0, moved);
    onReorder(next.map((stop) => stop.tripStopId));
  };

  const handleDrop = (event: DragEvent<HTMLDivElement>, index: number) => {
    event.preventDefault();
    if (dragIndex !== null) move(dragIndex, index);
    setDragIndex(null);
  };

  return (
    <Grid container spacing={2}>
      <Grid size={{ xs: 12, lg: 7 }}>
        {placing && (
          <ArgonBox mb={1}>
            <ArgonTypography variant="caption" color="info" fontWeight="medium">
              {t('tripStops.placement.mapHint')}
            </ArgonTypography>
          </ArgonBox>
        )}
        <RouteMap
          mapType={mapType}
          mapKey={mapKey}
          darkMode={darkMode}
          height="52vh"
          route={routeLine}
          corridor={corridorRing}
          stops={mapStops}
          tollStations={tollMarkers}
          pois={mapPois}
          onMapClick={placing ? onMapClick : undefined}
          onStopClick={editable ? onEditStop : undefined}
        />
      </Grid>

      <Grid size={{ xs: 12, lg: 5 }}>
        <ArgonBox display="flex" alignItems="center" justifyContent="space-between" gap={1} flexWrap="wrap">
          <ArgonTypography variant="button" fontWeight="medium">
            {t('tripStops.title')}
          </ArgonTypography>
          {editable && (
            <ArgonButton variant="outlined" color="info" size="small" onClick={onAddStop}>
              <Icon>add_location_alt</Icon>&nbsp;{t('tripStops.add')}
            </ArgonButton>
          )}
        </ArgonBox>

        {ordered.length === 0 ? (
          <ArgonTypography variant="caption" color="secondary">
            {t('tripStops.empty')}
          </ArgonTypography>
        ) : (
          <>
            <ArgonTypography variant="caption" color="secondary" display="block" mb={0.5}>
              {t('tripStops.reorderHint')}
            </ArgonTypography>
            <ArgonBox sx={{ maxHeight: '38vh', overflowY: 'auto' }}>
              {ordered.map((stop, index) => (
                <ArgonBox
                  key={stop.tripStopId}
                  draggable={editable}
                  onDragStart={() => setDragIndex(index)}
                  onDragOver={(event: DragEvent<HTMLDivElement>) => event.preventDefault()}
                  onDrop={(event: DragEvent<HTMLDivElement>) => handleDrop(event, index)}
                  display="flex"
                  alignItems="center"
                  gap={1}
                  py={0.75}
                  px={1}
                  mb={0.5}
                  sx={{
                    border: '1px solid',
                    borderColor: 'grey.300',
                    borderRadius: 1,
                    cursor: editable ? 'grab' : 'default',
                    borderLeft: `4px solid ${stopColor(stop.status)}`,
                  }}
                >
                  <ArgonTypography variant="button" fontWeight="bold" sx={{ minWidth: 22 }}>
                    {stop.sequence}
                  </ArgonTypography>
                  <ArgonBox flexGrow={1} minWidth={0}>
                    <ArgonTypography variant="button" fontWeight="medium" noWrap>
                      {stop.name}
                    </ArgonTypography>
                    <ArgonTypography variant="caption" color="secondary" display="block" noWrap>
                      {stop.address ?? `${stop.latitude.toFixed(4)}, ${stop.longitude.toFixed(4)}`}
                    </ArgonTypography>
                  </ArgonBox>
                  <ArgonBadge
                    variant="gradient"
                    color="secondary"
                    size="xs"
                    container
                    badgeContent={t(
                      `tripStops.statuses.${stop.status}` as 'tripStops.statuses.Pending'
                    )}
                  />
                  {editable && (
                    <>
                      <Tooltip title={t('tripStops.actions.moveUp')}>
                        <span>
                          <IconButton size="small" disabled={index === 0} onClick={() => move(index, index - 1)}>
                            <Icon fontSize="small">arrow_upward</Icon>
                          </IconButton>
                        </span>
                      </Tooltip>
                      <Tooltip title={t('tripStops.actions.moveDown')}>
                        <span>
                          <IconButton
                            size="small"
                            disabled={index === ordered.length - 1}
                            onClick={() => move(index, index + 1)}
                          >
                            <Icon fontSize="small">arrow_downward</Icon>
                          </IconButton>
                        </span>
                      </Tooltip>
                      <Tooltip title={t('generic.edit')}>
                        <IconButton size="small" onClick={() => onEditStop(stop.tripStopId)}>
                          <Icon fontSize="small">edit</Icon>
                        </IconButton>
                      </Tooltip>
                      {canRemoveStops && (
                        <Tooltip title={t('tripStops.actions.remove')}>
                          <IconButton size="small" color="error" onClick={() => onRemoveStop(stop.tripStopId)}>
                            <Icon fontSize="small">delete</Icon>
                          </IconButton>
                        </Tooltip>
                      )}
                    </>
                  )}
                </ArgonBox>
              ))}
            </ArgonBox>
          </>
        )}

        <ArgonBox mt={2}>
          <ArgonTypography variant="button" fontWeight="medium">
            {t('trips.routePlan.title')}
          </ArgonTypography>
          {routePlan ? (
            <ArgonBox mt={0.5}>
              {routePlan.status === 'Failed' ? (
                <ArgonTypography variant="caption" color="error" display="block">
                  {t('trips.routePlan.failed')}
                  {routePlan.errorCode ? ` (${routePlan.errorCode})` : ''}
                </ArgonTypography>
              ) : (
                <ArgonBox display="flex" gap={2} flexWrap="wrap">
                  <ArgonTypography variant="caption" color="text">
                    {t('trips.routePlan.plannedDistance')}:{' '}
                    {t('trips.kilometers', {
                      value: (routePlan.plannedDistanceMeters / 1000).toFixed(1),
                    })}
                  </ArgonTypography>
                  <ArgonTypography variant="caption" color="text">
                    {t('trips.routePlan.plannedDuration')}:{' '}
                    {t('trips.minutes', {
                      value: Math.round(routePlan.plannedDurationSeconds / 60),
                    })}
                  </ArgonTypography>
                  <ArgonTypography variant="caption" color="text">
                    {t('trips.routePlan.provider')}: {routePlan.provider}
                  </ArgonTypography>
                </ArgonBox>
              )}
            </ArgonBox>
          ) : (
            <ArgonTypography variant="caption" color="secondary" display="block">
              {t('trips.routePlan.notPlanned')}
            </ArgonTypography>
          )}

          <ArgonBox display="flex" alignItems="flex-end" gap={1} mt={1} flexWrap="wrap">
            <ArgonBox width="160px">
              <CustomTextField
                margin="none"
                name="corridorMeters"
                id="corridorMeters"
                label={t('trips.routePlan.corridorMeters')}
                type="number"
                value={corridorMeters}
                onChange={(event) => setCorridorMeters(event.target.value)}
              />
            </ArgonBox>
            <ArgonButton
              variant="gradient"
              color="info"
              size="small"
              disabled={planning || ordered.length === 0}
              onClick={() => onPlanRoute(Number(corridorMeters) || DEFAULT_CORRIDOR_METERS)}
            >
              <Icon>alt_route</Icon>&nbsp;
              {routePlan ? t('trips.actions.replan') : t('trips.actions.plan')}
            </ArgonButton>
          </ArgonBox>
          <ArgonTypography variant="caption" color="secondary">
            {t('trips.routePlan.corridorHint')}
          </ArgonTypography>
        </ArgonBox>
      </Grid>
    </Grid>
  );
}

export default RoutePlanner;
