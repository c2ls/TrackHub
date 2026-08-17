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
 * Credential API (Manager backend): plain typed async functions. Failures THROW
 * ApiError. The reads here back a "silent" flow (the credential dialog opens with
 * an empty form when the operator has no credential yet), so the caller swallows
 * read failures instead of toasting; create/update failures ARE surfaced.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type { CredentialFieldsFragment as CredentialFieldsType } from './generated/graphql';
import {
  GetCredentialByOperatorDocument,
  CreateCredentialDocument,
  UpdateCredentialDocument,
} from './credentialOperations';

export type Credential = CredentialFieldsType;

/** Loose form shape from the credential dialog (fields may be blank/undefined). */
export interface CredentialFormInput {
  credentialId?: string;
  operatorId: string;
  uri: string;
  username?: string | null;
  password?: string | null;
  key?: string | null;
  key2?: string | null;
}

export async function getCredentialByOperator(operatorId: string): Promise<Credential> {
  const data = await executeGraphQL('manager', GetCredentialByOperatorDocument, { operatorId });
  return data.credentialByOperator;
}

export async function createCredential(credential: CredentialFormInput): Promise<Credential> {
  const data = await executeGraphQL('manager', CreateCredentialDocument, {
    credential: {
      key: credential.key ?? '',
      key2: credential.key2 ?? '',
      operatorId: credential.operatorId,
      password: credential.password ?? '',
      uri: credential.uri,
      username: credential.username ?? '',
    },
  });
  return data.createCredential;
}

export async function updateCredential(
  credentialId: string,
  credential: CredentialFormInput
): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdateCredentialDocument, {
    id: credentialId,
    credential: {
      credentialId,
      key: credential.key ?? '',
      key2: credential.key2 ?? '',
      password: credential.password ?? '',
      uri: credential.uri,
      username: credential.username ?? '',
    },
  });
  return data.updateCredential;
}
