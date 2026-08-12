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
 * Account API (Manager backend): plain typed async functions. Failures THROW
 * ApiError — fallbacks and toasts belong to the caller layer (src/queries for
 * components). The former `accountContext` service is folded in here as
 * getAccountContext (the single-account bootstrap read).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  AccountItemFragment as AccountItemType,
  AccountDtoInput,
  UpdateAccountDtoInput,
  AccountStatus,
  GetAccountContextQuery,
} from './generated/graphql';
import {
  GetAccountDocument,
  GetAccountByUserDocument,
  GetAccountsDocument,
  CreateAccountDocument,
  UpdateAccountDocument,
  ChangeAccountStatusDocument,
  GetAccountContextDocument,
} from './accountOperations';

export type Account = AccountItemType;
export type AccountContext = GetAccountContextQuery['accountContext'];
export type { AccountDtoInput, UpdateAccountDtoInput, AccountStatus };

export async function getAccount(accountId: string): Promise<Account> {
  const data = await executeGraphQL('manager', GetAccountDocument, { id: accountId });
  return data.account;
}

export async function getAccountByUser(): Promise<Account> {
  const data = await executeGraphQL('manager', GetAccountByUserDocument);
  return data.accountByUser;
}

export async function getAccounts(): Promise<Account[]> {
  const data = await executeGraphQL('manager', GetAccountsDocument);
  return data.accounts;
}

export async function createAccount(account: AccountDtoInput): Promise<Account> {
  const data = await executeGraphQL('manager', CreateAccountDocument, {
    account: {
      name: account.name,
      description: account.description,
      typeId: account.typeId,
      active: account.active,
      password: account.password,
      emailAddress: account.emailAddress,
      firstName: account.firstName,
      lastName: account.lastName,
    },
  });
  return data.createAccount;
}

export async function updateAccount(
  accountId: string,
  account: Omit<UpdateAccountDtoInput, 'accountId'>
): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdateAccountDocument, {
    id: accountId,
    account: {
      accountId,
      name: account.name,
      description: account.description,
      typeId: account.typeId,
      active: account.active,
    },
  });
  return data.updateAccount;
}

export async function changeAccountStatus(
  accountId: string,
  targetStatus: AccountStatus,
  reason: string | null = null
): Promise<Account> {
  const data = await executeGraphQL('manager', ChangeAccountStatusDocument, {
    accountId,
    targetStatus,
    reason,
  });
  return data.changeAccountStatus;
}

/**
 * Bootstrap read (status + branding + features) for the current principal's
 * account. Allowed on non-operational accounts so the shell can render a
 * suspension state without issuing operational queries.
 */
export async function getAccountContext(): Promise<AccountContext> {
  const data = await executeGraphQL('manager', GetAccountContextDocument);
  return data.accountContext;
}
