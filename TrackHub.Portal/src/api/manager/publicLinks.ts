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
 * Public-link-grant API (Manager backend): plain typed async functions. Failures
 * THROW ApiError — consumed imperatively (load-on-expand + share dialog), caller
 * owns try/catch.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  PublicLinkGrantFieldsFragment as PublicLinkGrantFieldsType,
  PublicLinkGrantDtoInput,
} from './generated/graphql';
import {
  GetPublicLinkGrantDocument,
  GetPublicLinkGrantsByAccountDocument,
  CreatePublicLinkGrantDocument,
  RevokePublicLinkGrantDocument,
} from './publicLinksOperations';

export type PublicLinkGrant = PublicLinkGrantFieldsType;
export type { PublicLinkGrantDtoInput };

export async function getPublicLinkGrant(publicLinkGrantId: string): Promise<PublicLinkGrant> {
  const data = await executeGraphQL('manager', GetPublicLinkGrantDocument, { publicLinkGrantId });
  return data.publicLinkGrant;
}

export async function getPublicLinkGrantsByAccount(
  accountId: string,
  skip = 0,
  take = 50
): Promise<PublicLinkGrant[]> {
  const data = await executeGraphQL('manager', GetPublicLinkGrantsByAccountDocument, {
    accountId,
    skip,
    take,
  });
  return data.publicLinkGrantsByAccount;
}

export async function createPublicLinkGrant(
  publicLinkGrant: PublicLinkGrantDtoInput
): Promise<PublicLinkGrant> {
  const data = await executeGraphQL('manager', CreatePublicLinkGrantDocument, { publicLinkGrant });
  return data.createPublicLinkGrant;
}

export async function revokePublicLinkGrant(
  publicLinkGrantId: string,
  revokedBy: string
): Promise<boolean> {
  const data = await executeGraphQL('manager', RevokePublicLinkGrantDocument, {
    publicLinkGrantId,
    revokedBy,
  });
  return data.revokePublicLinkGrant;
}
