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

import { useState, useEffect, useContext, useMemo, useRef } from 'react';
import Grid from "@mui/material/Grid";
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import Icon from "@mui/material/Icon";
import ArgonBox from "components/ArgonBox";
import ArgonButton from "components/ArgonButton";
import ArgonTypography from "components/ArgonTypography";
import FilterNavbar from 'controls/Navbars/FilterNavbar';
import TripList from "layouts/dashboard/components/TripList";
import TripsMap from "layouts/dashboard/components/TripsMap";
import PlaybackControls from "layouts/dashboard/components/Positions/PlaybackControls";
import { useQueryClient } from '@tanstack/react-query';
import { getTripsByTransporter } from 'api/router/router';
import type { Trip, PositionSourceType } from 'api/router/router';
import { routerKeys } from 'queries/router';
import { useTransportersByUser } from 'queries/transporters';
import { getAccountFeatures } from 'api/manager/accountFeatures';
import { notifyApiError } from 'api/core/errors';
import useForm from 'controls/Dialogs/useForm';
import { usePlayback } from 'layouts/dashboard/utils/playback';
import type { TripPoint } from 'layouts/dashboard/utils/playback';
import { downloadCsv, sanitizeFileNamePart } from 'utils/csvUtils';
import { LoadingContext } from 'LoadingContext';
import { useTranslation } from 'react-i18next';
import { useAuth } from "AuthContext";
import { useArgonController } from 'context';
import type { AccountSettings } from 'api/manager/settings';
import type { Geofence } from 'api/geofencing/geofencing';

const POSITION_HISTORY_FEATURE_KEY = 'gps.positionHistory';

/** Replay filter values managed by the vendored useForm hook. */
interface PositionsFormValues {
  selectedItem?: string;
  startDate?: string;
  endDate?: string;
  [key: string]: string | undefined;
}


/** An option shown in the transporter selector. */
interface FilterNavbarOption { value: string; label: string; }

/** The query parameters of the currently loaded trip set (for CSV export). */
interface LoadedQuery {
  transporterId?: string;
  from?: string;
  to?: string;
  source: PositionSourceType;
}

interface PositionsProps {
  settings: AccountSettings;
  showGeofence?: boolean;
  geofences: Geofence[];
}

