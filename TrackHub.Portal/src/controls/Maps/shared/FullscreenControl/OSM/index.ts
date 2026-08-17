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

import { useEffect } from 'react';
import { useMap } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet-fullscreen/dist/Leaflet.fullscreen.js';
import 'leaflet-fullscreen/dist/leaflet.fullscreen.css';
import { useTranslation } from 'react-i18next';

type ControlCorner = 'topleft' | 'topright' | 'bottomleft' | 'bottomright';

interface FullscreenControlProps {
    position?: ControlCorner;
}

const FullscreenControl = ({ position = 'topleft' }: FullscreenControlProps) => {
    const { t } = useTranslation();
    const map = useMap();

    useEffect(() => {
        if (!map) return;

        const fullscreenControl = L.control.fullscreen({
            position: position,
            title: {
                'false': t('utilsmap.viewFullScreen'),
                'true': t('utilsmap.exitFullScreen')
            },
            forceSeparateButton: true
        });

        fullscreenControl.addTo(map);

        return () => {
            fullscreenControl.remove();
        };
    }, [map, position]);

    return null;
};

export default FullscreenControl;
