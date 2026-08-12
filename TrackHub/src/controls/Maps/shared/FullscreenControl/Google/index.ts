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

import { useEffect, useRef } from 'react';
import type { RefObject } from 'react';
import { useTranslation } from 'react-i18next';

type ControlCorner = 'TOP_LEFT' | 'TOP_RIGHT' | 'BOTTOM_LEFT' | 'BOTTOM_RIGHT';

// Vendor-prefixed fullscreen APIs not present in the standard DOM lib types.
interface FullscreenCapableElement extends HTMLElement {
    webkitRequestFullscreen?: () => void;
    msRequestFullscreen?: () => void;
}
interface FullscreenCapableDocument extends Document {
    webkitExitFullscreen?: () => void;
    msExitFullscreen?: () => void;
}

interface FullscreenControlProps {
    mapRef: RefObject<google.maps.Map | null>;
    position?: ControlCorner;
}

const FullscreenControl = ({ mapRef, position = 'TOP_RIGHT' }: FullscreenControlProps) => {
    const { t } = useTranslation();
    const buttonRef = useRef<HTMLButtonElement | null>(null);

    useEffect(() => {
        if (!mapRef || !mapRef.current || !window.google) return;

        const map = mapRef.current;

        // Create fullscreen button
        const fullscreenButton = document.createElement('button');
        fullscreenButton.textContent = '⛶';
        fullscreenButton.title = t('utilsmap.toogleFullScreen');
        fullscreenButton.style.cssText = `
            background: white;
            border: 2px solid rgba(0,0,0,.2);
            border-radius: 3px;
            box-shadow: rgba(0,0,0,.3) 0 1px 4px -1px;
            cursor: pointer;
            margin: 10px;
            padding: 0;
            width: 40px;
            height: 40px;
            font-size: 18px;
        `;

        buttonRef.current = fullscreenButton;

        const handleFullscreen = () => {
            const mapDiv = map.getDiv() as FullscreenCapableElement;

            if (!document.fullscreenElement) {
                if (mapDiv.requestFullscreen) {
                    mapDiv.requestFullscreen();
                } else if (mapDiv.webkitRequestFullscreen) {
                    mapDiv.webkitRequestFullscreen();
                } else if (mapDiv.msRequestFullscreen) {
                    mapDiv.msRequestFullscreen();
                }
                fullscreenButton.textContent = '⛶';
            } else {
                const doc = document as FullscreenCapableDocument;
                if (doc.exitFullscreen) {
                    doc.exitFullscreen();
                } else if (doc.webkitExitFullscreen) {
                    doc.webkitExitFullscreen();
                } else if (doc.msExitFullscreen) {
                    doc.msExitFullscreen();
                }
                fullscreenButton.textContent = '⛶';
            }
        };

        fullscreenButton.addEventListener('click', handleFullscreen);

        map.controls[window.google.maps.ControlPosition[position]].push(fullscreenButton);

        return () => {
            fullscreenButton.removeEventListener('click', handleFullscreen);
            if (buttonRef.current && buttonRef.current.parentNode) {
                buttonRef.current.parentNode.removeChild(buttonRef.current);
            }
        };
    }, [mapRef, position]);

    return null;
};

export default FullscreenControl;