function Positions({ settings, showGeofence, geofences }: PositionsProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { setLoading } = useContext(LoadingContext);
  const { isAuthenticated } = useAuth();
  const [controller] = useArgonController();
  const { darkMode } = controller;
  const [trips, setTrips] = useState<Trip[]>([]);
  const [selectedTrip, setSelectedTrip] = useState<string | null>(null);
  const [historyEnabled, setHistoryEnabled] = useState(false);
  const [source, setSource] = useState<PositionSourceType>('PROVIDER');
  const [loadedQuery, setLoadedQuery] = useState<LoadedQuery | null>(null);
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<PositionsFormValues>({});

  const transportersQuery = useTransportersByUser({ enabled: isAuthenticated });
  const transporters = useMemo<FilterNavbarOption[]>(
    () => (transportersQuery.data ?? []).map(transporter => ({
      value: transporter.transporterId,
      label: transporter.name
    })),
    [transportersQuery.data]
  );
  const defaultSelectionSetRef = useRef(false);

  // Keep the global spinner UX while the transporter list loads.
  useEffect(() => {
    setLoading(transportersQuery.isFetching);
  }, [transportersQuery.isFetching, setLoading]);

  // Default the filter to the first transporter, once, after the list first loads.
  useEffect(() => {
    if (transportersQuery.isSuccess && !defaultSelectionSetRef.current) {
      const list = transportersQuery.data ?? [];
      setValues({ selectedItem: list.length > 0 ? list[0].transporterId : '' });
      defaultSelectionSetRef.current = true;
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [transportersQuery.isSuccess, transportersQuery.data]);

  const fetchPositions = async () => {
    setLoading(true);
    const usedSource: PositionSourceType = historyEnabled ? source : 'PROVIDER';
    let result: Trip[] = [];
    try {
      // The required-fields validation gate guarantees these are set before we
      // reach here (see handleSearch).
      result = await queryClient.fetchQuery({
        queryKey: routerKeys.trips(values.selectedItem!, values.startDate!, values.endDate!, usedSource),
        queryFn: () => getTripsByTransporter(values.selectedItem!, values.startDate!, values.endDate!, usedSource),
        staleTime: 0,
      });
    } catch {
      // Failure is surfaced by the global toast; fall back to an empty result.
      result = [];
    }
    setTrips(result);
    setLoadedQuery({
      transporterId: values.selectedItem,
      from: values.startDate,
      to: values.endDate,
      source: usedSource
    });
    setSelectedTrip(null);
    setErrors({});
    setLoading(false);
  };

  const fetchFeatures = async () => {
    if (!settings?.accountId) return;
    try {
      const features = await getAccountFeatures(settings.accountId);
      const feature = features.find(item => item.featureKey === POSITION_HISTORY_FEATURE_KEY);
      setHistoryEnabled(!!feature?.enabled);
    } catch (error) {
      notifyApiError(error);
      setHistoryEnabled(false);
    }
  };

  useEffect(() => {
    if (isAuthenticated) {
      fetchFeatures();
    }
  }, [isAuthenticated, settings?.accountId]);

  const handleSelected = (selected: string | null) => {
    setSelectedTrip(selected);
  };

  const handleSearch = async () => {
    if (validate(['startDate', 'endDate', 'selectedItem'])) {
      await fetchPositions();
    }
  };

  const handleExport = () => {
    if (trips.length === 0 || !loadedQuery) return;
    const transporter = transporters.find(item => item.value === loadedQuery.transporterId);
    const transporterName = transporter ? transporter.label : loadedQuery.transporterId;
    const headers = [
      t('replay.exportTransporter'),
      t('replay.exportTrip'),
      t('replay.exportTimestamp'),
      t('replay.exportLatitude'),
      t('replay.exportLongitude'),
      t('replay.exportSpeed')
    ];
    // No Address column: the backend's TripPointVm carries no address (only live
    // positions do), so the old column was always empty. Restore it if/when the
    // Router service adds address to trip points.
    const rows = trips.flatMap(trip => (trip.points || []).map(point => [
      transporterName,
      trip.tripId,
      point.deviceDateTime,
      point.latitude,
      point.longitude,
      point.speed
    ]));
    const filename = [
      'replay',
      sanitizeFileNamePart(transporterName),
      loadedQuery.source,
      sanitizeFileNamePart(loadedQuery.from),
      sanitizeFileNamePart(loadedQuery.to)
    ].filter(Boolean).join('_');
    downloadCsv(`${filename}.csv`, headers, rows);
  };

  const selectedTripPoints = useMemo<TripPoint[] | null>(() => {
    if (!selectedTrip) return null;
    const trip = trips.find(item => item.tripId === selectedTrip);
    return trip ? trip.points : null;
  }, [selectedTrip, trips]);

  const playback = usePlayback(selectedTripPoints);

  return (
    <ArgonBox py={1}>
      <Grid container spacing={3} mb={1}>
        <Grid size={{xs:12, lg:12}}>
          <FilterNavbar
            list={transporters}
            values={values}
            handleChange={handleChange}
            errors={errors}
            handleSearch={handleSearch}
          />
          <ArgonBox display="flex" alignItems="center" gap={1} mb={1}>
            {historyEnabled && (
              <>
                <ArgonTypography variant="caption" fontWeight="bold">
                  {t('replay.source')}
                </ArgonTypography>
                <ToggleButtonGroup
                  size="small"
                  exclusive
                  value={source}
                  aria-label={t('replay.source')}
                  onChange={(_event, value: PositionSourceType | null) => value && setSource(value)}>
                  <ToggleButton value="PROVIDER">{t('replay.sourceProvider')}</ToggleButton>
                  <ToggleButton value="STORED">{t('replay.sourceTrackHub')}</ToggleButton>
                </ToggleButtonGroup>
              </>
            )}
            <ArgonBox ml="auto">
              <ArgonButton
                variant="contained"
                color="primary"
                size="small"
                disabled={trips.length === 0}
                onClick={handleExport}>
                <Icon>download</Icon>&nbsp;{t('replay.export')}
              </ArgonButton>
            </ArgonBox>
          </ArgonBox>
        </Grid>
      </Grid>
      <Grid container spacing={3}>
        <Grid size={{xs: 12, lg:9}}>
          <TripsMap
            mapType={settings.maps as 'OSM' | 'Google'}
            mapKey={settings.mapsKey}
            trips={trips}
            selectedTrip={selectedTrip}
            geofences={geofences}
            showGeofence={showGeofence}
            handleSelected={handleSelected}
            playbackPosition={selectedTrip ? playback.position : null}
            darkMode={darkMode}
            height="calc(100vh - 384px)"/>
          {selectedTrip && playback.hasTimeline && (
            <PlaybackControls
              playing={playback.playing}
              toggle={playback.toggle}
              speed={playback.speed}
              setSpeed={playback.setSpeed}
              progress={playback.progress}
              seek={playback.seek}
            />
          )}
        </Grid>
        <Grid size={{xs:12, lg:3}}>
          <TripList
            trips={trips}
            filters={values}
            selectedTrip={selectedTrip}
            handleSelected={handleSelected}
            maxHeight="calc(100vh - 384px)"/>
        </Grid>
      </Grid>
    </ArgonBox>
  );
}

export default Positions;
