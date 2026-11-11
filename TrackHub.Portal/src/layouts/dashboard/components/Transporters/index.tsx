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

import { useState, useEffect, useContext, useRef, useMemo, useCallback } from 'react';
import Card from "@mui/material/Card";
import Chip from "@mui/material/Chip";
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import UnitCard from "layouts/dashboard/components/UnitCard";
import TransportersTable from "layouts/dashboard/components/TransportersTable";
import RefreshCounter from 'layouts/dashboard/components/RefreshCounter';
import FilterBar from 'layouts/dashboard/components/Transporters/FilterBar';
import type { FilterOption, DashboardFilters, FleetStat } from 'layouts/dashboard/components/Transporters/FilterBar';
import { getPointOfInterestLookup } from 'api/manager/pointsOfInterest';
import type { PointOfInterestLookup } from 'api/manager/pointsOfInterest';
import { getGroupLookup } from 'api/manager/groups';
import type { GroupLookup } from 'api/manager/groups';
import { useQueryClient } from '@tanstack/react-query';
import { getOperatorLookup } from 'api/manager/operators';
import type { OperatorLookup } from 'api/manager/operators';
import { getAccountByUser } from 'api/manager/accounts';
import { getOpenAlertCounts } from 'api/manager/alertEvents';
import { getDevicePositions } from 'api/router/router';
import type { DevicePositionScope, Position } from 'api/router/router';
import { getTransportersInGeofence } from 'api/geofencing/geofencing';
import type { Geofence } from 'api/geofencing/geofencing';
import type { AccountSettings } from 'api/manager/settings';
import { operatorKeys } from 'queries/operators';
import { routerKeys } from 'queries/router';
import { groupKeys } from 'queries/groups';
import { poiKeys } from 'queries/pointsOfInterest';
import { geofenceKeys } from 'queries/geofences';
import { cleanString } from 'utils/stringUtils';
import { LoadingContext } from 'LoadingContext';
import { useTranslation } from 'react-i18next';
import { useAuth } from "AuthContext";
import { useFeatures } from 'context/features';
import { useArgonController } from 'context';

// Dashboard layout components
import GeneralMap from "layouts/dashboard/components/GeneralMap";
import type { TrailPoint } from "layouts/dashboard/components/GeneralMap";
import { countRecentDevices, countDevicesInMovement, getPercentage, getUnitStatus, filterPositions } from 'layouts/dashboard/utils/dashboard';

const TRAIL_LENGTH = 10;
const GEOFENCING_FEATURE_KEY = 'geofencing';

/** A per-type total shown as a chip / stat summary. */
interface TypeSummaryItem { name: string; total: number; }

interface TransportersProps {
  searchQuery: string;
  settings: AccountSettings;
  setShowGeofence: (value: boolean) => void;
  showGeofence: boolean;
  geofences: Geofence[];
}

