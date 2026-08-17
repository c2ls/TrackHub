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
* Formats a date value into a string in the format MM/DD/YYYY.
* Returns an empty string if the input is invalid.
*/
export function formatDate(value: string | number | Date | null | undefined): string {
    if (value === null || value === undefined || value === '') {
        return '';
    }
    const date = new Date(value);
    if (!(date instanceof Date) || isNaN(date.getTime())) {
        return '';
    }
    return `${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getDate().toString().padStart(2, '0')}/${date.getFullYear()}`;
}

/**
* Formats a date value into a string in the format MM/DD/YYYY HH:MM:SS.
* Returns an empty string if the input is invalid.
*/
export function formatDateTime(value: string | number | Date | null | undefined): string {
    if (value === null || value === undefined || value === '') {
        return '';
    }
    const date = new Date(value);
    if (!(date instanceof Date) || isNaN(date.getTime())) {
        return '';
    }
    return `${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getDate().toString().padStart(2, '0')}/${date.getFullYear()} ${(date.getHours()).toString().padStart(2, '0')}:${(date.getMinutes()).toString().padStart(2, '0')}:${(date.getSeconds()).toString().padStart(2, '0')}`;
}

/**
 * DateOnly (GraphQL `LocalDate`) handling.
 *
 * A DateOnly carries NO instant: `2027-03-04` is that calendar day everywhere.
 * `new Date('2027-03-04')` however parses as UTC midnight, so reading local
 * getters off it renders 03/03/2027 for any viewer west of UTC. These helpers
 * work on the calendar parts directly and never construct an instant.
 *
 * DateTimeOffset values (which DO carry an instant and must be converted to the
 * viewer's zone) keep using formatDate/formatDateTime — do not route them here.
 */
const DATE_ONLY_PATTERN = /^(\d{4})-(\d{2})-(\d{2})/;

interface DateParts {
  year: number;
  month: number;
  day: number;
}

/** Parses the leading `YYYY-MM-DD` of a DateOnly value without building a Date. */
function parseDateOnly(value: string | null | undefined): DateParts | null {
  if (!value) return null;
  const match = DATE_ONLY_PATTERN.exec(value.trim());
  if (!match) return null;
  const [, year, month, day] = match;
  const parts = { year: Number(year), month: Number(month), day: Number(day) };
  if (parts.month < 1 || parts.month > 12 || parts.day < 1 || parts.day > 31) return null;
  return parts;
}

/**
 * Formats a DateOnly (`YYYY-MM-DD`, optionally with a trailing time part) as
 * MM/DD/YYYY. Timezone-stable: the rendered day always equals the stored day.
 * Returns an empty string when the value is absent or unparseable.
 */
export function formatDateOnly(value: string | null | undefined): string {
    const parts = parseDateOnly(value);
    if (!parts) return '';
    return `${parts.month.toString().padStart(2, '0')}/${parts.day.toString().padStart(2, '0')}/${parts.year}`;
}

/**
 * Whole calendar days from *today* (in the viewer's timezone) until a DateOnly
 * value; negative once the day has passed, 0 on the day itself. Both sides are
 * reduced to calendar days before subtracting, so the result never shifts by one
 * near midnight or for negative UTC offsets.
 */
export function daysUntilDateOnly(
    value: string | null | undefined,
    today: Date = new Date()
): number | null {
    const parts = parseDateOnly(value);
    if (!parts) return null;
    // Both endpoints are built as UTC midnights of their calendar day, so the
    // difference is an exact whole number of days with no DST or offset drift.
    const target = Date.UTC(parts.year, parts.month - 1, parts.day);
    const start = Date.UTC(today.getFullYear(), today.getMonth(), today.getDate());
    return Math.round((target - start) / 86_400_000);
}

/**
* Converts a date value to an ISO string with timezone.
*/
export function toISOStringWithTimezone(value: Date): string {
    // https://www.30secondsofcode.org/js/s/iso-format-date-with-timezone/
    // Pad a number to 2 digits
    const pad = (n: number): string => `${Math.floor(Math.abs(n))}`.padStart(2, '0');
    // Get timezone offset in ISO format (+hh:mm or -hh:mm)
    const getTimezoneOffset = (date: Date): string => {
        const tzOffset = -date.getTimezoneOffset();
        const diff = tzOffset >= 0 ? '+' : '-';
        return diff + pad(tzOffset / 60) + ':' + pad(tzOffset % 60);
    };

    return value.getFullYear() +
    '-' + pad(value.getMonth() + 1) +
    '-' + pad(value.getDate()) +
    'T' + pad(value.getHours()) +
    ':' + pad(value.getMinutes()) +
    ':' + pad(value.getSeconds()) +
    getTimezoneOffset(value);
}

/**
 * Converts an ISO-8601 UTC instant into the `YYYY-MM-DDTHH:mm` shape an `<input type="datetime-local">`
 * expects — which is **local wall time**, with no zone suffix.
 *
 * The browser interprets the control's value in the viewer's timezone, so the UTC instant is shifted
 * by the local offset. This is the exact inverse of {@link fromDateTimeLocalInput}, which parses the
 * field back as local time — the pair round-trips an instant unchanged in any timezone.
 *
 * Returns '' for null/undefined/unparseable input so a missing value renders as an empty control.
 */
export function toDateTimeLocalInput(value: string | number | Date | null | undefined): string {
    if (!value) return '';
    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) return '';
    const offsetMs = parsed.getTimezoneOffset() * 60_000;
    return new Date(parsed.getTime() - offsetMs).toISOString().slice(0, 16);
}

/**
 * Inverse of {@link toDateTimeLocalInput}: reads a `datetime-local` value as local wall time and
 * returns the corresponding ISO-8601 UTC instant, or null when the field is empty/unparseable.
 */
export function fromDateTimeLocalInput(value: string | null | undefined): string | null {
    if (!value) return null;
    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? null : parsed.toISOString();
}

/**
 * Formats a duration given in seconds into a string like "1h 23m".
 * Kept here for compatibility with older imports.
 */
export function formatDurationString(seconds: number | null | undefined): string {
    if (!seconds || seconds <= 0) return '0h 0m';
    const hrs = Math.floor(seconds / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    return `${hrs}h ${mins}m`;
}
