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
 * Resource/action names as the backend spells them. These must match
 * `Common.Domain.Constants.Resources` / `.Actions` EXACTLY — `authorizedActions`
 * returns the seeded strings and `can()` compares them literally, so a typo
 * silently reads as "not permitted" rather than failing loudly.
 *
 * Only the pairs the portal actually gates on live here; the full catalog is the
 * backend's. See the wiki's User Permissions Overview.
 */

export const PermissionResources = {
  Trips: 'Trips',
} as const;

export const PermissionActions = {
  Read: 'Read',
  Write: 'Write',
  Edit: 'Edit',
  Delete: 'Delete',
  Export: 'Export',
  Execute: 'Execute',
  Custom: 'Custom',
} as const;
