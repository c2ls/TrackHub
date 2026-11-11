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

import { useMemo } from 'react';
import { calculateTotalDistance, formatDistance } from 'utils/distanceUtils';
import { formatISODuration } from 'utils/timeUtils';
import { useTranslation } from 'react-i18next';
import { TRIP_TYPE } from 'controls/Maps/core/mapTypes';
import type { Trip } from 'api/router/router';

/** Aggregated (or single-trip) statistics rendered in the panel. */
interface TripStats {
  title?: string;
  distance: string;
  duration: number;
  durationIso: string;
  maxSpeed: number;
  avgSpeed: number;
  stops?: number;
  alarms: number;
}

interface TripStatsPanelProps {
  trips?: Trip[];
  selectedTrip?: string | null;
}

const TripStatsPanel = ({ trips = [], selectedTrip }: TripStatsPanelProps) => {
  const { t } = useTranslation();
  const stats = useMemo<TripStats | null>(() => {
    if (!trips || trips.length === 0) return null;

    const findTrip = (id: string) => trips.find(t => t.tripId === id);

    const parseDurationToSeconds = (d: unknown): number => {
      if (!d && d !== 0) return 0;
      if (typeof d === 'number') return d;
      const m = String(d).match(/^P(?:(\d+)D)?(?:T(?:(\d+)H)?(?:(\d+)M)?(?:(\d+(?:\.\d+)?)S)?)?$/);
      if (!m) return 0;
      const days = Number(m[1] || 0);
      const h = Number(m[2] || 0);
      const mm = Number(m[3] || 0);
      const s = Math.round(Number(m[4] || 0));
      return days * 86400 + h * 3600 + mm * 60 + s;
    };

    const secondsToISODuration = (secs: number): string => {
      const days = Math.floor(secs / 86400);
      const h = Math.floor((secs % 86400) / 3600);
      const m = Math.floor((secs % 3600) / 60);
      const s = secs % 60;
      return `P${days > 0 ? `${days}D` : ''}T${h}H${m}M${s}S`;
    };

    if (selectedTrip) {
      const trip = findTrip(selectedTrip);
      if (!trip) return null;
      // A stop or a no-data stretch ends on the first point of the next trip, whose speed is not its own.
      const isMoving = trip.type === TRIP_TYPE.moving;
      const maxSpeed = isMoving ? Math.max(...(trip.points.map(p => p.speed || 0)), 0) : 0;
      const avgSpeed = isMoving ? trip.averageSpeed : 0;
      const alarms = trip.points.filter(p => p.eventId).length;
      return {
        title: `${trip.from} → ${trip.to}`,
        distance: formatDistance(trip.totalDistance || 0),
        duration: parseDurationToSeconds(trip.duration),
        durationIso: typeof trip.duration === 'string' ? trip.duration : secondsToISODuration(parseDurationToSeconds(trip.duration)),
        maxSpeed,
        avgSpeed,
        alarms
      };
    }

    // Aggregate
    const totalDistance = calculateTotalDistance(trips, 'totalDistance');
    const totalDuration = trips.reduce((s, t) => s + parseDurationToSeconds(t.duration), 0);
    const maxSpeed = Math.max(...trips.flatMap(t => t.points.map(p => p.speed || 0)), 0);
    const moving = trips.filter(t => t.type === TRIP_TYPE.moving);
    const avgSpeed = moving.reduce((s, t) => s + (t.averageSpeed || 0), 0) / Math.max(1, moving.length);
    const stops = trips.filter(t => t.type === TRIP_TYPE.stop).length;
    // Consecutive segments share their boundary point; count it once.
    const alarms = trips.reduce((s, t, i) => s + t.points.slice(i > 0 ? 1 : 0).filter(p => p.eventId).length, 0);

    return {
      distance: totalDistance,
      duration: totalDuration,
      durationIso: secondsToISODuration(totalDuration),
      maxSpeed,
      avgSpeed,
      stops,
      alarms
    };
  }, [trips, selectedTrip]);

  const formatDurationString = (seconds: number, iso: string): string => {
    if (iso) return formatISODuration(iso);
    if (!seconds || seconds <= 0) return '0h 0m';
    const hrs = Math.floor(seconds / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    return `${hrs}h ${mins}m`;
  };

  if (!stats) return null;

  return (
    <div className="trips-stats-panel">
      <div className="trips-stats-header">
        <strong>{t("tripPanel.summaryLabel")}</strong>
      </div>
      <div className="trips-stats-body">
        <div className="stats-row"><span>{t("tripPanel.totalDistance")}</span><span>{stats.distance}</span></div>
        <div className="stats-row"><span>{t("tripPanel.duration")}</span><span>{formatDurationString(stats.duration, stats.durationIso)}</span></div>
        <div className="stats-row"><span>{t("tripPanel.maxSpeed")}</span><span>{Math.round(stats.maxSpeed)} km/h</span></div>
        <div className="stats-row"><span>{t("tripPanel.avgSpeed")}</span><span>{Math.round(stats.avgSpeed)} km/h</span></div>
        {stats.stops !== undefined && <div className="stats-row"><span>{t("tripPanel.stops")}</span><span>{stats.stops}</span></div>}
        <div className="stats-row"><span>{t("tripPanel.alarms")}</span><span>{stats.alarms}</span></div>
      </div>
    </div>
  );
};

export default TripStatsPanel;
