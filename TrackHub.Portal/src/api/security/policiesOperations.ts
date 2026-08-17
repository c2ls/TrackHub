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
 * Policy GraphQL documents (Security backend). Codegen validates these against
 * schemas/security.graphql; values always travel as variables.
 */

import { graphql } from './generated';

export const PolicyItemFragment = graphql(`
  fragment PolicyItem on PolicyVm {
    policyId
    name
  }
`);

export const PolicyResourceTreeFragment = graphql(`
  fragment PolicyResourceTree on ResourceVm {
    resourceId
    resourceName
    actions {
      resourceId
      actionName
      actionId
    }
  }
`);

export const GetPoliciesDocument = graphql(`
  query GetPolicies {
    policies {
      ...PolicyItem
    }
  }
`);

export const GetResourcesByPolicyDocument = graphql(`
  query GetResourcesByPolicy($policyId: Int!) {
    resourcesByPolicy(query: { policyId: $policyId }) {
      policyId
      name
      resources {
        ...PolicyResourceTree
      }
    }
  }
`);

export const GetUsersByPolicyDocument = graphql(`
  query GetUsersByPolicy($policyId: Int!) {
    usersByPolicy(query: { policyId: $policyId }) {
      userId
      username
      firstName
      lastName
    }
  }
`);

export const CreateResourceActionPolicyDocument = graphql(`
  mutation CreateResourceActionPolicy($resourceId: Int!, $actionId: Int!, $policyId: Int!) {
    createResourceActionPolicy(
      command: {
        resourceActionPolicy: { resourceId: $resourceId, actionId: $actionId, policyId: $policyId }
      }
    ) {
      policyId
    }
  }
`);

export const DeleteResourceActionPolicyDocument = graphql(`
  mutation DeleteResourceActionPolicy($resourceId: Int!, $actionId: Int!, $policyId: Int!) {
    deleteResourceActionPolicy(resourceId: $resourceId, actionId: $actionId, policyId: $policyId)
  }
`);

export const CreateUserPolicyDocument = graphql(`
  mutation CreateUserPolicy($userId: UUID!, $policyId: Int!) {
    createUserPolicy(command: { userPolicy: { userId: $userId, policyId: $policyId } }) {
      userId
      policyId
    }
  }
`);

export const DeleteUserPolicyDocument = graphql(`
  mutation DeleteUserPolicy($userId: UUID!, $policyId: Int!) {
    deleteUserPolicy(userId: $userId, policyId: $policyId)
  }
`);
