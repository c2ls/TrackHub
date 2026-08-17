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

import { buildActiveAssignmentRows } from 'layouts/manageadmin/components/drivers/activeAssignments';
import type {
  DriverActiveAssignment,
  DriverTransporterAssignment,
} from 'api/manager/drivers';

const DRIVER_ID = '11111111-1111-1111-1111-111111111111';
const UNIT_A = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
const UNIT_B = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb';
const ROW_A = 'cccccccc-cccc-cccc-cccc-cccccccccccc';

const activeVm = (
  transporterId: string,
  startsAt: string | null,
  assignmentType: string | null = 'Regular'
): DriverActiveAssignment =>
  ({
    driverId: DRIVER_ID,
    accountId: 'account',
    resourceType: 'Transporter',
    resourceId: transporterId,
    active: true,
    startsAt,
    endsAt: null,
    assignmentType,
  }) as DriverActiveAssignment;

const historyRow = (
  id: string,
  transporterId: string,
  startsAt: string,
  status = 'Active'
): DriverTransporterAssignment =>
  ({
    driverTransporterAssignmentId: id,
    accountId: 'account',
    driverId: DRIVER_ID,
    driverName: 'Ana Driver',
    transporterId,
    transporterName: 'Unit A',
    startsAt,
    endsAt: null,
    assignmentType: 'Regular',
    status,
    createdByPrincipal: 'admin',
    lastModified: startsAt,
  }) as DriverTransporterAssignment;

const context = (rows: DriverTransporterAssignment[]) => ({
  driverName: 'Ana Driver',
  historyRows: rows,
  transporterNames: new Map([
    [UNIT_A, 'Unit A'],
    [UNIT_B, 'Unit B'],
  ]),
});

describe('buildActiveAssignmentRows', () => {
  test('keeps a long-lived assignment that history paging would have hidden', () => {
    // The regression: assigned two years ago, still active. It sorts last under
    // `StartsAt DESC`, so a client-side filter over one page dropped it. The
    // server-side active query lists it regardless.
    const vm = activeVm(UNIT_A, '2024-01-05T08:00:00Z');
    const rows = buildActiveAssignmentRows([vm], context([historyRow(ROW_A, UNIT_A, '2024-01-05T08:00:00Z')]));
    expect(rows).toHaveLength(1);
    expect(rows[0].assignmentId).toBe(ROW_A);
    expect(rows[0].isDefault).toBe(false);
  });

  test('resolves the row id so End Assignment is available', () => {
    const rows = buildActiveAssignmentRows(
      [activeVm(UNIT_A, '2026-05-01T00:00:00Z')],
      context([historyRow(ROW_A, UNIT_A, '2026-05-01T00:00:00Z')])
    );
    expect(rows[0].assignmentId).toBe(ROW_A);
    expect(rows[0].createdByPrincipal).toBe('admin');
  });

  test('picks the row with the matching start, not an ended one for the same pair', () => {
    const ended = historyRow('dddddddd-dddd-dddd-dddd-dddddddddddd', UNIT_A, '2023-01-01T00:00:00Z', 'Ended');
    const current = historyRow(ROW_A, UNIT_A, '2026-05-01T00:00:00Z');
    const rows = buildActiveAssignmentRows(
      [activeVm(UNIT_A, '2026-05-01T00:00:00Z')],
      context([ended, current])
    );
    expect(rows[0].assignmentId).toBe(ROW_A);
  });

  test('marks the synthesized default-transporter entry and offers no End action', () => {
    const rows = buildActiveAssignmentRows([activeVm(UNIT_B, null, null)], context([]));
    expect(rows[0].isDefault).toBe(true);
    expect(rows[0].assignmentId).toBeNull();
    expect(rows[0].transporterName).toBe('Unit B');
  });

  test('falls back to the transporter id when the unit name is unknown', () => {
    const unknown = 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee';
    const rows = buildActiveAssignmentRows([activeVm(unknown, null, null)], context([]));
    expect(rows[0].transporterName).toBe(unknown);
  });

  test('still renders an active row whose history row is missing', () => {
    // History unavailable (or trimmed) must not make the assignment vanish —
    // it renders without the End button rather than disappearing.
    const rows = buildActiveAssignmentRows([activeVm(UNIT_A, '2026-05-01T00:00:00Z')], context([]));
    expect(rows).toHaveLength(1);
    expect(rows[0].assignmentId).toBeNull();
    expect(rows[0].isDefault).toBe(false);
    expect(rows[0].driverName).toBe('Ana Driver');
  });

  test('supports several concurrent assignments to different units', () => {
    const rowB = 'ffffffff-ffff-ffff-ffff-ffffffffffff';
    const rows = buildActiveAssignmentRows(
      [activeVm(UNIT_A, '2026-05-01T00:00:00Z'), activeVm(UNIT_B, '2026-06-01T00:00:00Z')],
      context([
        historyRow(ROW_A, UNIT_A, '2026-05-01T00:00:00Z'),
        historyRow(rowB, UNIT_B, '2026-06-01T00:00:00Z'),
      ])
    );
    expect(rows.map((row) => row.assignmentId)).toEqual([ROW_A, rowB]);
    expect(new Set(rows.map((row) => row.key)).size).toBe(2);
  });

  test('returns nothing when the driver has no active assignment', () => {
    expect(buildActiveAssignmentRows([], context([]))).toEqual([]);
  });
});
