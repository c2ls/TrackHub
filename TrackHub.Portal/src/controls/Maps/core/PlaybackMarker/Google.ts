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
import { useGoogleMap } from '@react-google-maps/api';
import { createSvgIcon } from 'controls/Maps/utils/imageUtils';
import type { PlaybackPosition } from 'controls/Maps/core/mapTypes';

const PLAYBACK_COLOR = '#7B1FA2';

const buildIcon = (course: number): google.maps.Icon => ({
  url: createSvgIcon(course || 0, '', 'dataURL', { color: PLAYBACK_COLOR }),
  scaledSize: new window.google.maps.Size(30, 30),
  anchor: new window.google.maps.Point(15, 15)
});

interface GooglePlaybackMarkerProps {
  position: PlaybackPosition;
}

const GooglePlaybackMarker = ({ position }: GooglePlaybackMarkerProps) => {
  const map = useGoogleMap();
  const markerRef = useRef<google.maps.Marker | null>(null);
  const courseRef = useRef<number | null>(null);

  useEffect(() => () => {
    if (markerRef.current) {
      markerRef.current.setMap(null);
      markerRef.current = null;
    }
  }, [map]);

  useEffect(() => {
    if (!map || !window.google || !position) return;
    const course = position.course || 0;
    if (!markerRef.current) {
      markerRef.current = new window.google.maps.Marker({
        map,
        position: { lat: position.lat, lng: position.lng },
        icon: buildIcon(course),
        clickable: false,
        zIndex: 1000
      });
      courseRef.current = course;
    } else {
      markerRef.current.setPosition({ lat: position.lat, lng: position.lng });
      if (courseRef.current !== course) {
        markerRef.current.setIcon(buildIcon(course));
        courseRef.current = course;
      }
    }
  }, [map, position]);

  return null;
};

export default GooglePlaybackMarker;
