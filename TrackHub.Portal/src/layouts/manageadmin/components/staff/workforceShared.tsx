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
 * Small presentation helpers shared by the workforce accordions (credentials,
 * qualifications, assignments, expirations).
 */

import type { ReactNode } from 'react';
import ArgonTypography from 'components/ArgonTypography';
import SearchSelect from 'edition/SearchSelect';
import type { SearchOption, SearchOptionsHook } from 'edition/SearchSelect';
import { useStaffOperationsOptions } from 'edition/pickerOptions';
import { daysUntilDateOnly } from 'utils/dateUtils';

export type BadgeColor =
  | 'primary'
  | 'secondary'
  | 'info'
  | 'success'
  | 'warning'
  | 'error'
  | 'light'
  | 'dark';

export function TextCell({ children }: { children?: ReactNode }) {
  return (
    <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
      {children || '-'}
    </ArgonTypography>
  );
}

/**
 * Whole calendar days from `today` (the account calendar's, see useAccountCalendar) until a
 * DateOnly expiry; negative once it has passed, 0 on the day itself.
 */
export function daysUntil(value: string | null | undefined, today: string): number | null {
  return daysUntilDateOnly(value, today);
}

/**
 * Severity color for an expiration date, mirroring the alert thresholds the
 * backend scan uses (30/15/7/0 days). No date at all is neutral.
 */
export function expiryColor(expiresAt: string | null | undefined, today: string): BadgeColor {
  const days = daysUntil(expiresAt, today);
  if (days === null) return 'secondary';
  if (days < 0) return 'dark';
  if (days <= 7) return 'error';
  if (days <= 15) return 'warning';
  if (days <= 30) return 'info';
  return 'success';
}

/** Assignment/qualification lifecycle status color. */
export function statusColor(status: string | null | undefined): BadgeColor {
  switch ((status || '').toUpperCase()) {
    case 'ACTIVE':
    case 'VALID':
      return 'success';
    case 'EXPIRED':
    case 'REVOKED':
    case 'CANCELLED':
      return 'error';
    case 'ENDED':
      return 'secondary';
    default:
      return 'info';
  }
}

interface StaffPickerProps {
  value: SearchOption | null;
  onChange: (staff: SearchOption | null) => void;
  label: string;
  placeholder: string;
  id: string;
  useOptions?: SearchOptionsHook;
}

/** Shared "which staff" picker over the account's staff, active or not, searched server-side. */
export function StaffPicker({ value, onChange, label, placeholder, id, useOptions = useStaffOperationsOptions }: StaffPickerProps) {
  return (
    <SearchSelect
      id={id}
      label={label}
      value={value?.value ?? null}
      valueLabel={value?.label}
      onChange={onChange}
      useOptions={useOptions}
      placeholder={placeholder}
    />
  );
}
