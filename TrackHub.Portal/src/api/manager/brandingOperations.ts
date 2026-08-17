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
 * Account branding GraphQL documents (Manager backend). Codegen validates these
 * against schemas/manager.graphql and emits typed document nodes — values always
 * travel as variables, never string interpolation.
 */

import { graphql } from './generated';

export const AccountBrandingItemFragment = graphql(`
  fragment AccountBrandingItem on AccountBrandingVm {
    accountId
    displayName
    logoDocumentId
    primaryColor
    reportHeader
    lastModified
  }
`);

// Read is allowed on non-operational accounts (renders the branded suspension screen).
export const GetAccountBrandingDocument = graphql(`
  query GetAccountBranding($accountId: UUID!) {
    accountBranding(query: { accountId: $accountId }) {
      ...AccountBrandingItem
    }
  }
`);

export const UpdateAccountBrandingDocument = graphql(`
  mutation UpdateAccountBranding($branding: AccountBrandingDtoInput!) {
    updateAccountBranding(command: { branding: $branding }) {
      ...AccountBrandingItem
    }
  }
`);
