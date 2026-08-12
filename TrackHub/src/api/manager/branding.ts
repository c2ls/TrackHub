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
 * Account branding API (Manager backend): plain typed async functions. Failures
 * THROW ApiError — fallbacks and toasts belong to the caller layer.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  AccountBrandingItemFragment as AccountBrandingItemType,
  AccountBrandingDtoInput,
} from './generated/graphql';
import {
  GetAccountBrandingDocument,
  UpdateAccountBrandingDocument,
} from './brandingOperations';

export type AccountBranding = AccountBrandingItemType;
export type { AccountBrandingDtoInput };

export async function getAccountBranding(accountId: string): Promise<AccountBranding> {
  const data = await executeGraphQL('manager', GetAccountBrandingDocument, { accountId });
  return data.accountBranding;
}

export async function updateAccountBranding(
  branding: AccountBrandingDtoInput
): Promise<AccountBranding> {
  const data = await executeGraphQL('manager', UpdateAccountBrandingDocument, {
    branding: {
      accountId: branding.accountId,
      displayName: branding.displayName,
      logoDocumentId: branding.logoDocumentId,
      primaryColor: branding.primaryColor,
      reportHeader: branding.reportHeader,
    },
  });
  return data.updateAccountBranding;
}
