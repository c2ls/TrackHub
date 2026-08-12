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

/**
 * Lightweight in-window refresh bus for the GPS-integration screens. An operator
 * toggle / sync / credential save dispatches the event; the dashboard, sync-run,
 * device and assignment panels listen for it and invalidate/refetch. The spec
 * retires this bus in a later pass (query-key invalidation); until then it stays.
 */
export const GPS_INTEGRATION_REFRESH_EVENT = 'gps-integration-refresh';

/** Listener signature the panels register with `window.addEventListener`. */
export type GpsIntegrationRefreshListener = (event: Event) => void;

export const notifyGpsIntegrationRefresh = (): void => {
  window.dispatchEvent(new Event(GPS_INTEGRATION_REFRESH_EVENT));
};
