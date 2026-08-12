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

/** Payload dispatched on the `app-error` CustomEvent. */
export interface AppErrorDetail {
  message: string;
  type: string;
  code: string | undefined;
  i18nKey: string | undefined;
}

/**
 * Handles the given error by logging it to the console (non-production only).
 */
export function handleSilentError(error: unknown): void {
  if (process.env.NODE_ENV !== 'production') {
    console.error(error);
  }
}
