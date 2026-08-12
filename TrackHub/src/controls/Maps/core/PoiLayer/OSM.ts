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
import { useMap } from 'react-leaflet';
import L from 'leaflet';
import { useTranslation } from 'react-i18next';
import { getPoiColorHex, createPoiPinSvg, buildPoiPopupHtml } from 'controls/Maps/core/poiUtils';
import type { MapPoi } from 'controls/Maps/core/mapTypes';

interface OSMPoiLayerProps {
  pois?: MapPoi[];
}

const OSMPoiLayer = ({ pois = [] }: OSMPoiLayerProps) => {
  const map = useMap();
  const { t } = useTranslation();

  useEffect(() => {
    if (!map) return undefined;
    const layerGroup = L.layerGroup();

    pois.filter(poi => poi.active !== false).forEach(poi => {
      const icon = L.divIcon({
        className: 'poi-icon',
        html: createPoiPinSvg(getPoiColorHex(poi.color)),
        iconSize: [28, 36],
        iconAnchor: [14, 34],
        popupAnchor: [0, -32]
      });
      const marker = L.marker([poi.latitude, poi.longitude], { icon });
      marker.bindTooltip(poi.name, { direction: 'top', offset: [0, -30] });
      marker.bindPopup(buildPoiPopupHtml(poi, t), { maxWidth: 240 });
      layerGroup.addLayer(marker);
    });

    layerGroup.addTo(map);
    return () => {
      map.removeLayer(layerGroup);
    };
  }, [map, pois, t]);

  return null;
};

export default OSMPoiLayer;
