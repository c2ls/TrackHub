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
 * Account GraphQL documents (Manager backend). Codegen validates these against
 * schemas/manager.graphql and emits typed document nodes — values always travel
 * as variables, never string interpolation.
 *
 * The `accountContext` bootstrap read (status + branding + features) is folded
 * into this account module: it is a single-account, account-scoped read tied to
 * account identity/lifecycle, with no independent domain surface. Branding and
 * features keep their own modules because they own dedicated CRUD screens.
 */

import { graphql } from './generated';

/** Account record for the systemadmin/manageadmin account lists and CRUD flows. */
export const AccountItemFragment = graphql(`
  fragment AccountItem on AccountVm {
    accountId
    name
    description
    type
    typeId
    status
    statusId
    active
    lastModified
  }
`);

export const GetAccountDocument = graphql(`
  query GetAccount($id: UUID!) {
    account(query: { id: $id }) {
      ...AccountItem
    }
  }
`);

export const GetAccountByUserDocument = graphql(`
  query GetAccountByUser {
    accountByUser {
      ...AccountItem
    }
  }
`);

export const GetAccountsDocument = graphql(`
  query GetAccounts {
    accounts {
      ...AccountItem
    }
  }
`);

export const CreateAccountDocument = graphql(`
  mutation CreateAccount($account: AccountDtoInput!) {
    createAccount(command: { account: $account }) {
      ...AccountItem
    }
  }
`);

export const UpdateAccountDocument = graphql(`
  mutation UpdateAccount($id: UUID!, $account: UpdateAccountDtoInput!) {
    updateAccount(id: $id, command: { account: $account })
  }
`);

export const ChangeAccountStatusDocument = graphql(`
  mutation ChangeAccountStatus($accountId: UUID!, $targetStatus: AccountStatus!, $reason: String) {
    changeAccountStatus(
      command: { accountId: $accountId, targetStatus: $targetStatus, reason: $reason }
    ) {
      ...AccountItem
    }
  }
`);

/**
 * Single bootstrap read for the current principal's account: lifecycle status,
 * branding, and the effective feature set. Allowed on non-operational accounts
 * so the shell can render a suspension state without further operational queries.
 */
export const GetAccountContextDocument = graphql(`
  query GetAccountContext {
    accountContext {
      status
      statusId
      branding {
        accountId
        displayName
        logoDocumentId
        primaryColor
        reportHeader
        lastModified
      }
      features {
        accountFeatureId
        accountId
        featureKey
        enabled
        tier
        source
        effectiveFrom
        effectiveTo
        configurationJson
        lastModified
      }
    }
  }
`);
