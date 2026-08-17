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
 * Client GraphQL documents (Security backend). Codegen validates these against
 * schemas/security.graphql; values always travel as variables.
 */

import { graphql } from './generated';

export const ClientItemFragment = graphql(`
  fragment ClientItem on ClientVm {
    clientId
    userId
    name
    description
    processed
    lastModified
  }
`);

export const GetClientsDocument = graphql(`
  query GetClients($skip: Int!, $take: Int!) {
    clients(query: { skip: $skip, take: $take }) {
      ...ClientItem
    }
  }
`);

export const CreateClientDocument = graphql(`
  mutation CreateClient($client: ClientDtoInput!) {
    createClient(command: { client: $client }) {
      ...ClientItem
      secret
    }
  }
`);

export const UpdateClientDocument = graphql(`
  mutation UpdateClient($id: UUID!, $client: ClientUserDtoInput!) {
    updateClient(id: $id, command: { client: $client })
  }
`);

export const DeleteClientDocument = graphql(`
  mutation DeleteClient($id: UUID!) {
    deleteClient(id: $id)
  }
`);
