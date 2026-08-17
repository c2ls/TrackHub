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

import { createContext, useContext } from 'react';
import routes from 'routes';
import type { HelpManifest, HelpTopicMeta } from 'queries/help';

/**
 * One entry in the in-modal navigation stack. `auto` is what the Help button
 * and F1 push: it resolves to the current screen's primary topic once the
 * manifest is available (or to the index when the screen has no topic).
 */
export type HelpNavEntry =
  | { type: 'auto' }
  | { type: 'index' }
  | { type: 'topic'; id: string; anchor?: string };

export interface HelpContextValue {
  open: boolean;
  entries: HelpNavEntry[];
  /** Open help on a specific topic, or on the current screen's primary topic. */
  openHelp: (topicId?: string) => void;
  closeHelp: () => void;
  navigateToTopic: (id: string, anchor?: string) => void;
  openIndex: () => void;
  goBack: () => void;
  /** Route key of the screen the user is on, or null outside known screens. */
  currentScreenKey: string | null;
  /** Route keys the current principal can access (role + feature filtered). */
  allowedScreens: string[];
  isFeatureEnabled: (featureKey?: string | null) => boolean;
}

const noop = () => {};

/** Default no-op value so components render harmlessly outside the provider (tests, auth pages). */
export const HelpContext = createContext<HelpContextValue>({
  open: false,
  entries: [],
  openHelp: noop,
  closeHelp: noop,
  navigateToTopic: noop,
  openIndex: noop,
  goBack: noop,
  currentScreenKey: null,
  allowedScreens: [],
  isFeatureEnabled: () => true,
});

/** Public API for components: `const { openHelp } = useHelp();`. */
export function useHelp(): Pick<HelpContextValue, 'openHelp' | 'closeHelp'> {
  const { openHelp, closeHelp } = useContext(HelpContext);
  return { openHelp, closeHelp };
}

export function useHelpContext(): HelpContextValue {
  return useContext(HelpContext);
}

/** Longest-prefix match of the location against routes.tsx (sub-paths resolve to their owning route). */
export function resolveScreenKey(pathname: string): string | null {
  let bestKey: string | null = null;
  let bestLength = -1;
  for (const route of routes) {
    if (route.type !== 'route' || !route.route) continue;
    if (pathname === route.route || pathname.startsWith(`${route.route}/`)) {
      if (route.route.length > bestLength) {
        bestKey = route.key;
        bestLength = route.route.length;
      }
    }
  }
  return bestKey;
}

/**
 * Index/search visibility (UX filtering, not security — content is a public
 * static asset, CH-01): hide topics gated by a disabled account feature, and
 * topics whose only screens the principal cannot access. Index-only topics
 * (screens: []) are always visible.
 */
export function isTopicVisible(
  topic: HelpTopicMeta,
  allowedScreens: string[],
  isFeatureEnabled: (featureKey?: string | null) => boolean
): boolean {
  if (topic.featureKey && !isFeatureEnabled(topic.featureKey)) return false;
  if (topic.screens.length === 0) return true;
  return topic.screens.some((screen) => allowedScreens.includes(screen));
}

/** Topics for a screen ordered by category then order; the first is the primary topic. */
export function topicsForScreen(
  manifest: HelpManifest,
  screenKey: string | null
): HelpTopicMeta[] {
  if (!screenKey) return [];
  const categoryOrder = new Map(manifest.categories.map(({ id, order }) => [id, order]));
  return manifest.topics
    .filter((topic) => topic.screens.includes(screenKey))
    .sort(
      (a, b) =>
        (categoryOrder.get(a.category) ?? 99) - (categoryOrder.get(b.category) ?? 99) ||
        a.order - b.order ||
        a.id.localeCompare(b.id)
    );
}
