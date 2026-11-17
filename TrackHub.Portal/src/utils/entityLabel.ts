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

import type { TFunction } from 'i18next';

export function entityTypeLabel(t: TFunction, type: string): string {
  return t(`entityTypes.${type}` as 'entityTypes.Trip', { defaultValue: type });
}

/** "Unit: AAA000" — the record's name when resolved, its id otherwise. */
export function entityLabel(t: TFunction, names: Map<string, string>, type: string, id: string): string {
  return `${entityTypeLabel(t, type)}: ${names.get(`${type}:${id}`) ?? id}`;
}
