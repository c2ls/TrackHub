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

import type { Credential, CredentialFormInput } from 'api/manager/credential';

// The service functions are passed in as parameters (the api-layer functions in
// api/manager/credential and api/router). Type them precisely against those
// signatures so this stays a thin, runtime-identical dispatcher.
type CreateCredentialFn = (credential: CredentialFormInput) => Promise<Credential>;
type UpdateCredentialFn = (credentialId: string, credential: CredentialFormInput) => Promise<boolean>;
type TestConnectivityFn = (operatorId: string) => Promise<boolean>;

async function handleAdd(credential: CredentialFormInput, createCredential: CreateCredentialFn): Promise<void> {
    await createCredential(credential);
}

async function handleEdit(credential: CredentialFormInput, updateCredential: UpdateCredentialFn): Promise<void> {
    await updateCredential(credential.credentialId as string, credential);
}

export async function handleSaveCredential(
    credential: CredentialFormInput,
    createCredential: CreateCredentialFn,
    updateCredential: UpdateCredentialFn
): Promise<void> {
    if (!credential.credentialId) {
        await handleAdd(credential, createCredential);
    } else {
        await handleEdit(credential, updateCredential);
    }
}

export async function handleTestCredential(
    operatorId: string,
    testConnectivity: TestConnectivityFn
): Promise<boolean> {
    return await testConnectivity(operatorId);
}
