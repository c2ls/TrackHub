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
 * Transporter-type API (Manager backend): plain typed async functions. Failures
 * THROW ApiError — fallbacks and toasts belong to the caller layer (src/queries).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  TransporterTypeItemFragment as TransporterTypeItemType,
  TransporterTypeDtoInput,
} from './generated/graphql';
import {
  GetTransporterTypesDocument,
  UpdateTransporterTypeDocument,
} from './transporterTypeOperations';

export type TransporterTypeItem = TransporterTypeItemType;
export type { TransporterTypeDtoInput };

export async function getTransporterTypes(): Promise<TransporterTypeItem[]> {
  const data = await executeGraphQL('manager', GetTransporterTypesDocument);
  return data.transporterTypes;
}

export async function updateTransporterType(
  transporterTypeId: number,
  transporterType: Omit<TransporterTypeDtoInput, 'transporterTypeId'>
): Promise<boolean> {
  const input: TransporterTypeDtoInput = {
    transporterTypeId,
    accBased: !!transporterType.accBased,
    stoppedGap: Number(transporterType.stoppedGap),
    maxDistance: Number(transporterType.maxDistance),
    maxTimeGap: Number(transporterType.maxTimeGap),
  };
  const data = await executeGraphQL('manager', UpdateTransporterTypeDocument, {
    id: transporterTypeId,
    transporterType: input,
  });
  return data.updateTransporterType;
}
