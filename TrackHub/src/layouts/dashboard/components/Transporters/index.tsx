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
import Grid from "@mui/material/Grid";
import Chip from "@mui/material/Chip";
import ArgonBox from "components/ArgonBox";
import DetailedStatisticsCard from "controls/Cards/StatisticsCards/DetailedStatisticsCard";
import TransportersTable from "layouts/dashboard/components/TransportersTable";
import RefreshCounter from 'layouts/dashboard/components/RefreshCounter';
import FilterBar from 'layouts/dashboard/components/Transporters/FilterBar';
import type { FilterOption, DashboardFilters } from 'layouts/dashboard/components/Transporters/FilterBar';
import { getPointsOfInterestByAccount } from 'api/manager/pointsOfInterest';
import type { PointOfInterest } from 'api/manager/pointsOfInterest';
import { getGroups } from 'api/manager/groups';
import type { Group } from 'api/manager/groups';
import { useQueryClient } from '@tanstack/react-query';
import { getTransportersByGroup, getTransporterDeviceAssignmentsByAccount } from 'api/manager/transporters';
import type { Transporter, TransporterAssignmentWithAudit } from 'api/manager/transporters';
import { getOperators } from 'api/manager/operators';
import type { OperatorSummary } from 'api/manager/operators';
import { getDevicesByAccount } from 'api/manager/devices';
import type { Device } from 'api/manager/devices';
import { getAccountByUser } from 'api/manager/accounts';
import { getAlertEvents } from 'api/manager/alertEvents';
import { getDevicePositions } from 'api/router/router';
import type { Position } from 'api/router/router';
import { getTransportersInGeofence } from 'api/geofencing/geofencing';
import type { Geofence } from 'api/geofencing/geofencing';
import type { AccountSettings } from 'api/manager/settings';
import { transporterKeys } from 'queries/transporters';
import { operatorKeys } from 'queries/operators';
import { routerKeys } from 'queries/router';
import { deviceKeys } from 'queries/devices';
import { groupKeys } from 'queries/groups';
import { poiKeys } from 'queries/pointsOfInterest';
import { geofenceKeys } from 'queries/geofences';
import { cleanString } from 'utils/stringUtils';
import { LoadingContext } from 'LoadingContext';
import { useTranslation } from 'react-i18next';
import { useAuth } from "AuthContext";
import { useArgonController } from 'context';

// Dashboard layout components
import GeneralMap from "layouts/dashboard/components/GeneralMap";
import type { TrailPoint } from "layouts/dashboard/components/GeneralMap";
import MapControlStyle from 'controls/Maps/styles/MapControl';
import { countRecentDevices, countDevicesInMovement, getPercentage, filterPositions } from 'layouts/dashboard/utils/dashboard';

