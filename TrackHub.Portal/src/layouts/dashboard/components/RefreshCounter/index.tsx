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

import { useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import type { AccountSettings } from 'api/manager/settings';

interface RefreshCounterProps {
  settings: AccountSettings;
  fetchPositions: () => void;
  calculateReference: () => void;
}

function RefreshCounter({ settings, fetchPositions, calculateReference }: RefreshCounterProps): ReactNode {
    const [counter, setCounter] = useState(settings.refreshMapInterval || 60);

    useEffect(() => {
      if (counter === 0) {
        fetchPositions();
        calculateReference();
        setCounter(settings.refreshMap ? settings.refreshMapInterval : 60);
      } else if (settings.refreshMap) {
        const timer = setInterval(() => setCounter(counter - 1), 1000);
        return () => clearInterval(timer);
      }
    }, [counter, settings.refreshMap]);

    return settings.refreshMap && <div className="mapcontrol">{counter} s.</div>;
  }

  export default RefreshCounter;
