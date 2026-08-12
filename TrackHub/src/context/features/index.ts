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
import type { AccountContext } from 'api/manager/accounts';

export type AccountFeature = AccountContext['features'][number];

export interface FeaturesContextValue {
  /** Feature rows from the account-context bootstrap read (empty until it resolves). */
  features: AccountFeature[];
  /** Whether the account feature is enabled and inside its effective window. */
  isFeatureEnabled: (featureKey?: string | null) => boolean;
}

/**
 * Mirrors the backend FeatureFlagService decision: a missing row means
 * disabled, and enablement is bounded by the effective-from/to window.
 * A nullish key means the caller is not feature-gated.
 */
export function isFeatureActive(
  features: AccountFeature[],
  featureKey?: string | null,
  at: Date = new Date()
): boolean {
  if (!featureKey) return true;
  const now = at.getTime();
  return features.some(
    (feature) =>
      feature.featureKey === featureKey &&
      feature.enabled &&
      (!feature.effectiveFrom || Date.parse(feature.effectiveFrom) <= now) &&
      (!feature.effectiveTo || Date.parse(feature.effectiveTo) >= now)
  );
}

/** Default renders everything so components stay harmless outside the provider (tests, auth pages). */
export const FeaturesContext = createContext<FeaturesContextValue>({
  features: [],
  isFeatureEnabled: () => true,
});

/** Public API for components: `const { isFeatureEnabled } = useFeatures();`. */
export function useFeatures(): FeaturesContextValue {
  return useContext(FeaturesContext);
}
