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
 * Account support-grant GraphQL documents (Manager backend). Codegen validates
 * these against schemas/manager.graphql and emits typed document nodes — values
 * always travel as variables, never string interpolation.
 */

import { graphql } from './generated';

export const AccountSupportGrantItemFragment = graphql(`
  fragment AccountSupportGrantItem on AccountSupportGrantVm {
    accountSupportGrantId
    accountId
    supportUserId
    reason
    ticketReference
    approvedBy
    approvedAt
    accessLevel
    startsAt
    endsAt
    revokedAt
    revokedBy
    lastModified
  }
`);

export const GetSupportGrantStatusDocument = graphql(`
  query GetSupportGrantStatus($accountSupportGrantId: UUID!) {
    supportGrantStatus(query: { accountSupportGrantId: $accountSupportGrantId }) {
      ...AccountSupportGrantItem
    }
  }
`);

export const GetAccountSupportGrantsDocument = graphql(`
  query GetAccountSupportGrants($accountId: UUID, $skip: Int!, $take: Int!) {
    accountSupportGrants(query: { accountId: $accountId, skip: $skip, take: $take }) {
      ...AccountSupportGrantItem
    }
  }
`);

export const CreateAccountSupportGrantDocument = graphql(`
  mutation CreateAccountSupportGrant($accountSupportGrant: AccountSupportGrantDtoInput!) {
    createAccountSupportGrant(command: { accountSupportGrant: $accountSupportGrant }) {
      ...AccountSupportGrantItem
    }
  }
`);

export const ApproveAccountSupportGrantDocument = graphql(`
  mutation ApproveAccountSupportGrant($accountSupportGrantId: UUID!, $approvedBy: String!) {
    approveAccountSupportGrant(
      command: { accountSupportGrantId: $accountSupportGrantId, approvedBy: $approvedBy }
    )
  }
`);

export const RevokeAccountSupportGrantDocument = graphql(`
  mutation RevokeAccountSupportGrant($accountSupportGrantId: UUID!, $revokedBy: String!) {
    revokeAccountSupportGrant(
      command: { accountSupportGrantId: $accountSupportGrantId, revokedBy: $revokedBy }
    )
  }
`);
