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
 * Role GraphQL documents (Security backend). Codegen validates these against
 * schemas/security.graphql; values always travel as variables.
 */

import { graphql } from './generated';

export const RoleItemFragment = graphql(`
  fragment RoleItem on RoleVm {
    roleId
    name
  }
`);

export const RoleResourceTreeFragment = graphql(`
  fragment RoleResourceTree on ResourceVm {
    resourceId
    resourceName
    actions {
      actionId
      actionName
      resourceId
    }
  }
`);

export const GetRolesDocument = graphql(`
  query GetRoles {
    roles {
      ...RoleItem
    }
  }
`);

export const GetResourcesByRoleDocument = graphql(`
  query GetResourcesByRole($roleId: Int!) {
    resourcesByRole(query: { roleId: $roleId }) {
      roleId
      name
      resources {
        ...RoleResourceTree
      }
    }
  }
`);

export const GetUsersByRoleDocument = graphql(`
  query GetUsersByRole($roleId: Int!) {
    usersByRole(query: { roleId: $roleId }) {
      userId
      username
      emailAddress
      firstName
      lastName
    }
  }
`);

export const CreateResourceActionRoleDocument = graphql(`
  mutation CreateResourceActionRole($resourceId: Int!, $actionId: Int!, $roleId: Int!) {
    createResourceActionRole(
      command: {
        resourceActionRole: { resourceId: $resourceId, actionId: $actionId, roleId: $roleId }
      }
    ) {
      roleId
    }
  }
`);

export const DeleteResourceActionRoleDocument = graphql(`
  mutation DeleteResourceActionRole($resourceId: Int!, $actionId: Int!, $roleId: Int!) {
    deleteResourceActionRole(resourceId: $resourceId, actionId: $actionId, roleId: $roleId)
  }
`);

export const CreateUserRoleDocument = graphql(`
  mutation CreateUserRole($userId: UUID!, $roleId: Int!) {
    createUserRole(command: { userRole: { userId: $userId, roleId: $roleId } }) {
      userId
      roleId
    }
  }
`);

export const DeleteUserRoleDocument = graphql(`
  mutation DeleteUserRole($userId: UUID!, $roleId: Int!) {
    deleteUserRole(userId: $userId, roleId: $roleId)
  }
`);