const TRAIL_LENGTH = 10;

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
  const [controller] = useArgonController();
  const { darkMode } = controller;
  const [positions, setPositions] = useState<Position[]>([]);
  const [active, setActive] = useState(0);
  const [movement, setMovement] = useState(0);
  const [inGeofence, setInGeofence] = useState(0);
  const [criticalAlerts, setCriticalAlerts] = useState(0);
  const [selectedTransporter, setSelectedTransporter] = useState<string | null>(null);
  const [typeSummary, setTypeSummary] = useState<TypeSummaryItem[]>([]);
  // Distance from the page top to the map row plus the fixed below-map spacing (70px). The header
  // stack above the map (navbar, tabs, stat cards, filter bar) is width-dependent — the filter bar
  // wraps on smaller screens — so it is measured, not hardcoded: the dashboard must always fit the
  // viewport with no page-level scroll (the map view and the side grid's own scroll depend on it).
  const [mapViewportOffset, setMapViewportOffset] = useState(371);
  const [tableHeight, setTableHeight] = useState('calc(100vh - 371px)');
  // Plate/name search lives in the page's top-right search box (searchQuery);
  // the filter bar only narrows by type/group/operator/status.
  const [filters, setFilters] = useState<DashboardFilters>({ transporterType: 'all', groupId: 'all', operatorId: 'all', status: 'all' });
  const [groupOptions, setGroupOptions] = useState<FilterOption[]>([]);
  const [operatorOptions, setOperatorOptions] = useState<FilterOption[]>([]);
  // Sets of transporterIds the selected group/operator maps to; null = no narrowing.
  const [groupTransporterIds, setGroupTransporterIds] = useState<Set<string> | null>(null);
  const [operatorTransporterIds, setOperatorTransporterIds] = useState<Set<string> | null>(null);
  const [pois, setPois] = useState<PointOfInterest[]>([]);
  const [showPois, setShowPois] = useState(false);
  const [followMode, setFollowMode] = useState(false);
  const [showTrail, setShowTrail] = useState(false);
  const [trails, setTrails] = useState<Record<string, TrailPoint[]>>({});
  const chipContainerRef = useRef<HTMLDivElement>(null);
  const poisLoadedRef = useRef(false);
  // A slow round-trip (providers timing out server-side) can outlast the refresh
  // countdown; never stack overlapping position fetches.
  const positionsFetchInFlightRef = useRef(false);
  // Lazy membership caches: fetched once per selected group/operator.
  const groupMembershipCacheRef = useRef<Map<string | number, Set<string>>>(new Map());
  const operatorMembershipCacheRef = useRef<Map<string | number, Set<string>>>(new Map());
  // Account-wide device list + device-transporter assignments, fetched once
  // on the first operator selection to map an operator to its transporters.
  const operatorMappingRef = useRef<{ devices: Device[]; assignments: TransporterAssignmentWithAudit[] } | null>(null);

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

  // Self-fitting: absorb any page-level overflow into the offset whenever the app content's
  // height changes (the #root element grows with content; ResizeObserver on it also catches
  // font/wrap shifts that happen without a React render). The header stack (cards, filter bar)
  // wraps differently per width, so this converges to an exact viewport fit instead of chasing a
  // constant; a window resize re-fits from the base.
  useEffect(() => {
    let frame = 0;
    const fit = () => {
      cancelAnimationFrame(frame);
      frame = requestAnimationFrame(() => {
        const doc = document.documentElement;
        const root = document.getElementById('root') ?? document.body;
        const excess = doc.scrollHeight - doc.clientHeight;
        if (excess > 0) {
          // Page overflows: shrink the map/grid by the overflow.
          setMapViewportOffset((offset) => offset + excess);
          return;
        }
        // Page fits with room to spare: grow the map/grid into the free space.
        const slack = doc.clientHeight - Math.ceil(root.getBoundingClientRect().bottom);
        if (slack > 1) {
          setMapViewportOffset((offset) => Math.max(200, offset - slack));
        }
      });
    };
    const observer = new ResizeObserver(fit);
    observer.observe(document.getElementById('root') ?? document.body);
    window.addEventListener('resize', fit);
    return () => {
      cancelAnimationFrame(frame);
      observer.disconnect();
      window.removeEventListener('resize', fit);
    };
  }, []);

  useEffect(() => {
    if (chipContainerRef.current) {
      const chipHeight = chipContainerRef.current.offsetHeight;
      setTableHeight(`calc(100vh - ${chipHeight + mapViewportOffset}px)`); // Viewport minus the measured header stack and chip row
    }
  }, [typeSummary, mapViewportOffset]);

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
    setLoading(true);
    try {
      // A failed or empty refresh keeps the last known positions on the map
      // (the live map continues showing the cached positions it already has).
      // A total failure is surfaced by the global toast and swallowed here.
      const result = await queryClient.fetchQuery({
        queryKey: routerKeys.devicePositions(),
        queryFn: getDevicePositions,
        staleTime: 0,
      });
      if (Array.isArray(result) && result.length > 0) {
        setPositions(result);
        setActive(countRecentDevices(result, settings.onlineInterval));
        setMovement(countDevicesInMovement(result));
      }
    } catch {
      // Keep the last known positions on a failed refresh.
    } finally {
      positionsFetchInFlightRef.current = false;
      setLoading(false);
    }
  };

  const calculateReference = async () => {
    try {
      const result = await queryClient.fetchQuery({
        queryKey: geofenceKeys.transportersInGeofence,
        queryFn: () => getTransportersInGeofence(),
        staleTime: 0,
      });
      setInGeofence(result.length);
    } catch(e) {
      // Failure is surfaced by the global toast; keep the previous count.
      console.error(e);
    }
  };

  // Open critical alerts: anything not acknowledged/resolved with
  // Critical severity. A failed read (e.g. permissions) keeps the count at 0.
  const fetchCriticalAlerts = async () => {
    try {
      const account = await getAccountByUser();
      if (!account?.accountId) return;
      const events = await getAlertEvents(account.accountId, 0, 100);
      const closedStatuses = ['acknowledged', 'resolved'];
      setCriticalAlerts((events || []).filter(event =>
        (event.severity || '').toLowerCase() === 'critical' &&
        !closedStatuses.includes((event.status || '').toLowerCase())
      ).length);
    } catch {
      setCriticalAlerts(0);
    }
  };

  const fetchFilterOptions = async () => {
    const [groupList, operatorList] = await Promise.all([
      // A failed group read is surfaced by the global toast; keep the group
      // filter empty instead of rejecting the whole options load.
      queryClient.fetchQuery({
        queryKey: groupKeys.byAccount(),
        queryFn: getGroups,
      }).catch((): Group[] => []),
      // A failed operator read is surfaced by the global toast; keep the
      // operator filter empty instead of rejecting the whole options load.
      queryClient.fetchQuery({
        queryKey: operatorKeys.summary(),
        queryFn: getOperators,
      }).catch((): OperatorSummary[] => []),
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

  // Resolves the transporter ids of a group, lazily and once per group.
  const resolveGroupMembership = async (groupId: string | number) => {
    if (!groupId || groupId === 'all') {
      setGroupTransporterIds(null);
      return;
    }
    const cache = groupMembershipCacheRef.current;
    if (cache.has(groupId)) {
      setGroupTransporterIds(cache.get(groupId)!);
      return;
    }
    setLoading(true);
    try {
      let transportersInGroup: Transporter[];
      try {
        // Group filter option values are numeric group ids; 'all' is filtered
        // out above, so a real selection is always a number at runtime.
        transportersInGroup = await queryClient.fetchQuery({
          queryKey: transporterKeys.byGroup(groupId as number),
          queryFn: () => getTransportersByGroup(groupId as number),
        });
      } catch {
        // Fetch failed (already surfaced by the global toast): keep the map
        // un-narrowed instead of blanking it.
        setGroupTransporterIds(null);
        return;
      }
      const ids = new Set(transportersInGroup.map(item => item.transporterId));
      cache.set(groupId, ids);
      setGroupTransporterIds(ids);
    } finally {
      setLoading(false);
    }
  };

  // Resolves the transporter ids served by an operator, lazily and once per
  // operator: account devices (operatorId per device) joined with the active
  // device-transporter assignments (deviceId -> transporterId).
  const resolveOperatorMembership = async (operatorId: string | number) => {
    if (!operatorId || operatorId === 'all') {
      setOperatorTransporterIds(null);
      return;
    }
    const cache = operatorMembershipCacheRef.current;
    if (cache.has(operatorId)) {
      setOperatorTransporterIds(cache.get(operatorId)!);
      return;
    }
    setLoading(true);
    try {
      if (!operatorMappingRef.current) {
        let devices: Device[];
        let assignments: TransporterAssignmentWithAudit[];
        try {
          [devices, assignments] = await Promise.all([
            queryClient.fetchQuery({
              queryKey: deviceKeys.byAccount(),
              queryFn: getDevicesByAccount,
            }),
            queryClient.fetchQuery({
              queryKey: transporterKeys.assignmentsByAccount(settings?.accountId ?? '', true),
              queryFn: () => getTransporterDeviceAssignmentsByAccount(settings?.accountId, true),
            })
          ]);
        } catch {
          // Assignments fetch failed (surfaced by the global toast): keep un-narrowed.
          setOperatorTransporterIds(null);
          return;
        }
        if (!Array.isArray(devices)) {
          setOperatorTransporterIds(null);
          return;
        }
        operatorMappingRef.current = { devices, assignments: assignments || [] };
      }
      const { devices, assignments } = operatorMappingRef.current;
      const deviceIds = new Set(devices
        .filter(device => device.operatorId === operatorId)
        .map(device => device.deviceId));
      const ids = new Set(assignments
        .filter(assignment => deviceIds.has(assignment.deviceId))
        .map(assignment => assignment.transporterId));
      cache.set(operatorId, ids);
      setOperatorTransporterIds(ids);
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (name: string, value: string | number) => {
    setFilters(prev => ({ ...prev, [name]: value }) as DashboardFilters);
    if (name === 'groupId') {
      resolveGroupMembership(value);
    } else if (name === 'operatorId') {
      resolveOperatorMembership(value);
    }
  };

  const handleTogglePois = async () => {
    if (!showPois && !poisLoadedRef.current) {
      setLoading(true);
      const result = await queryClient.fetchQuery({
        queryKey: poiKeys.byAccount(),
        queryFn: getPointsOfInterestByAccount,
      }).catch((): PointOfInterest[] => []);
      setPois(result);
      poisLoadedRef.current = true;
      setLoading(false);
    }
    setShowPois(value => !value);
  };

  const handleFollowDisengage = useCallback(() => {
    setFollowMode(false);
  }, []);

  // Filters narrow the map view only; stats and table reflect the full set.
  const filteredPositions = useMemo(
    () => filterPositions(positions, {
      ...filters,
      onlineInterval: settings.onlineInterval,
      groupTransporterIds,
      operatorTransporterIds
    }),
    [positions, filters, settings.onlineInterval, groupTransporterIds, operatorTransporterIds]
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

  return (
    <ArgonBox py={1}>
        <Grid container spacing={3} mb={1}>
            <Grid size={{xs: 12, md:6, lg:2.4}}>
                <DetailedStatisticsCard
                    title={t("dashboard.totalTitle")}
                    count={positions.length}
                    icon={{ color: "info", component: <i className="ni ni-map-big" /> }}
                    percentage={{ color: "success", count: "", hide: true }}
                />
            </Grid>
            <Grid size={{xs: 12, md:6, lg:2.4}}>
                <DetailedStatisticsCard
                    title={t("dashboard.activeTitle")}
                    count={active}
                    icon={{ color: "error", component: <i className="ni ni-watch-time" /> }}
                    percentage={{ color: "success", count: `${getPercentage(active, positions.length)}%`, hide: false }}
                />
            </Grid>
            <Grid size={{xs: 12, md:6, lg:2.4}}>
                <DetailedStatisticsCard
                    title={t("dashboard.movementTitle")}
                    count={movement}
                    icon={{ color: "success", component: <i className="ni ni-button-play" /> }}
                    percentage={{ color: "error", count: `${getPercentage(movement, positions.length)}%`, hide: false }}
                />
            </Grid>
            <Grid size={{xs: 12, md:6, lg:2.4}}>
                <DetailedStatisticsCard
                    title={t("dashboard.inGeofence")}
                    count={inGeofence}
                    icon={{
                      color: "warning",
                      onClick: () => setShowGeofence(!showGeofence),
                      component: <i className="ni ni-pin-3" /> }}
                    percentage={{ color: "success", count: `${getPercentage(inGeofence, positions.length)}%`, hide: false }}
                />
            </Grid>
            <Grid size={{xs: 12, md:6, lg:2.4}}>
                <DetailedStatisticsCard
                    title={t("dashboard.criticalAlerts")}
                    count={criticalAlerts}
                    icon={{ color: "error", component: <i className="ni ni-notification-70" /> }}
                    percentage={{ color: "success", count: "", hide: true }}
                />
            </Grid>
        </Grid>
        <FilterBar
            typeOptions={typeOptions}
            groupOptions={groupOptions}
            operatorOptions={operatorOptions}
            filters={filters}
            onFilterChange={handleFilterChange}
            showPois={showPois}
            onTogglePois={handleTogglePois}
            followMode={followMode}
            onToggleFollow={() => setFollowMode(value => !value)}
            followDisabled={!selectedTransporter}
            showTrail={showTrail}
            onToggleTrail={() => setShowTrail(value => !value)}
        />
        <Grid container spacing={3}>
            <Grid size={{xs: 12, lg:9}}>
            <MapControlStyle>
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
                    height={`calc(100vh - ${mapViewportOffset}px)`}/>
                <RefreshCounter
                    settings={settings}
                    fetchPositions={fetchPositions}
                    calculateReference={calculateReference} />
            </MapControlStyle>
            </Grid>
            <Grid size={{xs: 12, lg:3}}>
                <ArgonBox ref={chipContainerRef} mb={2} display="flex" flexWrap="wrap" gap={1}>
                    {typeSummary.map(({ name, total }) => (
                        <Chip
                            key={name}
                            label={`${t(`transporterTypes.${cleanString(name)}` as 'transporterTypes.car')}: ${total}`}
                            size="small"
                            color="primary"
                            variant="outlined"
                        />
                    ))}
                </ArgonBox>
                <TransportersTable
                    transporters={positions}
                    selected={selectedTransporter}
                    handleSelected={handleSelected}
                    searchQuery={searchQuery}
                    maxHeight={tableHeight}/>
            </Grid>
        </Grid>
    </ArgonBox>
  );
}

export default Transporters;
