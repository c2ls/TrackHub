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
 * User GraphQL documents (Security backend). Codegen validates these against
 * schemas/security.graphql; values always travel as variables.
 */

import { graphql } from './generated';

export const UserDetailFragment = graphql(`
  fragment UserDetail on UserVm {
    userId
    username
    emailAddress
    firstName
    secondName
    lastName
    secondSurname
    dob
    loginAttempts
    accountId
    active
    integrationUser
  }
`);

export const GetUserDocument = graphql(`
  query GetUser($id: UUID!) {
    user(query: { id: $id }) {
      ...UserDetail
    }
  }
`);

export const GetCurrentUserDocument = graphql(`
  query GetCurrentUser {
    currentUser {
      userId
      username
      emailAddress
      firstName
      secondName
      lastName
      secondSurname
      dob
      loginAttempts
      accountId
      active
      roles {
        roleId
        name
      }
      profiles {
        policyId
        name
      }
    }
  }
`);

export const GetIntegrationUsersDocument = graphql(`
  query GetIntegrationUsers {
    users(query: { filter: { filters: [{ key: "IntegrationUser", value: true }] } }) {
      userId
      username
      emailAddress
    }
  }
`);

export const GetUsersByAccountDocument = graphql(`
  query GetUsersByAccount($skip: Int!, $take: Int!) {
    usersByAccount(query: { skip: $skip, take: $take }) {
      ...UserDetail
      lockedUntil
    }
  }
`);

export const CreateUserDocument = graphql(`
  mutation CreateUser($user: CreateUserDtoInput!) {
    createUser(command: { user: $user }) {
      ...UserDetail
    }
  }
`);

export const CreateManagerDocument = graphql(`
  mutation CreateManager($user: CreateUserDtoInput!, $accountId: UUID!) {
    createManager(command: { user: $user, accountId: $accountId }) {
      userId
    }
  }
`);

export const UpdateUserDocument = graphql(`
  mutation UpdateUser($id: UUID!, $user: UpdateUserDtoInput!) {
    updateUser(id: $id, command: { user: $user })
  }
`);

export const UpdateCurrentUserDocument = graphql(`
  mutation UpdateCurrentUser($user: UpdateCurrentUserDtoInput!) {
    updateCurrentUser(command: { user: $user })
  }
`);

export const UpdatePasswordDocument = graphql(`
  mutation UpdatePassword($id: UUID!, $user: UserPasswordDtoInput!) {
    updatePassword(id: $id, command: { user: $user })
  }
`);

export const DeleteUserDocument = graphql(`
  mutation DeleteUser($id: UUID!) {
    deleteUser(id: $id)
  }
`);

export const UnlockUserDocument = graphql(`
  mutation UnlockUser($id: UUID!) {
    unlockUser(id: $id)
  }
`);

export const UserIsAdminDocument = graphql(`
  query UserIsAdmin {
    userIsAdmin
  }
`);

export const UserIsManagerDocument = graphql(`
  query UserIsManager {
    userIsManager
  }
`);
