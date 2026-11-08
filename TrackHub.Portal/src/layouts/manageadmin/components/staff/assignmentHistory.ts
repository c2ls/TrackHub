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

import type { StaffAssignmentHistoryFilters } from 'api/manager/staff';
import type { AccountCalendar, LocalDate } from 'utils/accountCalendar';

export const MAX_ASSIGNMENT_HISTORY_DAYS = 366;
export const DEFAULT_ASSIGNMENT_HISTORY_DAYS = 30;

const DAY_MS = 86_400_000;

export interface AssignmentHistoryFilterState {
  staffId: string | null;
  transporterId: string | null;
  from: LocalDate;
  to: LocalDate;
}

export function defaultAssignmentHistoryWindow(calendar: AccountCalendar): Pick<AssignmentHistoryFilterState, 'from' | 'to'> {
  return { from: calendar.daysAgo(DEFAULT_ASSIGNMENT_HISTORY_DAYS), to: calendar.today() };
}

export function assignmentHistoryWindowInvalid({ from, to }: Pick<AssignmentHistoryFilterState, 'from' | 'to'>): boolean {
  if (!from || !to) return true;
  const days = (Date.parse(to) - Date.parse(from)) / DAY_MS;
  return !(days >= 0 && days < MAX_ASSIGNMENT_HISTORY_DAYS);
}

export function toAssignmentHistoryFilters(
  state: AssignmentHistoryFilterState,
  calendar: AccountCalendar
): StaffAssignmentHistoryFilters {
  return {
    staffId: state.staffId,
    transporterId: state.transporterId,
    from: calendar.dayStartIso(state.from),
    to: calendar.dayEndIso(state.to),
  };
}
