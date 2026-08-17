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
 * Edition extension point — distributions built on this codebase register
 * additional backend endpoints here; these objects stay empty in this
 * repository. Entries merge into the core endpoint maps in
 * api/core/endpoints.ts, and every key in `editionHealthEndpoints` becomes a
 * status-page tile (probed after the core services; its plain-language name and
 * description come from `edition/locales` under
 * `platformStatus.services.<key>.name` / `.description`).
 *
 * Like the core maps: eager values for GraphQL/REST endpoints, lazy getters
 * for health URLs.
 */

export const editionGraphQLEndpoints = {} as const;

export const editionRestEndpoints = {} as const;

export const editionHealthEndpoints = {} as const;
