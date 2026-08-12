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
 * Credential GraphQL documents (Manager backend). Codegen validates these
 * against schemas/manager.graphql — values always travel as variables.
 */

import { graphql } from './generated';

export const CredentialFieldsFragment = graphql(`
  fragment CredentialFields on CredentialVm {
    credentialId
    key
    key2
    password
    uri
    username
  }
`);

export const GetCredentialByOperatorDocument = graphql(`
  query GetCredentialByOperator($operatorId: UUID!) {
    credentialByOperator(query: { operatorId: $operatorId }) {
      ...CredentialFields
    }
  }
`);

export const CreateCredentialDocument = graphql(`
  mutation CreateCredential($credential: CredentialDtoInput!) {
    createCredential(command: { credential: $credential }) {
      ...CredentialFields
    }
  }
`);

export const UpdateCredentialDocument = graphql(`
  mutation UpdateCredential($id: UUID!, $credential: UpdateCredentialDtoInput!) {
    updateCredential(id: $id, command: { credential: $credential })
  }
`);
