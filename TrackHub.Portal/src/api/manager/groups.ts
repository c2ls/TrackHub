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
 * Group API (Manager backend): plain typed async functions. Failures THROW
 * ApiError — fallbacks and toasts belong to the caller layer (src/queries for
 * component reads; the allocator dialogs call the membership mutations directly
 * so they can preserve the legacy silent-delete semantics).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  GroupItemFragment as GroupItemType,
  GroupDtoInput,
  UpdateGroupDtoInput,
  GetUsersByGroupQuery,
} from './generated/graphql';
import {
  GetGroupsDocument,
  CreateGroupDocument,
  UpdateGroupDocument,
  DeleteGroupDocument,
  GetUsersByGroupDocument,
  CreateUserGroupDocument,
  DeleteUserGroupDocument,
  CreateTransporterGroupDocument,
  DeleteTransporterGroupDocument,
} from './groupOperations';

export type Group = GroupItemType;
export type GroupUser = GetUsersByGroupQuery['usersByGroup'][number];
export type { GroupDtoInput, UpdateGroupDtoInput };

export async function getGroups(): Promise<Group[]> {
  const data = await executeGraphQL('manager', GetGroupsDocument);
  return data.groupsByAccount;
}

export async function createGroup(group: GroupDtoInput): Promise<Group> {
  const input: GroupDtoInput = {
    name: group.name,
    description: group.description,
    active: !!group.active,
  };
  const data = await executeGraphQL('manager', CreateGroupDocument, { group: input });
  return data.createGroup;
}

export async function updateGroup(
  groupId: number,
  group: Omit<UpdateGroupDtoInput, 'groupId'>
): Promise<boolean> {
  const input: UpdateGroupDtoInput = {
    groupId,
    name: group.name,
    description: group.description,
    active: !!group.active,
  };
  const data = await executeGraphQL('manager', UpdateGroupDocument, { id: groupId, group: input });
  return data.updateGroup;
}

/** Returns the id of the deleted group (schema: `deleteGroup: Long!`). */
export async function deleteGroup(groupId: number): Promise<number> {
  const data = await executeGraphQL('manager', DeleteGroupDocument, { id: groupId });
  return data.deleteGroup;
}

export async function getUsersByGroup(groupId: number): Promise<GroupUser[]> {
  const data = await executeGraphQL('manager', GetUsersByGroupDocument, { groupId });
  return data.usersByGroup;
}

export async function createUserGroup(
  userId: string,
  groupId: number
): Promise<{ userId: string; groupId: number }> {
  const data = await executeGraphQL('manager', CreateUserGroupDocument, {
    userGroup: { userId, groupId },
  });
  return data.createUserGroup;
}

/** Returns the id of the deleted membership (schema: `deleteUserGroup: UUID!`). */
export async function deleteUserGroup(userId: string, groupId: number): Promise<string> {
  const data = await executeGraphQL('manager', DeleteUserGroupDocument, { userId, groupId });
  return data.deleteUserGroup;
}

export async function createTransporterGroup(
  transporterId: string,
  groupId: number
): Promise<{ transporterId: string; groupId: number }> {
  const data = await executeGraphQL('manager', CreateTransporterGroupDocument, {
    transporterGroup: { transporterId, groupId },
  });
  return data.createTransporterGroup;
}

/** Returns the id of the deleted membership (schema: `deleteTransporterGroup: UUID!`). */
export async function deleteTransporterGroup(
  transporterId: string,
  groupId: number
): Promise<string> {
  const data = await executeGraphQL('manager', DeleteTransporterGroupDocument, {
    transporterId,
    groupId,
  });
  return data.deleteTransporterGroup;
}
