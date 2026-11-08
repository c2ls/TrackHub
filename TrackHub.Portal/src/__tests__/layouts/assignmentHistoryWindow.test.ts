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

import {
  assignmentHistoryWindowInvalid,
  defaultAssignmentHistoryWindow,
  DEFAULT_ASSIGNMENT_HISTORY_DAYS,
  MAX_ASSIGNMENT_HISTORY_DAYS,
  toAssignmentHistoryFilters,
} from 'layouts/manageadmin/components/staff/assignmentHistory';
import { accountCalendar, addDays } from 'utils/accountCalendar';

const bogota = accountCalendar('America/Bogota', () => new Date('2027-03-02T03:30:00Z'));

describe('assignment history window', () => {
  test('defaults to the last days of the ACCOUNT calendar', () => {
    expect(defaultAssignmentHistoryWindow(bogota)).toEqual({
      from: addDays('2027-03-01', -DEFAULT_ASSIGNMENT_HISTORY_DAYS),
      to: '2027-03-01',
    });
  });

  test('bounds are the account-zone day edges, the last day included', () => {
    const filters = toAssignmentHistoryFilters(
      { staffId: 'd-1', transporterId: null, from: '2027-03-01', to: '2027-03-01' },
      bogota
    );

    expect(filters).toEqual({
      staffId: 'd-1',
      transporterId: null,
      from: '2027-03-01T05:00:00.000Z',
      to: '2027-03-02T04:59:59.999Z',
    });
  });

  test('the range is capped, and must be closed and forward', () => {
    expect(assignmentHistoryWindowInvalid({ from: '2027-01-01', to: '2027-01-01' })).toBe(false);
    expect(assignmentHistoryWindowInvalid({ from: '2027-01-01', to: addDays('2027-01-01', MAX_ASSIGNMENT_HISTORY_DAYS - 1) })).toBe(false);
    expect(assignmentHistoryWindowInvalid({ from: '2027-01-01', to: addDays('2027-01-01', MAX_ASSIGNMENT_HISTORY_DAYS) })).toBe(true);
    expect(assignmentHistoryWindowInvalid({ from: '2027-01-02', to: '2027-01-01' })).toBe(true);
    expect(assignmentHistoryWindowInvalid({ from: '', to: '2027-01-01' })).toBe(true);
  });
});
