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
 * Qualification/assignment constant sets. These mirror the backend's string
 * constant sets (spec 09 §6); they travel as GraphQL variables, so the exact
 * casing here is the contract.
 */

import type { TFunction } from 'i18next';

export const QUALIFICATION_TYPES = [
  'License',
  'MedicalExam',
  'Training',
  'BackgroundCheck',
  'HazmatPermit',
  'Other',
] as const;

export const QUALIFICATION_STATUSES = ['Valid', 'Expired', 'Revoked'] as const;

export const ASSIGNMENT_TYPES = ['Regular', 'Temporary'] as const;

export function qualificationTypeLabel(t: TFunction, value: string | null | undefined): string {
  const key = (value || '').toLowerCase();
  return t(`workforce.qualifications.types.${key}` as 'workforce.qualifications.types.license', {
    defaultValue: value || '-',
  });
}

export function qualificationStatusLabel(t: TFunction, value: string | null | undefined): string {
  const key = (value || '').toLowerCase();
  return t(`workforce.qualifications.statuses.${key}` as 'workforce.qualifications.statuses.valid', {
    defaultValue: value || '-',
  });
}

export function assignmentTypeLabel(t: TFunction, value: string | null | undefined): string {
  const key = (value || '').toLowerCase();
  return t(`workforce.assignments.types.${key}` as 'workforce.assignments.types.regular', {
    defaultValue: value || '-',
  });
}

export function assignmentStatusLabel(t: TFunction, value: string | null | undefined): string {
  const key = (value || '').toLowerCase();
  return t(`workforce.assignments.statuses.${key}` as 'workforce.assignments.statuses.active', {
    defaultValue: value || '-',
  });
}