function Transporters({ searchQuery, settings, setShowGeofence, showGeofence, geofences }: TransportersProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { setLoading } = useContext(LoadingContext);
  const { isAuthenticated } = useAuth();
  // The live map is platform baseline and is never feature-gated (spec 07 §3).
  // The in-geofence tile and the overlay toggle are the only geofencing-gated
  // surfaces on this screen: without the feature the query answers
  // FEATURE_DISABLED, so the tile is not rendered and the query never fires
  // (spec 07 §16 acceptance 7).
  const { isFeatureEnabled } = useFeatures();
  const geofencingEnabled = isFeatureEnabled(GEOFENCING_FEATURE_KEY);
  const [controller] = useArgonController();
  const { darkMode } = controller;
  const [positions, setPositions] = useState<Position[]>([]);
  const [active, setActive] = useState(0);
  const [movement, setMovement] = useState(0);
  const [inGeofence, setInGeofence] = useState(0);
  const [criticalAlerts, setCriticalAlerts] = useState(0);
  const [selectedTransporter, setSelectedTransporter] = useState<string | null>(null);
  const [typeSummary, setTypeSummary] = useState<TypeSummaryItem[]>([]);
  // Plate/name search lives in the page's top-right search box (searchQuery);
  // the filter bar only narrows by type/group/operator/status.
  const [filters, setFilters] = useState<DashboardFilters>({ transporterType: 'all', groupId: 'all', operatorId: 'all', status: 'all' });
  const [groupOptions, setGroupOptions] = useState<FilterOption[]>([]);
  const [operatorOptions, setOperatorOptions] = useState<FilterOption[]>([]);
  const [pois, setPois] = useState<PointOfInterestLookup[]>([]);
  const [showPois, setShowPois] = useState(false);
  const [followMode, setFollowMode] = useState(false);
  const [showTrail, setShowTrail] = useState(false);
  const [trails, setTrails] = useState<Record<string, TrailPoint[]>>({});
  const poisLoadedRef = useRef(false);
  // A slow round-trip (providers timing out server-side) can outlast the refresh
  // countdown; never stack overlapping position fetches.
  const positionsFetchInFlightRef = useRef(false);
  const positionsLoadedOnceRef = useRef(false);
  const accountIdRef = useRef<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);
  // Group/operator narrowing happens server-side; the refs are read by the polling closure.
  const positionScopeRef = useRef<DevicePositionScope>({});
  const displayedScopeRef = useRef<DevicePositionScope | null>(null);

  useEffect(() => {
    const typesObject = positions.reduce<Record<string, TypeSummaryItem>>((acc, position) => {
      if (!acc[position.transporterType]) {
        acc[position.transporterType] = { name: position.transporterType, total: 0 };
      }
      acc[position.transporterType].total += 1;
      return acc;
    }, {});
    setTypeSummary(Object.values(typesObject));
  }, [positions]);

  // Client-side ring buffer of the last N received points per unit,
  // used to render the trail of the selected unit.
  useEffect(() => {
    if (positions.length === 0) return;
    setTrails(prev => {
      const next: Record<string, TrailPoint[]> = { ...prev };
      positions.forEach(position => {
        const key = position.transporterId || position.deviceName;
        if (!key) return;
        const buffer = next[key] ? [...next[key]] : [];
        const last = buffer[buffer.length - 1];
        if (!last || last.dateTime !== position.deviceDateTime) {
          buffer.push({ lat: position.latitude, lng: position.longitude, dateTime: position.deviceDateTime });
          if (buffer.length > TRAIL_LENGTH) {
            buffer.splice(0, buffer.length - TRAIL_LENGTH);
          }
        }
        next[key] = buffer;
      });
      return next;
    });
  }, [positions]);

  const fetchPositions = async () => {
    if (positionsFetchInFlightRef.current) {
      return;
    }
    positionsFetchInFlightRef.current = true;
    // Only the first load blocks the screen; a periodic refresh shows inline progress on the map.
    const blocking = !positionsLoadedOnceRef.current;
    if (blocking) setLoading(true); else setRefreshing(true);
    const scope = positionScopeRef.current;
    try {
      // A failed or empty refresh keeps the last known positions on the map
      // (the live map continues showing the cached positions it already has).
      // A total failure is surfaced by the global toast and swallowed here.
      const result = await queryClient.fetchQuery({
        queryKey: routerKeys.devicePositions(scope),
        queryFn: ({ signal }) => getDevicePositions(scope, { signal }),
        staleTime: 0,
      });
      // An empty answer only counts as a glitch for the scope already on screen; a newly
      // selected group or operator with no units must show none.
      const superseded = scope !== positionScopeRef.current;
      if (!superseded && Array.isArray(result) && (result.length > 0 || scope !== displayedScopeRef.current)) {
        displayedScopeRef.current = scope;
        setPositions(result);
        setActive(countRecentDevices(result, settings.onlineInterval));
        setMovement(countDevicesInMovement(result, settings.onlineInterval));
      }
    } catch {
      // Keep the last known positions on a failed or cancelled refresh.
    } finally {
      positionsFetchInFlightRef.current = false;
      positionsLoadedOnceRef.current = true;
      if (blocking) setLoading(false); else setRefreshing(false);
    }
    if (scope !== positionScopeRef.current) {
      void fetchPositions();
    }
  };

  const changePositionScope = (scope: DevicePositionScope) => {
    const previous = positionScopeRef.current;
    positionScopeRef.current = scope;
    if (positionsFetchInFlightRef.current) {
      // The aborted fetch notices the new scope when it settles and refetches.
      void queryClient.cancelQueries({ queryKey: routerKeys.devicePositions(previous) });
    } else {
      void fetchPositions();
    }
  };

  const calculateReference = async () => {
    if (!geofencingEnabled) return;
    try {
      const result = await queryClient.fetchQuery({
        queryKey: geofenceKeys.transportersInGeofence,
        queryFn: ({ signal }) => getTransportersInGeofence(null, null, { signal }),
        staleTime: 0,
      });
      // The query returns one row per (geofence, unit) pair; the tile counts units,
      // and a unit inside two overlapping geofences is still one unit.
      setInGeofence(new Set(result.map((item) => item.transporterId)).size);
    } catch(e) {
      // Failure is surfaced by the global toast; keep the previous count.
      console.error(e);
    }
  };

  // Open critical alerts, counted server-side over every open alert the user can see, once when the
  // dashboard opens. A failed read (e.g. permissions) keeps the count at 0.
  const fetchCriticalAlerts = async () => {
    try {
      accountIdRef.current ??= (await getAccountByUser())?.accountId ?? null;
      if (!accountIdRef.current) return;
      const counts = await getOpenAlertCounts(accountIdRef.current);
      setCriticalAlerts(counts.Critical ?? 0);
    } catch {
      setCriticalAlerts(0);
    }
  };

  const fetchFilterOptions = async () => {
    const [groupList, operatorList] = await Promise.all([
      // A failed group read is surfaced by the global toast; keep the group
      // filter empty instead of rejecting the whole options load.
      queryClient.fetchQuery({
        queryKey: groupKeys.lookup(),
        queryFn: ({ signal }) => getGroupLookup({ signal }),
      }).catch((): GroupLookup[] => []),
      // A failed operator read is surfaced by the global toast; keep the
      // operator filter empty instead of rejecting the whole options load.
      queryClient.fetchQuery({
        queryKey: operatorKeys.lookup(),
        queryFn: ({ signal }) => getOperatorLookup({ signal }),
      }).catch((): OperatorLookup[] => []),
    ]);
    setGroupOptions((groupList || []).map(group => ({ value: group.groupId, label: group.name })));
    setOperatorOptions((operatorList || []).map(operator => ({ value: operator.operatorId, label: operator.name })));
  };

  useEffect(() => {
    if (isAuthenticated) {
      calculateReference();
      fetchPositions();
      fetchFilterOptions();
      fetchCriticalAlerts();
    }
  }, [isAuthenticated]);

  const handleSelected = (selected: string | null) => {
    setSelectedTransporter(selected);
  };

  const handleFilterChange = (name: string, value: string | number) => {
    const next = { ...filters, [name]: value } as DashboardFilters;
    setFilters(next);
    if (name === 'groupId' || name === 'operatorId') {
      changePositionScope({
        groupId: next.groupId === 'all' ? null : Number(next.groupId),
        operatorId: next.operatorId === 'all' ? null : next.operatorId,
      });
    }
  };

  const handleTogglePois = async () => {
    if (!showPois && !poisLoadedRef.current) {
      setLoading(true);
      // The overlay renders pin colours and popup type/description/address, all
      // of which `pointOfInterestLookup` now carries.
      const result = await queryClient.fetchQuery({
        queryKey: poiKeys.lookup(),
        queryFn: ({ signal }) => getPointOfInterestLookup({ signal }),
      }).catch((): PointOfInterestLookup[] => []);
      setPois(result);
      poisLoadedRef.current = true;
      setLoading(false);
    }
    setShowPois(value => !value);
  };

  const handleFollowDisengage = useCallback(() => {
    setFollowMode(false);
  }, []);

  // Group/operator scope the fetched set (stats and table follow them); type/status narrow the map only.
  const filteredPositions = useMemo(
    () => filterPositions(positions, {
      transporterType: filters.transporterType,
      status: filters.status,
      onlineInterval: settings.onlineInterval,
    }),
    [positions, filters.transporterType, filters.status, settings.onlineInterval]
  );

  const typeOptions = useMemo<FilterOption[]>(
    () => typeSummary.map(({ name }) => ({
      value: name,
      label: t(`transporterTypes.${cleanString(name)}` as 'transporterTypes.car')
    })),
    [typeSummary, t]
  );

  const selectedTrail = useMemo<TrailPoint[]>(() => {
    if (!selectedTransporter) return [];
    const position = positions.find(p => p.deviceName === selectedTransporter);
    if (!position) return [];
    const key = position.transporterId || position.deviceName;
    return trails[key] || [];
  }, [selectedTransporter, positions, trails]);

  const selectedPosition = useMemo(
    () => positions.find((position) => position.deviceName === selectedTransporter) ?? null,
    [positions, selectedTransporter]
  );

  const stats: FleetStat[] = [
    { title: t("dashboard.totalTitle"), count: positions.length },
    { title: t("dashboard.activeTitle"), count: active, share: getPercentage(active, positions.length) },
    { title: t("dashboard.movementTitle"), count: movement, share: getPercentage(movement, positions.length) },
    ...(geofencingEnabled
      ? [{ title: t("dashboard.inGeofence"), count: inGeofence, share: getPercentage(inGeofence, positions.length) }]
      : []),
    { title: t("dashboard.criticalAlerts"), count: criticalAlerts },
  ];

  const layerChips = [
    ...(geofencingEnabled
      ? [{ label: t('screen.geofence'), on: showGeofence, toggle: () => setShowGeofence(!showGeofence) }]
      : []),
    { label: t('dashboard.poiLayer'), on: showPois, toggle: handleTogglePois },
    { label: t('dashboard.followMode'), on: followMode, toggle: () => setFollowMode(value => !value), disabled: !selectedTransporter },
    { label: t('dashboard.trail'), on: showTrail, toggle: () => setShowTrail(value => !value) },
  ];

  return (
    <ArgonBox
      sx={({ breakpoints }) => ({
        display: 'flex',
        flexDirection: 'column',
        // Top bar (56px + its 24px gap) and the layout's 24px bottom padding.
        [breakpoints.up('lg')]: { height: 'calc(100vh - 104px)', minHeight: 480 },
      })}
    >
        <FilterBar
            stats={stats}
            typeOptions={typeOptions}
            groupOptions={groupOptions}
            operatorOptions={operatorOptions}
            filters={filters}
            onFilterChange={handleFilterChange}
        />
        <ArgonBox
          sx={({ breakpoints }) => ({
            position: 'relative',
            display: 'flex',
            flexDirection: 'column',
            gap: 2,
            [breakpoints.up('lg')]: { flexDirection: 'row', flex: 1, minHeight: 0 },
          })}
        >
            <ArgonBox
              sx={({ breakpoints, borders: { borderWidth, borderColor, borderRadius } }) => ({
                position: 'relative',
                height: '60vh',
                border: `${borderWidth[1]} solid ${borderColor}`,
                borderRadius: borderRadius.lg,
                overflow: 'hidden',
                '& .map-container': { height: '100%', borderRadius: 0 },
                [breakpoints.up('lg')]: { flex: 1, minWidth: 0, height: 'auto' },
              })}
            >
                <GeneralMap
                    mapType={settings.maps as 'OSM' | 'Google'}
                    positions={filteredPositions}
                    mapKey={settings.mapsKey}
                    selectedMarker={selectedTransporter}
                    geofences={geofences}
                    showGeofence={showGeofence}
                    handleSelected={handleSelected}
                    onlineInterval={settings.onlineInterval}
                    pois={pois}
                    showPois={showPois}
                    trail={selectedTrail}
                    showTrail={showTrail && !!selectedTransporter}
                    followUnit={followMode ? selectedTransporter : null}
                    onFollowDisengage={handleFollowDisengage}
                    darkMode={darkMode}
                    height="100%"/>
                <ArgonBox
                  display="flex"
                  flexWrap="wrap"
                  alignItems="center"
                  gap={1}
                  sx={{
                    position: 'absolute',
                    top: 12,
                    left: 12,
                    right: 64,
                    zIndex: 1000,
                    pointerEvents: 'none',
                    '& > *': { pointerEvents: 'auto' },
                  }}
                >
                    {layerChips.map(({ label, on, toggle, disabled }) => (
                        <Chip
                            key={label}
                            label={label}
                            size="small"
                            clickable
                            disabled={disabled}
                            aria-pressed={on}
                            color={on ? 'primary' : 'default'}
                            variant={on ? 'filled' : 'outlined'}
                            onClick={toggle}
                            sx={{ bgcolor: on ? undefined : 'background.paper', boxShadow: 1 }}
                        />
                    ))}
                    <ArgonBox ml="auto">
                        <RefreshCounter
                            settings={settings}
                            refreshing={refreshing}
                            fetchPositions={fetchPositions}
                            calculateReference={calculateReference} />
                    </ArgonBox>
                </ArgonBox>
                {selectedPosition && (
                    <ArgonBox
                      sx={({ breakpoints }) => ({
                        position: 'absolute',
                        left: 12,
                        right: 12,
                        bottom: 28,
                        zIndex: 1000,
                        [breakpoints.up('md')]: { right: 'auto', width: 480 },
                      })}
                    >
                        <UnitCard
                            position={selectedPosition}
                            status={getUnitStatus(selectedPosition, settings.onlineInterval)}
                            onClose={() => handleSelected(null)} />
                    </ArgonBox>
                )}
            </ArgonBox>
            <Card
              sx={({ breakpoints }) => ({
                overflow: 'hidden',
                height: '60vh',
                '& .MuiTableContainer-root': { border: 0, borderRadius: 0 },
                [breakpoints.up('lg')]: { flex: 'none', width: 332, height: 'auto' },
              })}
            >
                <ArgonBox
                  px={2}
                  py={1.25}
                  sx={({ borders: { borderWidth, borderColor } }) => ({
                    borderBottom: `${borderWidth[1]} solid ${borderColor}`,
                  })}
                >
                    <ArgonBox display="flex" alignItems="baseline" justifyContent="space-between">
                        <ArgonTypography variant="button" fontWeight="bold">
                            {t("dashboard.transportersTitle")}
                        </ArgonTypography>
                        <ArgonTypography variant="caption" color="secondary">
                            {positions.length}
                        </ArgonTypography>
                    </ArgonBox>
                    <ArgonTypography variant="caption" color="secondary" display="block">
                        {typeSummary
                          .map(({ name, total }) => `${t(`transporterTypes.${cleanString(name)}` as 'transporterTypes.car')} ${total}`)
                          .join(' · ')}
                    </ArgonTypography>
                </ArgonBox>
                <ArgonBox sx={{ flex: 1, minHeight: 0, display: 'flex', flexDirection: 'column' }}>
                    <TransportersTable
                        transporters={positions}
                        selected={selectedTransporter}
                        handleSelected={handleSelected}
                        searchQuery={searchQuery}
                        maxHeight="100%"
                        onlineInterval={settings.onlineInterval}/>
                </ArgonBox>
            </Card>
        </ArgonBox>
    </ArgonBox>
  );
}
export default Transporters;
