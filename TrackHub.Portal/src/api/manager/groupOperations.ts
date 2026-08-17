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
 * Group GraphQL documents (Manager backend). Codegen validates these against
 * schemas/manager.graphql and emits typed document nodes — values always travel
 * as variables, never string interpolation.
 */

import { graphql } from './generated';

export const GroupItemFragment = graphql(`
  fragment GroupItem on GroupVm {
    groupId
    name
    description
    active
    accountId
  }
`);

export const GetGroupsDocument = graphql(`
  query GetGroups {
    groupsByAccount {
      ...GroupItem
    }
  }
`);

export const CreateGroupDocument = graphql(`
  mutation CreateGroup($group: GroupDtoInput!) {
    createGroup(command: { group: $group }) {
      ...GroupItem
    }
  }
`);

export const UpdateGroupDocument = graphql(`
  mutation UpdateGroup($id: Long!, $group: UpdateGroupDtoInput!) {
    updateGroup(id: $id, command: { group: $group })
  }
`);

export const DeleteGroupDocument = graphql(`
  mutation DeleteGroup($id: Long!) {
    deleteGroup(id: $id)
  }
`);

export const GetUsersByGroupDocument = graphql(`
  query GetUsersByGroup($groupId: Long!) {
    usersByGroup(query: { groupId: $groupId }) {
      userId
      username
      active
      accountId
    }
  }
`);

export const CreateUserGroupDocument = graphql(`
  mutation CreateUserGroup($userGroup: UserGroupDtoInput!) {
    createUserGroup(command: { userGroup: $userGroup }) {
      userId
      groupId
    }
  }
`);

export const DeleteUserGroupDocument = graphql(`
  mutation DeleteUserGroup($userId: UUID!, $groupId: Long!) {
    deleteUserGroup(userId: $userId, groupId: $groupId)
  }
`);

export const CreateTransporterGroupDocument = graphql(`
  mutation CreateTransporterGroup($transporterGroup: TransporterGroupDtoInput!) {
    createTransporterGroup(command: { transporterGroup: $transporterGroup }) {
      transporterId
      groupId
    }
  }
`);

export const DeleteTransporterGroupDocument = graphql(`
  mutation DeleteTransporterGroup($transporterId: UUID!, $groupId: Long!) {
    deleteTransporterGroup(transporterId: $transporterId, groupId: $groupId)
  }
`);
