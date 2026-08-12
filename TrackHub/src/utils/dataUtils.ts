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

import { toISOStringWithTimezone } from "utils/dateUtils";

/** Heterogeneous value accepted by the formatter helpers below. */
type FormattableValue = string | number | boolean | Date | null | undefined;

/**
 * Formats the given value as a string.
 * Returns null if the input is falsy.
 */
export function formatJSONValue(value: FormattableValue): string | null {
  return value ? `${value}` : null;
}

/**
 * Formats the given date value to an ISO string with timezone offset.
 * Returns null if the input is falsy.
 */
export function formatDateTimeOffSet(value: string | number | Date | null | undefined): string | null {
  return value ? `${toISOStringWithTimezone(new Date(value))}` : null;
}
