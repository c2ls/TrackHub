/**
* Copyright (c) 2026 Sergio Hernandez. All rights reserved.
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
 * Document GraphQL documents (Manager backend). Codegen validates these
 * against schemas/manager.graphql and emits typed document nodes — values
 * always travel as variables, never string interpolation. The redacted
 * DocumentVm never exposes storage keys; downloadUrl is only populated after
 * a Clean scan. File bytes (upload/download) live over REST — see documents.ts.
 */

import { graphql } from './generated';

export const DocumentFieldsFragment = graphql(`
  fragment DocumentFields on DocumentVm {
    documentId
    accountId
    ownerEntityType
    ownerEntityId
    uploadedByPrincipalType
    uploadedByPrincipalId
    fileName
    category
    title
    description
    contentType
    sizeBytes
    sha256Hash
    classification
    status
    expiresAt
    visibilityScope
    scanStatus
    currentVersion
    downloadUrl
    lastModified
  }
`);

export const DocumentVersionFieldsFragment = graphql(`
  fragment DocumentVersionFields on DocumentVersionVm {
    documentVersionId
    documentId
    accountId
    versionNumber
    contentType
    fileName
    sizeBytes
    sha256Hash
    scanStatus
    replacedByPrincipalType
    replacedByPrincipalId
    reason
    createdAt
  }
`);

export const DocumentTypeFieldsFragment = graphql(`
  fragment DocumentTypeFields on DocumentTypeVm {
    documentTypeId
    accountId
    category
    displayName
    required
    expiring
    defaultValidityDays
    enabled
    createdAt
  }
`);

export const DocumentSignatureFieldsFragment = graphql(`
  fragment DocumentSignatureFields on DocumentSignatureVm {
    documentSignatureId
    documentId
    accountId
    signerPrincipalType
    signerPrincipalId
    signerName
    signatureImageDocumentId
    legalTextAccepted
    latitude
    longitude
    signedAt
    createdAt
  }
`);

// ---- Queries ----
export const GetDocumentsForOwnerDocument = graphql(`
  query GetDocumentsForOwner(
    $accountId: UUID!
    $ownerEntityType: String!
    $ownerEntityId: String!
    $skip: Int!
    $take: Int!
  ) {
    documentsForOwner(
      query: {
        accountId: $accountId
        ownerEntityType: $ownerEntityType
        ownerEntityId: $ownerEntityId
        skip: $skip
        take: $take
      }
    ) {
      ...DocumentFields
    }
  }
`);

export const GetDocumentDocument = graphql(`
  query GetDocument($documentId: UUID!) {
    document(query: { documentId: $documentId }) {
      ...DocumentFields
    }
  }
`);

export const GetDocumentVersionsDocument = graphql(`
  query GetDocumentVersions($documentId: UUID!, $skip: Int!, $take: Int!) {
    documentVersions(query: { documentId: $documentId, skip: $skip, take: $take }) {
      ...DocumentVersionFields
    }
  }
`);

export const GetDocumentSignaturesDocument = graphql(`
  query GetDocumentSignatures($documentId: UUID!) {
    documentSignatures(query: { documentId: $documentId }) {
      ...DocumentSignatureFields
    }
  }
`);

export const GetActiveDocumentByCategoryDocument = graphql(`
  query GetActiveDocumentByCategory(
    $ownerEntityType: String!
    $ownerEntityId: String!
    $category: String!
  ) {
    activeDocumentByCategory(
      query: { ownerEntityType: $ownerEntityType, ownerEntityId: $ownerEntityId, category: $category }
    ) {
      ...DocumentFields
    }
  }
`);

export const SearchDocumentsDocument = graphql(`
  query SearchDocuments($filter: DocumentSearchFilterInput!, $skip: Int!, $take: Int!) {
    searchDocuments(query: { filter: $filter, skip: $skip, take: $take }) {
      ...DocumentFields
    }
  }
`);

export const GetExpiringDocumentsDocument = graphql(`
  query GetExpiringDocuments($withinDays: Int!, $skip: Int!, $take: Int!) {
    expiringDocuments(query: { withinDays: $withinDays, skip: $skip, take: $take }) {
      ...DocumentFields
    }
  }
`);

export const GetDocumentSharesDocument = graphql(`
  query GetDocumentShares($documentId: UUID!) {
    documentShares(query: { documentId: $documentId }) {
      ...PublicLinkGrantFields
    }
  }
`);

export const GetDocumentTypesDocument = graphql(`
  query GetDocumentTypes($accountId: UUID!, $includeDisabled: Boolean!) {
    documentTypes(query: { accountId: $accountId, includeDisabled: $includeDisabled }) {
      ...DocumentTypeFields
    }
  }
`);

// ---- Mutations ----
export const VoidDocumentDocument = graphql(`
  mutation VoidDocument($documentId: UUID!, $reason: String!) {
    voidDocument(command: { documentId: $documentId, reason: $reason })
  }
`);

export const ExpireDocumentDocument = graphql(`
  mutation ExpireDocument($documentId: UUID!, $expiresAt: DateTime!) {
    expireDocument(command: { documentId: $documentId, expiresAt: $expiresAt })
  }
`);

export const DeleteDocumentReferenceDocument = graphql(`
  mutation DeleteDocumentReference($documentId: UUID!) {
    deleteDocumentReference(command: { documentId: $documentId })
  }
`);

export const SignDocumentDocument = graphql(`
  mutation SignDocument($signature: DocumentSignatureDtoInput!) {
    signDocument(command: { signature: $signature }) {
      ...DocumentSignatureFields
    }
  }
`);

export const ConfigureDocumentTypeDocument = graphql(`
  mutation ConfigureDocumentType($documentType: DocumentTypeDtoInput!) {
    configureDocumentType(command: { documentType: $documentType }) {
      ...DocumentTypeFields
    }
  }
`);

export const DisableDocumentTypeDocument = graphql(`
  mutation DisableDocumentType($documentTypeId: UUID!) {
    disableDocumentType(command: { documentTypeId: $documentTypeId })
  }
`);
