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
 * Service-client-permission GraphQL documents (Security backend). Codegen
 * validates these against schemas/security.graphql; values always travel as
 * variables.
 */

import { graphql } from './generated';

export const ServiceClientPermissionItemFragment = graphql(`
  fragment ServiceClientPermissionItem on ServiceClientPermissionVm {
    serviceClientPermissionId
    clientId
    accountId
    resource
    action
    scope
    audience
    active
    effectiveFrom
    effectiveTo
    lastModified
  }
`);

export const GetServiceClientPermissionsDocument = graphql(`
  query GetServiceClientPermissions(
    $clientId: String
    $accountId: UUID
    $skip: Int!
    $take: Int!
  ) {
    serviceClientPermissions(
      query: { clientId: $clientId, accountId: $accountId, skip: $skip, take: $take }
    ) {
      ...ServiceClientPermissionItem
    }
  }
`);

export const CreateServiceClientPermissionDocument = graphql(`
  mutation CreateServiceClientPermission($permission: ServiceClientPermissionDtoInput!) {
    createServiceClientPermission(command: { permission: $permission }) {
      ...ServiceClientPermissionItem
    }
  }
`);

export const UpdateServiceClientPermissionDocument = graphql(`
  mutation UpdateServiceClientPermission(
    $serviceClientPermissionId: UUID!
    $permission: ServiceClientPermissionDtoInput!
  ) {
    updateServiceClientPermission(
      command: {
        serviceClientPermissionId: $serviceClientPermissionId
        permission: $permission
      }
    )
  }
`);

export const DeleteServiceClientPermissionDocument = graphql(`
  mutation DeleteServiceClientPermission($serviceClientPermissionId: UUID!) {
    deleteServiceClientPermission(
      command: { serviceClientPermissionId: $serviceClientPermissionId }
    )
  }
`);
