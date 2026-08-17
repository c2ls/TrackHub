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
 * User API (Security backend): plain typed async functions. Failures THROW
 * ApiError — fallbacks and toasts belong to the caller layer. `isAdmin` /
 * `isManager` are the silent ops: their callers default to `false` on failure
 * (the old useUserService swallowed them without a toast).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  UserDetailFragment as UserDetailType,
  GetCurrentUserQuery,
  GetUsersByAccountQuery,
  GetIntegrationUsersQuery,
  CreateUserDtoInput,
  UpdateUserDtoInput,
  UpdateCurrentUserDtoInput,
} from './generated/graphql';
import {
  GetUserDocument,
  GetCurrentUserDocument,
  GetIntegrationUsersDocument,
  GetUsersByAccountDocument,
  CreateUserDocument,
  CreateManagerDocument,
  UpdateUserDocument,
  UpdateCurrentUserDocument,
  UpdatePasswordDocument,
  DeleteUserDocument,
  UnlockUserDocument,
  UserIsAdminDocument,
  UserIsManagerDocument,
} from './usersOperations';

export type User = UserDetailType;
export type CurrentUser = GetCurrentUserQuery['currentUser'];
export type AccountUser = GetUsersByAccountQuery['usersByAccount'][number];
export type IntegrationUser = GetIntegrationUsersQuery['users'][number];
export type { CreateUserDtoInput, UpdateUserDtoInput, UpdateCurrentUserDtoInput };

export async function getUser(userId: string): Promise<User> {
  const data = await executeGraphQL('security', GetUserDocument, { id: userId });
  return data.user;
}

export async function getCurrentUser(): Promise<CurrentUser> {
  const data = await executeGraphQL('security', GetCurrentUserDocument);
  return data.currentUser;
}

/** Integration users only — used to populate the client-owner picker. */
export async function getUsers(): Promise<IntegrationUser[]> {
  const data = await executeGraphQL('security', GetIntegrationUsersDocument);
  return data.users;
}

export async function getUsersByAccount(skip = 0, take = 500): Promise<AccountUser[]> {
  const data = await executeGraphQL('security', GetUsersByAccountDocument, { skip, take });
  return data.usersByAccount;
}

export async function createUser(user: CreateUserDtoInput): Promise<User> {
  const input: CreateUserDtoInput = {
    username: user.username,
    password: user.password,
    emailAddress: user.emailAddress,
    firstName: user.firstName,
    secondName: user.secondName ?? null,
    lastName: user.lastName,
    secondSurname: user.secondSurname ?? null,
    dob: user.dob ?? null,
    active: user.active ?? true,
    integrationUser: user.integrationUser ?? false,
  };
  const data = await executeGraphQL('security', CreateUserDocument, { user: input });
  return data.createUser;
}

/**
 * Creates a manager for the given account. Note: unlike the old string-built
 * query, `integrationUser` is now always supplied (schema-required Boolean!);
 * the legacy document omitted it, so this path could never have succeeded.
 */
export async function createManager(
  user: CreateUserDtoInput,
  accountId: string
): Promise<{ userId: string }> {
  const input: CreateUserDtoInput = {
    username: user.username,
    password: user.password,
    emailAddress: user.emailAddress,
    firstName: user.firstName,
    secondName: user.secondName ?? null,
    lastName: user.lastName,
    secondSurname: user.secondSurname ?? null,
    dob: user.dob ?? null,
    active: user.active ?? true,
    integrationUser: user.integrationUser ?? false,
  };
  const data = await executeGraphQL('security', CreateManagerDocument, { user: input, accountId });
  return data.createManager;
}

export async function updateUser(
  userId: string,
  user: Omit<UpdateUserDtoInput, 'userId'>
): Promise<boolean> {
  const input: UpdateUserDtoInput = {
    userId,
    username: user.username,
    emailAddress: user.emailAddress,
    firstName: user.firstName,
    secondName: user.secondName ?? null,
    lastName: user.lastName,
    secondSurname: user.secondSurname ?? null,
    dob: user.dob ?? null,
    active: user.active,
    integrationUser: user.integrationUser,
  };
  const data = await executeGraphQL('security', UpdateUserDocument, { id: userId, user: input });
  return data.updateUser;
}

export async function updateCurrentUser(user: UpdateCurrentUserDtoInput): Promise<boolean> {
  const input: UpdateCurrentUserDtoInput = {
    firstName: user.firstName,
    secondName: user.secondName ?? null,
    lastName: user.lastName,
    secondSurname: user.secondSurname ?? null,
    dob: user.dob ?? null,
  };
  const data = await executeGraphQL('security', UpdateCurrentUserDocument, { user: input });
  return data.updateCurrentUser;
}

export async function updatePassword(userId: string, password: string): Promise<boolean> {
  const data = await executeGraphQL('security', UpdatePasswordDocument, {
    id: userId,
    user: { userId, password },
  });
  return data.updatePassword;
}

/** Returns the id of the deleted user (schema: `deleteUser: UUID!`). */
export async function deleteUser(userId: string): Promise<string> {
  const data = await executeGraphQL('security', DeleteUserDocument, { id: userId });
  return data.deleteUser;
}

export async function unlockUser(userId: string): Promise<boolean> {
  const data = await executeGraphQL('security', UnlockUserDocument, { id: userId });
  return data.unlockUser;
}

/** Silent op: the caller defaults to `false` on failure (no toast). */
export async function isAdmin(): Promise<boolean> {
  const data = await executeGraphQL('security', UserIsAdminDocument);
  return data.userIsAdmin;
}

/** Silent op: the caller defaults to `false` on failure (no toast). */
export async function isManager(): Promise<boolean> {
  const data = await executeGraphQL('security', UserIsManagerDocument);
  return data.userIsManager;
}
