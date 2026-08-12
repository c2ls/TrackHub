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

import { useState, useRef, useEffect, useMemo, useCallback } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { Trip } from 'api/router/router';

// Duration of a full trip playback at 1x speed.
const BASE_DURATION_MS = 60000;

/** A single point of a trip (the shape playback interpolates over). */
export type TripPoint = Trip['points'][number];

/** Normalized playback timeline: sorted points + their 0..1 timestamps. */
export interface Timeline {
  points: TripPoint[];
  stamps: number[];
}

/** Interpolated marker position emitted by the playback engine. */
export interface PlaybackPosition {
  lat: number;
  lng: number;
  course: number;
}

/** Public API returned by {@link usePlayback}. */
export interface UsePlaybackResult {
  playing: boolean;
  toggle: () => void;
  speed: number;
  setSpeed: Dispatch<SetStateAction<number>>;
  progress: number;
  seek: (value: number) => void;
  position: PlaybackPosition | null;
  hasTimeline: boolean;
}

/**
 * Builds a normalized timeline from trip points: each point gets a 0..1
 * timestamp proportional to its deviceDateTime delta within the trip.
 * @param points - Trip points ({ latitude, longitude, course, deviceDateTime }).
 * @returns { points, stamps } or null when no usable points.
 */
export function buildTimeline(points: TripPoint[] | null | undefined): Timeline | null {
  if (!points || points.length === 0) return null;
  const sorted = points
    .filter(point => point.deviceDateTime && !isNaN(new Date(point.deviceDateTime).getTime()))
    .sort((a, b) => new Date(a.deviceDateTime).getTime() - new Date(b.deviceDateTime).getTime());
  if (sorted.length === 0) return null;
  const start = new Date(sorted[0].deviceDateTime).getTime();
  const end = new Date(sorted[sorted.length - 1].deviceDateTime).getTime();
  const total = Math.max(end - start, 1);
  const stamps = sorted.map(point => (new Date(point.deviceDateTime).getTime() - start) / total);
  return { points: sorted, stamps };
}

const toPosition = (point: TripPoint): PlaybackPosition => ({
  lat: point.latitude,
  lng: point.longitude,
  course: point.course || 0
});

/**
 * Interpolates the playback position for a progress value (0..1), linearly
 * between the two surrounding points proportional to their time deltas.
 * @param timeline - Result of buildTimeline.
 * @param progress - Playback progress 0..1.
 * @returns { lat, lng, course } or null.
 */
export function interpolatePosition(timeline: Timeline | null, progress: number): PlaybackPosition | null {
  if (!timeline) return null;
  const { points, stamps } = timeline;
  if (points.length === 1 || progress <= 0) return toPosition(points[0]);
  if (progress >= 1) return toPosition(points[points.length - 1]);
  let index = 1;
  while (index < stamps.length && stamps[index] < progress) index += 1;
  if (index >= points.length) return toPosition(points[points.length - 1]);
  const previous = points[index - 1];
  const next = points[index];
  const span = stamps[index] - stamps[index - 1];
  const k = span > 0 ? (progress - stamps[index - 1]) / span : 1;
  return {
    lat: previous.latitude + (next.latitude - previous.latitude) * k,
    lng: previous.longitude + (next.longitude - previous.longitude) * k,
    course: next.course ?? previous.course ?? 0
  };
}

/**
 * Client-side trip playback state: play/pause, speed multiplier, timeline
 * progress and the interpolated marker position. Purely client-side over
 * already-fetched trip points.
 * @param points - Points of the selected trip.
 * @returns Playback API.
 */
export function usePlayback(points: TripPoint[] | null | undefined): UsePlaybackResult {
  const timeline = useMemo(() => buildTimeline(points), [points]);
  const [playing, setPlaying] = useState(false);
  const [speed, setSpeed] = useState(1);
  const [progress, setProgress] = useState(0);
  const progressRef = useRef(0);
  const speedRef = useRef(1);
  const frameRef = useRef<number | null>(null);
  const lastTickRef = useRef<number | null>(null);

  useEffect(() => {
    speedRef.current = speed;
  }, [speed]);

  // Reset when the selected trip changes.
  useEffect(() => {
    setPlaying(false);
    progressRef.current = 0;
    setProgress(0);
  }, [timeline]);

  useEffect(() => {
    if (!playing || !timeline) return undefined;
    lastTickRef.current = performance.now();
    const step = (now: number) => {
      const delta = now - lastTickRef.current!;
      lastTickRef.current = now;
      const next = progressRef.current + (delta * speedRef.current) / BASE_DURATION_MS;
      if (next >= 1) {
        progressRef.current = 1;
        setProgress(1);
        setPlaying(false);
        return;
      }
      progressRef.current = next;
      setProgress(next);
      frameRef.current = requestAnimationFrame(step);
    };
    frameRef.current = requestAnimationFrame(step);
    return () => {
      if (frameRef.current) cancelAnimationFrame(frameRef.current);
    };
  }, [playing, timeline]);

  const toggle = useCallback(() => {
    if (!timeline) return;
    setPlaying(previous => {
      if (!previous && progressRef.current >= 1) {
        progressRef.current = 0;
        setProgress(0);
      }
      return !previous;
    });
  }, [timeline]);

  const seek = useCallback((value: number) => {
    const clamped = Math.min(Math.max(value, 0), 1);
    progressRef.current = clamped;
    setProgress(clamped);
  }, []);

  const position = useMemo(() => interpolatePosition(timeline, progress), [timeline, progress]);

  return {
    playing,
    toggle,
    speed,
    setSpeed,
    progress,
    seek,
    position,
    hasTimeline: !!timeline
  };
}
