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
import type { RefObject } from 'react';

interface ScaleControlProps {
    mapRef: RefObject<google.maps.Map | null>;
}

// The Google Maps scale control has a fixed position (bottom-right); unlike the
// OSM variant there is no position option, so none is accepted here.
const ScaleControl = ({ mapRef }: ScaleControlProps) => {
    useEffect(() => {
        if (!mapRef || !mapRef.current || !window.google) return;

        const map = mapRef.current;

        map.setOptions({ scaleControl: true });

        return () => {
            map.setOptions({ scaleControl: false });
        };
    }, [mapRef]);

    return null;
};

export default ScaleControl;
