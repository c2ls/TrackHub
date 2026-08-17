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

import { useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import type { Driver } from 'api/manager/drivers';
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
 * Whole calendar days from today until a DateOnly expiry (negative once it has
 * passed, 0 on the day itself). Delegates to the DateOnly-aware helper: mixing a
 * UTC-midnight parse with a local `Date.now()` put the count — and therefore the
 * severity colour — off by one for every viewer at a negative UTC offset.
 */
export function daysUntil(value: string | null | undefined): number | null {
  return daysUntilDateOnly(value);
}

/**
 * Severity color for an expiration date, mirroring the alert thresholds the
 * backend scan uses (30/15/7/0 days). No date at all is neutral.
 */
export function expiryColor(expiresAt: string | null | undefined): BadgeColor {
  const days = daysUntil(expiresAt);
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

interface DriverPickerProps {
  drivers: Driver[];
  value: string;
  onChange: (driverId: string) => void;
  label: string;
  placeholder: string;
  id: string;
}

/**
 * Shared "which driver am I administering" picker used by the per-driver
 * sections. `drivers` is the account's COMPLETE list (the api layer pages to
 * exhaustion), so a typeahead sits above the select to keep a few hundred
 * options navigable. The selected driver is always kept in the option list, so
 * narrowing the filter can never blank out a live selection.
 */
export function DriverPicker({ drivers, value, onChange, label, placeholder, id }: DriverPickerProps) {
  const { t } = useTranslation();
  const [search, setSearch] = useState('');

  const options = useMemo(() => {
    const term = search.trim().toLowerCase();
    const matches = term
      ? drivers.filter((driver) => (driver.name || '').toLowerCase().includes(term))
      : drivers;
    const selected = value ? drivers.find((driver) => driver.driverId === value) : undefined;
    return selected && !matches.some((driver) => driver.driverId === value)
      ? [selected, ...matches]
      : matches;
  }, [drivers, search, value]);

  return (
    <ArgonBox>
      <CustomTextField
        margin="none"
        name={`${id}Search`}
        id={`${id}Search`}
        label={t('workforce.searchDriver')}
        type="text"
        fullWidth
        value={search}
        onChange={(event) => setSearch(event.target.value)}
      />
      <CustomSelect
        list={options.map((driver) => ({ value: driver.driverId, label: driver.name }))}
        name={id}
        id={id}
        label={label}
        value={value}
        handleChange={(event) => onChange(String(event.target.value ?? ''))}
        numericValue={false}
        placeholder={placeholder}
      />
      {drivers.length > 0 && options.length === 0 && (
        <ArgonTypography variant="caption" color="secondary">
          {t('workforce.noDriverMatches')}
        </ArgonTypography>
      )}
    </ArgonBox>
  );
}
