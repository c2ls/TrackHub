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
import { fetchAllPages } from 'api/core/paging';
import type { ListParams, Page } from 'api/core/paging';
import type {
  GroupItemFragment as GroupItemType,
  GroupDtoInput,
  UpdateGroupDtoInput,
  GetUsersByGroupQuery,
  GetGroupLookupQuery,
} from './generated/graphql';
import {
  GetGroupsDocument,
  GetGroupLookupDocument,
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
export type GroupsPage = Page<Group>;
export type GroupLookup = GetGroupLookupQuery['groupLookup'][number];
export type GroupUser = GetUsersByGroupQuery['usersByGroup']['items'][number];
export type GroupUsersPage = Page<GroupUser>;
export type { GroupDtoInput, UpdateGroupDtoInput };

export async function getGroups(params: ListParams = {}): Promise<GroupsPage> {
  const data = await executeGraphQL('manager', GetGroupsDocument, {
    skip: params.skip ?? null,
    take: params.take ?? null,
    search: params.search ?? null,
  });
  return data.groupsByAccount;
}

/**
 * The account's groups as id + display name. Unpaged by design (the server
 * raises past its own ceiling rather than truncating) — for the dashboard group
 * filter, the POI form's group select and the POI table's groupId→name map.
 */
export async function getGroupLookup(): Promise<GroupLookup[]> {
  const data = await executeGraphQL('manager', GetGroupLookupDocument);
  return data.groupLookup;
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

export async function getUsersByGroup(
  groupId: number,
  params: ListParams = {}
): Promise<GroupUsersPage> {
  const data = await executeGraphQL('manager', GetUsersByGroupDocument, {
    groupId,
    skip: params.skip ?? null,
    take: params.take ?? null,
    search: params.search ?? null,
  });
  return data.usersByGroup;
}

/**
 * Every member of a group, all server pages drained. There is no per-group user
 * lookup, and the allocator dialog subtracts this list from the account's users:
 * a truncated membership makes assigned users reappear as available.
 */
export async function getAllUsersByGroup(groupId: number): Promise<GroupUser[]> {
  return fetchAllPages(async (skip, take) => (await getUsersByGroup(groupId, { skip, take })).items);
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
