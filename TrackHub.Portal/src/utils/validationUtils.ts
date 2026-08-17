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

export function validateEmail(value: string | null | undefined): string | boolean | null | undefined {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return value && emailRegex.test(value);
};

/**
 * Platform password policy (matches the Security backend validators and
 * ColumnMetadata.MinimumPasswordLength): at least 8 characters with at least
 * one uppercase letter, one lowercase letter, and one digit.
 */
export function validatePassword(value: string | null | undefined): string | boolean | null | undefined {
    return (
      value &&
      value.length >= 8 &&
      /[A-Z]/.test(value) &&
      /[a-z]/.test(value) &&
      /[0-9]/.test(value)
    );
};
