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

import { Polyline } from '@react-google-maps/api';
import type { TrailPoint } from 'controls/Maps/core/mapTypes';

interface GoogleTrailLayerProps {
  points?: TrailPoint[];
  color?: string;
}

const GoogleTrailLayer = ({ points = [], color = '#1E90FF' }: GoogleTrailLayerProps) => {
  if (points.length < 2) return null;
  return (
    <Polyline
      path={points.map(point => ({ lat: point.lat, lng: point.lng }))}
      options={{ strokeColor: color, strokeWeight: 3, strokeOpacity: 0.7 }}
    />
  );
};

export default GoogleTrailLayer;
