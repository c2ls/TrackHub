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

import { Polyline } from 'react-leaflet';
import type { LatLngExpression } from 'leaflet';
import type { TrailPoint } from 'controls/Maps/core/mapTypes';

interface OSMTrailLayerProps {
  points?: TrailPoint[];
  color?: string;
}

const OSMTrailLayer = ({ points = [], color = '#1E90FF' }: OSMTrailLayerProps) => {
  if (points.length < 2) return null;
  const positions: LatLngExpression[] = points.map(point => [point.lat, point.lng]);
  return (
    <Polyline
      positions={positions}
      pathOptions={{ color, weight: 3, opacity: 0.7, dashArray: '6 6' }}
    />
  );
};

export default OSMTrailLayer;
