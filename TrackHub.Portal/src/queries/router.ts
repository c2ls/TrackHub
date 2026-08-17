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

/**
 * Router query keys. The router reads are imperative (live-map polling and a
 * search-button trip lookup), so consumers drive them with
 * `queryClient.fetchQuery` rather than mount hooks — these keys keep those
 * cache entries consistent. Silent ops (pingOperator, reverseGeocode) are
 * called directly and intentionally have no cache entry.
 */

import type { PositionSourceType } from 'api/router/router';

export const routerKeys = {
  all: ['router'] as const,
  devicePositions: () => [...routerKeys.all, 'devicePositions'] as const,
  trips: (transporterId: string, from: string, to: string, source: PositionSourceType) =>
    [...routerKeys.all, 'trips', transporterId, from, to, source] as const,
};
