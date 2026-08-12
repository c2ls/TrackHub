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

import { useEffect } from 'react';
import { useGoogleMap } from '@react-google-maps/api';
import { useTranslation } from 'react-i18next';
import { getPoiColorHex, createPoiPinSvg, buildPoiPopupHtml } from 'controls/Maps/core/poiUtils';
import type { MapPoi } from 'controls/Maps/core/mapTypes';

interface GooglePoiLayerProps {
  pois?: MapPoi[];
}

interface PoiOverlay {
  marker: google.maps.Marker;
  infoWindow: google.maps.InfoWindow;
  listener: google.maps.MapsEventListener;
}

const GooglePoiLayer = ({ pois = [] }: GooglePoiLayerProps) => {
  const map = useGoogleMap();
  const { t } = useTranslation();

  useEffect(() => {
    if (!map || !window.google) return undefined;

    const overlays: PoiOverlay[] = pois.filter(poi => poi.active !== false).map(poi => {
      const marker = new window.google.maps.Marker({
        map,
        position: { lat: poi.latitude, lng: poi.longitude },
        title: poi.name,
        icon: {
          url: createPoiPinSvg(getPoiColorHex(poi.color), 'dataURL'),
          scaledSize: new window.google.maps.Size(28, 36),
          anchor: new window.google.maps.Point(14, 34)
        }
      });
      const infoWindow = new window.google.maps.InfoWindow({
        content: buildPoiPopupHtml(poi, t)
      });
      const listener = marker.addListener('click', () => infoWindow.open({ map, anchor: marker }));
      return { marker, infoWindow, listener };
    });

    return () => {
      overlays.forEach(({ marker, infoWindow, listener }) => {
        listener.remove();
        infoWindow.close();
        marker.setMap(null);
      });
    };
  }, [map, pois, t]);

  return null;
};

export default GooglePoiLayer;
