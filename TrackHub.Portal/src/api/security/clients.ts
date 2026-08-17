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
 * Client API (Security backend): plain typed async functions. Failures THROW
 * ApiError — fallbacks and toasts belong to the caller layer.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  ClientItemFragment as ClientItemType,
  ClientDtoInput,
  ClientUserDtoInput,
  CreateClientMutation,
} from './generated/graphql';
import {
  GetClientsDocument,
  CreateClientDocument,
  UpdateClientDocument,
  DeleteClientDocument,
} from './clientsOperations';

export type Client = ClientItemType;
export type CreatedClient = CreateClientMutation['createClient'];
export type { ClientDtoInput, ClientUserDtoInput };

export async function getClients(skip = 0, take = 500): Promise<Client[]> {
  const data = await executeGraphQL('security', GetClientsDocument, { skip, take });
  return data.clients;
}

export async function createClient(client: ClientDtoInput): Promise<CreatedClient> {
  const input: ClientDtoInput = {
    name: client.name,
    description: client.description,
    secret: client.secret,
    userId: client.userId ?? null,
  };
  const data = await executeGraphQL('security', CreateClientDocument, { client: input });
  return data.createClient;
}

export async function updateClient(
  clientId: string,
  client: Omit<ClientUserDtoInput, 'clientId'>
): Promise<boolean> {
  const data = await executeGraphQL('security', UpdateClientDocument, {
    id: clientId,
    client: { clientId, userId: client.userId },
  });
  return data.updateClient;
}

/** Returns the id of the deleted client (schema: `deleteClient: UUID!`). */
export async function deleteClient(clientId: string): Promise<string> {
  const data = await executeGraphQL('security', DeleteClientDocument, { id: clientId });
  return data.deleteClient;
}
