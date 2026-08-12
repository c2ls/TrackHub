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

import { useEffect, useRef } from 'react';
import { useMap } from 'react-leaflet';
import L from 'leaflet';
import { createSvgIcon } from 'controls/Maps/utils/imageUtils';
import type { PlaybackPosition } from 'controls/Maps/core/mapTypes';

const PLAYBACK_COLOR = '#7B1FA2';

const buildIcon = (course: number): L.DivIcon => L.divIcon({
  className: 'playback-icon',
  html: createSvgIcon(course || 0, '', 'svg', { color: PLAYBACK_COLOR }),
  iconSize: [30, 30],
  iconAnchor: [15, 15]
});

interface OSMPlaybackMarkerProps {
  position: PlaybackPosition;
}

const OSMPlaybackMarker = ({ position }: OSMPlaybackMarkerProps) => {
  const map = useMap();
  const markerRef = useRef<L.Marker | null>(null);
  const courseRef = useRef<number | null>(null);

  useEffect(() => () => {
    if (markerRef.current) {
      markerRef.current.remove();
      markerRef.current = null;
    }
  }, [map]);

  useEffect(() => {
    if (!map || !position) return;
    const course = position.course || 0;
    if (!markerRef.current) {
      markerRef.current = L.marker([position.lat, position.lng], {
        icon: buildIcon(course),
        interactive: false,
        zIndexOffset: 1000
      }).addTo(map);
      courseRef.current = course;
    } else {
      markerRef.current.setLatLng([position.lat, position.lng]);
      if (courseRef.current !== course) {
        markerRef.current.setIcon(buildIcon(course));
        courseRef.current = course;
      }
    }
  }, [map, position]);

  return null;
};

export default OSMPlaybackMarker;
