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
 * Permission API (Security backend): plain typed async functions. Failures THROW
 * ApiError — fallbacks and toasts belong to the caller layer.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import { GetAuthorizedActionsDocument } from './permissionsOperations';

/** One resource/action pair the caller may reach. */
export interface AuthorizedAction {
  resourceName: string;
  actionName: string;
}

export async function getAuthorizedActions(userId: string): Promise<AuthorizedAction[]> {
  const data = await executeGraphQL('security', GetAuthorizedActionsDocument, { userId });
  return data.authorizedActions;
}
