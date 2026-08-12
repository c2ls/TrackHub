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
 * Role API (Security backend): plain typed async functions. Failures THROW
 * ApiError — fallbacks and toasts belong to the caller layer. The resource /
 * action assignment mutations are intentionally silent in the caller (the old
 * useRoleService swallowed them without a toast).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  RoleItemFragment as RoleItemType,
  GetResourcesByRoleQuery,
  GetUsersByRoleQuery,
} from './generated/graphql';
import {
  GetRolesDocument,
  GetResourcesByRoleDocument,
  GetUsersByRoleDocument,
  CreateResourceActionRoleDocument,
  DeleteResourceActionRoleDocument,
  CreateUserRoleDocument,
  DeleteUserRoleDocument,
} from './rolesOperations';

export type Role = RoleItemType;
export type RoleResources = GetResourcesByRoleQuery['resourcesByRole'];
export type RoleUser = GetUsersByRoleQuery['usersByRole'][number];

export async function getRoles(): Promise<Role[]> {
  const data = await executeGraphQL('security', GetRolesDocument);
  return data.roles;
}

export async function getResourcesByRole(roleId: number): Promise<RoleResources> {
  const data = await executeGraphQL('security', GetResourcesByRoleDocument, { roleId });
  return data.resourcesByRole;
}

export async function getUsersByRole(roleId: number): Promise<RoleUser[]> {
  const data = await executeGraphQL('security', GetUsersByRoleDocument, { roleId });
  return data.usersByRole;
}

export async function createResourceActionRole(
  resourceId: number,
  actionId: number,
  roleId: number
): Promise<{ roleId: number }> {
  const data = await executeGraphQL('security', CreateResourceActionRoleDocument, {
    resourceId,
    actionId,
    roleId,
  });
  return data.createResourceActionRole;
}

export async function deleteResourceActionRole(
  resourceId: number,
  actionId: number,
  roleId: number
): Promise<number> {
  const data = await executeGraphQL('security', DeleteResourceActionRoleDocument, {
    resourceId,
    actionId,
    roleId,
  });
  return data.deleteResourceActionRole;
}

export async function createUserRole(
  userId: string,
  roleId: number
): Promise<{ userId: string; roleId: number }> {
  const data = await executeGraphQL('security', CreateUserRoleDocument, { userId, roleId });
  return data.createUserRole;
}

/** Returns the id of the user whose role link was removed (schema: `deleteUserRole: UUID!`). */
export async function deleteUserRole(userId: string, roleId: number): Promise<string> {
  const data = await executeGraphQL('security', DeleteUserRoleDocument, { userId, roleId });
  return data.deleteUserRole;
}
