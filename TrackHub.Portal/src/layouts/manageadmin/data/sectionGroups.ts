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
 * Functional groups for the Account Management screen. The order is fixed and
 * language-independent (workflow order: account setup → fleet → access →
 * alerts → documents → operations); labels come from
 * `manageAdmin.groups.<key>` in the locale bundles. New sections must be
 * added to exactly one group.
 */
export const SECTION_GROUP_KEYS = [
  'account',
  'fleet',
  'access',
  'alerts',
  'documents',
  'operations',
] as const;

export type SectionGroupKey = (typeof SECTION_GROUP_KEYS)[number];
