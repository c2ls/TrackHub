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
 * Public-link-grant GraphQL documents (Manager backend). Codegen validates
 * these against schemas/manager.graphql — values always travel as variables.
 */

import { graphql } from './generated';

export const PublicLinkGrantFieldsFragment = graphql(`
  fragment PublicLinkGrantFields on PublicLinkGrantVm {
    publicLinkGrantId
    accountId
    resourceType
    resourceId
    scopes
    purpose
    expiresAt
    revokedAt
    revokedBy
    createdByPrincipalId
    accessCount
    lastAccessedAt
    lastModified
    token
  }
`);

export const GetPublicLinkGrantDocument = graphql(`
  query GetPublicLinkGrant($publicLinkGrantId: UUID!) {
    publicLinkGrant(query: { publicLinkGrantId: $publicLinkGrantId }) {
      ...PublicLinkGrantFields
    }
  }
`);

export const GetPublicLinkGrantsByAccountDocument = graphql(`
  query GetPublicLinkGrantsByAccount($accountId: UUID!, $skip: Int!, $take: Int!) {
    publicLinkGrantsByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) {
      ...PublicLinkGrantFields
    }
  }
`);

export const CreatePublicLinkGrantDocument = graphql(`
  mutation CreatePublicLinkGrant($publicLinkGrant: PublicLinkGrantDtoInput!) {
    createPublicLinkGrant(command: { publicLinkGrant: $publicLinkGrant }) {
      ...PublicLinkGrantFields
    }
  }
`);

export const RevokePublicLinkGrantDocument = graphql(`
  mutation RevokePublicLinkGrant($publicLinkGrantId: UUID!, $revokedBy: String!) {
    revokePublicLinkGrant(command: { publicLinkGrantId: $publicLinkGrantId, revokedBy: $revokedBy })
  }
`);
