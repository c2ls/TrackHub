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
 * Converts a string to camel case. '-', '_' and '.' act as word separators
 * (dotted feature keys like 'notifications.email' map to i18n label keys).
 */
export function toCamelCase(str: string): string {
    return str.replace(/([-_.][a-z])/gi, ($1) => {
        return $1.toUpperCase().replace('-', '').replace('_', '').replace('.', '');
    }).replace(/(^[A-Z])/g, $1 => $1.toLowerCase());
}

/**
 * Removes all non-alphanumeric characters from a string.
 */
export function cleanString(str: string): string {
    return str.replace(/[^a-zA-Z0-9]/g, '').toLowerCase();
}
