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
 * Service-client-permission API (Security backend): plain typed async
 * functions. Failures THROW ApiError — fallbacks and toasts belong to the
 * caller layer.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  ServiceClientPermissionItemFragment as ServiceClientPermissionItemType,
  ServiceClientPermissionDtoInput,
} from './generated/graphql';
import {
  GetServiceClientPermissionsDocument,
  CreateServiceClientPermissionDocument,
  UpdateServiceClientPermissionDocument,
  DeleteServiceClientPermissionDocument,
} from './serviceClientPermissionsOperations';

export type ServiceClientPermission = ServiceClientPermissionItemType;
export type { ServiceClientPermissionDtoInput };

export async function getServiceClientPermissions(
  clientId: string | null = null,
  accountId: string | null = null,
  skip = 0,
  take = 50
): Promise<ServiceClientPermission[]> {
  const data = await executeGraphQL('security', GetServiceClientPermissionsDocument, {
    clientId,
    accountId,
    skip,
    take,
  });
  return data.serviceClientPermissions;
}

export async function createServiceClientPermission(
  permission: ServiceClientPermissionDtoInput
): Promise<ServiceClientPermission> {
  const data = await executeGraphQL('security', CreateServiceClientPermissionDocument, {
    permission,
  });
  return data.createServiceClientPermission;
}

export async function updateServiceClientPermission(
  serviceClientPermissionId: string,
  permission: ServiceClientPermissionDtoInput
): Promise<boolean> {
  const data = await executeGraphQL('security', UpdateServiceClientPermissionDocument, {
    serviceClientPermissionId,
    permission,
  });
  return data.updateServiceClientPermission;
}

/** Returns the id of the deleted permission (schema: `deleteServiceClientPermission: UUID!`). */
export async function deleteServiceClientPermission(
  serviceClientPermissionId: string
): Promise<string> {
  const data = await executeGraphQL('security', DeleteServiceClientPermissionDocument, {
    serviceClientPermissionId,
  });
  return data.deleteServiceClientPermission;
}
