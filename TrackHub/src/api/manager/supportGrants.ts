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
 * Account support-grant API (Manager backend): plain typed async functions.
 * Failures THROW ApiError — fallbacks and toasts belong to the caller layer.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  AccountSupportGrantItemFragment as AccountSupportGrantItemType,
  AccountSupportGrantDtoInput,
} from './generated/graphql';
import {
  GetSupportGrantStatusDocument,
  GetAccountSupportGrantsDocument,
  CreateAccountSupportGrantDocument,
  ApproveAccountSupportGrantDocument,
  RevokeAccountSupportGrantDocument,
} from './supportGrantsOperations';

export type AccountSupportGrant = AccountSupportGrantItemType;
export type { AccountSupportGrantDtoInput };

export async function getSupportGrantStatus(
  accountSupportGrantId: string
): Promise<AccountSupportGrant> {
  const data = await executeGraphQL('manager', GetSupportGrantStatusDocument, {
    accountSupportGrantId,
  });
  return data.supportGrantStatus;
}

export async function getAccountSupportGrants(
  accountId: string | null,
  skip = 0,
  take = 50
): Promise<AccountSupportGrant[]> {
  const data = await executeGraphQL('manager', GetAccountSupportGrantsDocument, {
    accountId,
    skip,
    take,
  });
  return data.accountSupportGrants;
}

export async function createAccountSupportGrant(
  grant: AccountSupportGrantDtoInput
): Promise<AccountSupportGrant> {
  const data = await executeGraphQL('manager', CreateAccountSupportGrantDocument, {
    accountSupportGrant: {
      accountId: grant.accountId,
      supportUserId: grant.supportUserId,
      reason: grant.reason,
      ticketReference: grant.ticketReference,
      accessLevel: grant.accessLevel,
      startsAt: grant.startsAt,
      endsAt: grant.endsAt,
    },
  });
  return data.createAccountSupportGrant;
}

export async function approveAccountSupportGrant(
  accountSupportGrantId: string,
  approvedBy: string
): Promise<boolean> {
  const data = await executeGraphQL('manager', ApproveAccountSupportGrantDocument, {
    accountSupportGrantId,
    approvedBy,
  });
  return data.approveAccountSupportGrant;
}

export async function revokeAccountSupportGrant(
  accountSupportGrantId: string,
  revokedBy: string
): Promise<boolean> {
  const data = await executeGraphQL('manager', RevokeAccountSupportGrantDocument, {
    accountSupportGrantId,
    revokedBy,
  });
  return data.revokeAccountSupportGrant;
}
