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

import { useCallback, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { useLocation } from 'react-router-dom';

import HelpDialog from 'controls/Help';
import {
  HelpContext,
  resolveScreenKey,
} from 'context/help/HelpContext';
import type { HelpNavEntry } from 'context/help/HelpContext';

export { useHelp } from 'context/help/HelpContext';

interface HelpProviderProps {
  children: ReactNode;
  /** Route keys the current principal can access (role + feature filtered, from App). */
  allowedScreens: string[];
  isFeatureEnabled: (featureKey?: string | null) => boolean;
}

/**
 * Contextual help state: mounted once in App around the dashboard
 * shell. Owns the modal open state and the in-modal navigation stack, and
 * binds the F1 / Shift+? shortcuts. Data fetching lives in the dialog so
 * users who never open help never fetch anything.
 */
function HelpProvider({ children, allowedScreens, isFeatureEnabled }: HelpProviderProps) {
  const { pathname } = useLocation();
  const [open, setOpen] = useState(false);
  const [entries, setEntries] = useState<HelpNavEntry[]>([]);

  const currentScreenKey = useMemo(() => resolveScreenKey(pathname), [pathname]);

  const openHelp = useCallback((topicId?: string) => {
    setEntries([topicId ? { type: 'topic', id: topicId } : { type: 'auto' }]);
    setOpen(true);
  }, []);

  const closeHelp = useCallback(() => setOpen(false), []);

  const navigateToTopic = useCallback((id: string, anchor?: string) => {
    setEntries((stack) => [...stack, { type: 'topic', id, anchor }]);
  }, []);

  const openIndex = useCallback(() => {
    setEntries((stack) =>
      stack[stack.length - 1]?.type === 'index' ? stack : [...stack, { type: 'index' }]
    );
  }, []);

  const goBack = useCallback(() => {
    setEntries((stack) => (stack.length > 1 ? stack.slice(0, -1) : stack));
  }, []);

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'F1') {
        // F1 always opens, even from inside an input.
        event.preventDefault();
        openHelp();
        return;
      }
      if (event.key === '?') {
        const target = event.target as HTMLElement | null;
        const editing =
          !!target &&
          (target.tagName === 'INPUT' ||
            target.tagName === 'TEXTAREA' ||
            target.isContentEditable);
        if (!editing) {
          event.preventDefault();
          openHelp();
        }
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [openHelp]);

  const value = useMemo(
    () => ({
      open,
      entries,
      openHelp,
      closeHelp,
      navigateToTopic,
      openIndex,
      goBack,
      currentScreenKey,
      allowedScreens,
      isFeatureEnabled,
    }),
    [
      open,
      entries,
      openHelp,
      closeHelp,
      navigateToTopic,
      openIndex,
      goBack,
      currentScreenKey,
      allowedScreens,
      isFeatureEnabled,
    ]
  );

  return (
    <HelpContext.Provider value={value}>
      {children}
      <HelpDialog />
    </HelpContext.Provider>
  );
}

export default HelpProvider;
