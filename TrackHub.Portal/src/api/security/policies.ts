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
 * Policy API (Security backend): plain typed async functions. Failures THROW
 * ApiError — fallbacks and toasts belong to the caller layer. The resource /
 * action assignment mutations are intentionally silent in the caller (the old
 * usePolicyService swallowed them without a toast).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  PolicyItemFragment as PolicyItemType,
  GetResourcesByPolicyQuery,
  GetUsersByPolicyQuery,
} from './generated/graphql';
import {
  GetPoliciesDocument,
  GetResourcesByPolicyDocument,
  GetUsersByPolicyDocument,
  CreateResourceActionPolicyDocument,
  DeleteResourceActionPolicyDocument,
  CreateUserPolicyDocument,
  DeleteUserPolicyDocument,
} from './policiesOperations';

export type Policy = PolicyItemType;
export type PolicyResources = GetResourcesByPolicyQuery['resourcesByPolicy'];
export type PolicyUser = GetUsersByPolicyQuery['usersByPolicy'][number];

export async function getPolicies(): Promise<Policy[]> {
  const data = await executeGraphQL('security', GetPoliciesDocument);
  return data.policies;
}

export async function getResourcesByPolicy(policyId: number): Promise<PolicyResources> {
  const data = await executeGraphQL('security', GetResourcesByPolicyDocument, { policyId });
  return data.resourcesByPolicy;
}

export async function getUsersByPolicy(policyId: number): Promise<PolicyUser[]> {
  const data = await executeGraphQL('security', GetUsersByPolicyDocument, { policyId });
  return data.usersByPolicy;
}

export async function createResourceActionPolicy(
  resourceId: number,
  actionId: number,
  policyId: number
): Promise<{ policyId: number }> {
  const data = await executeGraphQL('security', CreateResourceActionPolicyDocument, {
    resourceId,
    actionId,
    policyId,
  });
  return data.createResourceActionPolicy;
}

export async function deleteResourceActionPolicy(
  resourceId: number,
  actionId: number,
  policyId: number
): Promise<number> {
  const data = await executeGraphQL('security', DeleteResourceActionPolicyDocument, {
    resourceId,
    actionId,
    policyId,
  });
  return data.deleteResourceActionPolicy;
}

export async function createUserPolicy(
  userId: string,
  policyId: number
): Promise<{ userId: string; policyId: number }> {
  const data = await executeGraphQL('security', CreateUserPolicyDocument, { userId, policyId });
  return data.createUserPolicy;
}

/** Returns the id of the user whose policy link was removed (schema: `deleteUserPolicy: UUID!`). */
export async function deleteUserPolicy(userId: string, policyId: number): Promise<string> {
  const data = await executeGraphQL('security', DeleteUserPolicyDocument, { userId, policyId });
  return data.deleteUserPolicy;
}
