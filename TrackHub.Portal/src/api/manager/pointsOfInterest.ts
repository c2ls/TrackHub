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
 * Point-of-interest API (Manager backend): plain typed async functions. Failures
 * THROW ApiError — fallbacks and toasts belong to the caller layer (src/queries).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  PointOfInterestItemFragment as PointOfInterestItemType,
  PointOfInterestDtoInput,
  UpdatePointOfInterestDtoInput,
} from './generated/graphql';
import {
  GetPointsOfInterestByAccountDocument,
  CreatePointOfInterestDocument,
  UpdatePointOfInterestDocument,
  DeletePointOfInterestDocument,
} from './pointsOfInterestOperations';

export type PointOfInterest = PointOfInterestItemType;
export type { PointOfInterestDtoInput, UpdatePointOfInterestDtoInput };

type PoiFields = Omit<PointOfInterestDtoInput, 'accountId'>;

/** Normalizes the mutable POI fields shared by create and update. */
function toPoiFields(poi: PoiFields): PoiFields {
  return {
    name: poi.name,
    description: poi.description ?? null,
    type: Number(poi.type),
    latitude: Number(poi.latitude),
    longitude: Number(poi.longitude),
    address: poi.address ?? null,
    color: poi.color ? Number(poi.color) : null,
    groupId: poi.groupId ? Number(poi.groupId) : null,
    active: !!poi.active,
  };
}

export async function getPointsOfInterestByAccount(): Promise<PointOfInterest[]> {
  const data = await executeGraphQL('manager', GetPointsOfInterestByAccountDocument);
  return data.pointsOfInterestByAccount;
}

export async function createPointOfInterest(
  poi: PointOfInterestDtoInput
): Promise<PointOfInterest> {
  // PointOfInterestDtoInput requires accountId (the legacy string-built create
  // never sent it — POI creation was broken). Callers must supply it now.
  const input: PointOfInterestDtoInput = { accountId: poi.accountId, ...toPoiFields(poi) };
  const data = await executeGraphQL('manager', CreatePointOfInterestDocument, {
    pointOfInterest: input,
  });
  return data.createPointOfInterest;
}

export async function updatePointOfInterest(
  pointOfInterestId: string,
  poi: UpdatePointOfInterestDtoInput
): Promise<boolean> {
  const input: UpdatePointOfInterestDtoInput = toPoiFields(poi);
  const data = await executeGraphQL('manager', UpdatePointOfInterestDocument, {
    id: pointOfInterestId,
    pointOfInterest: input,
  });
  return data.updatePointOfInterest;
}

/** Returns the id of the deleted POI (schema: `deletePointOfInterest: UUID!`). */
export async function deletePointOfInterest(pointOfInterestId: string): Promise<string> {
  const data = await executeGraphQL('manager', DeletePointOfInterestDocument, {
    id: pointOfInterestId,
  });
  return data.deletePointOfInterest;
}
