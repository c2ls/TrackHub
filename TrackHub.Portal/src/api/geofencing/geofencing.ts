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
 * Geofencing API (Geofencing backend): plain typed async functions. Failures
 * THROW ApiError — fallbacks and toasts belong to the caller layer
 * (src/queries handles both for components).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  GeofenceFieldsFragment as GeofenceFieldsType,
  GeofenceDtoInput,
  GetGeofencesByAccountQuery,
  GetTransportersInGeofenceQuery,
} from './generated/graphql';
import {
  GetGeofenceDocument,
  GetGeofencesByAccountDocument,
  GetTransportersInGeofenceDocument,
  CreateGeofenceDocument,
  UpdateGeofenceDocument,
  DeleteGeofenceDocument,
} from './geofencingOperations';

export type Geofence = GeofenceFieldsType;
export type GeofencesPage = GetGeofencesByAccountQuery['geofencesByAccount'];
export type TransporterInGeofence =
  GetTransportersInGeofenceQuery['transportersInGeofence'][number];
export type { GeofenceDtoInput };

/** Server-side filters accepted by {@link getGeofencesByAccount}. */
export interface GeofenceListFilters {
  skip?: number | null;
  take?: number | null;
  type?: number | null;
  active?: boolean | null;
  search?: string | null;
}

export async function getGeofence(geofenceId: string): Promise<Geofence> {
  const data = await executeGraphQL('geofencing', GetGeofenceDocument, { id: geofenceId });
  return data.geofence;
}

export async function getGeofencesByAccount(
  enableCaching = false,
  filters: GeofenceListFilters = {}
): Promise<GeofencesPage> {
  const data = await executeGraphQL('geofencing', GetGeofencesByAccountDocument, {
    enableCaching,
    skip: filters.skip ?? null,
    take: filters.take ?? null,
    type: filters.type ?? null,
    active: filters.active ?? null,
    search: filters.search ?? null,
  });
  return data.geofencesByAccount;
}

/** Server-side max page size the backend clamps `take` to. */
const GEOFENCE_PAGE_SIZE = 500;
/** Defensive upper bound on the drained item count (guards a runaway loop). */
const GEOFENCE_DRAIN_CAP = 10_000;

/**
 * Returns the account's *entire* geofence set by draining every server page
 * (the backend clamps `take` to 500). Loops {@link getGeofencesByAccount} at the
 * max page size, accumulating items until the running total reaches
 * `totalCount` — stopping early on an empty page and never exceeding the
 * defensive {@link GEOFENCE_DRAIN_CAP}. Full-set consumers (map editor, dashboard
 * overlay, event-history filter) use this instead of a single capped page.
 */
export async function getAllGeofencesByAccount(
  enableCaching = false,
  filters: Omit<GeofenceListFilters, 'skip' | 'take'> = {}
): Promise<Geofence[]> {
  const items: Geofence[] = [];
  let skip = 0;
  for (;;) {
    const page = await getGeofencesByAccount(enableCaching, {
      ...filters,
      skip,
      take: GEOFENCE_PAGE_SIZE,
    });
    items.push(...page.items);
    // Stop on an empty page (defensive), once we've reached the reported total,
    // or if we hit the safety cap.
    if (page.items.length === 0) break;
    if (items.length >= page.totalCount) break;
    if (items.length >= GEOFENCE_DRAIN_CAP) break;
    skip += GEOFENCE_PAGE_SIZE;
  }
  return items;
}

export async function getTransportersInGeofence(
  geofenceId?: string | null,
  type?: number | null
): Promise<TransporterInGeofence[]> {
  const data = await executeGraphQL('geofencing', GetTransportersInGeofenceDocument, {
    geofenceId: geofenceId ?? null,
    type: type ?? null,
  });
  return data.transportersInGeofence;
}

export async function createGeofence(geofence: GeofenceDtoInput): Promise<Geofence> {
  const data = await executeGraphQL('geofencing', CreateGeofenceDocument, { geofence });
  return data.createGeofence;
}

export async function updateGeofence(
  geofenceId: string,
  geofence: GeofenceDtoInput
): Promise<boolean> {
  const data = await executeGraphQL('geofencing', UpdateGeofenceDocument, {
    id: geofenceId,
    geofence,
  });
  return data.updateGeofence;
}

/** Returns the id of the deleted geofence (schema: `deleteGeofence: UUID!`). */
export async function deleteGeofence(geofenceId: string): Promise<string> {
  const data = await executeGraphQL('geofencing', DeleteGeofenceDocument, { id: geofenceId });
  return data.deleteGeofence;
}
