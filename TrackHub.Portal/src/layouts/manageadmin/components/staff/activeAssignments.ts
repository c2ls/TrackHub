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
 * Builds the rows of the "Active Assignments" table.
 *
 * The active set comes from `staffAssignments(staffId)` (spec 09 §7.2), which
 * is unpaged and decides "active" server-side. That VM carries no assignment row
 * id — its `resourceId` is the transporter id — so the staff's own history
 * (paged to exhaustion, and bounded because it is filtered to one staff)
 * supplies the id the "End Assignment" action needs.
 *
 * Never rebuild this from a page of history: that query is ordered
 * `StartsAt DESC`, so a long-lived assignment falls below the page boundary and
 * silently disappears from the active list.
 */

import type { StaffActiveAssignment, StaffTransporterAssignment } from 'api/manager/staff';

export interface ActiveAssignmentRow {
  key: string;
  /** null for the synthesized default-transporter entry — there is no row to end. */
  assignmentId: string | null;
  staffName: string;
  transporterName: string;
  startsAt: string | null;
  endsAt: string | null;
  assignmentType: string | null;
  createdByPrincipal: string | null;
  isDefault: boolean;
}

export interface ActiveAssignmentContext {
  staffName: string;
  /** The staff's complete assignment history, used only to resolve row ids. */
  historyRows: StaffTransporterAssignment[];
  /** Account transporters, for naming a default-transporter entry. */
  transporterNames: Map<string, string>;
}

/**
 * Matches an active VM back to its history row. One active assignment per
 * (staff, transporter) pair is a backend invariant, so the transporter id
 * identifies the row; `startsAt` disambiguates it from ended rows for the same
 * pair.
 */
function findRow(
  rows: StaffTransporterAssignment[],
  transporterId: string,
  startsAt: string | null | undefined
): StaffTransporterAssignment | undefined {
  return rows.find(
    (row) =>
      row.transporterId === transporterId &&
      (startsAt ? row.startsAt === startsAt : (row.status || '').toUpperCase() === 'ACTIVE')
  );
}

export function buildActiveAssignmentRows(
  assignments: StaffActiveAssignment[],
  { staffName, historyRows, transporterNames }: ActiveAssignmentContext
): ActiveAssignmentRow[] {
  return assignments.map((assignment) => {
    const row = findRow(historyRows, assignment.resourceId, assignment.startsAt);
    return {
      key: row?.staffTransporterAssignmentId ?? `default-${assignment.resourceId}`,
      assignmentId: row?.staffTransporterAssignmentId ?? null,
      staffName: row?.staffName ?? staffName,
      transporterName:
        row?.transporterName ??
        transporterNames.get(assignment.resourceId) ??
        assignment.resourceId,
      startsAt: assignment.startsAt ?? null,
      endsAt: assignment.endsAt ?? null,
      assignmentType: assignment.assignmentType ?? null,
      createdByPrincipal: row?.createdByPrincipal ?? null,
      // No underlying row and no start instant: the synthesized
      // default-transporter entry (spec 09 §7.2), not an assignment record.
      isDefault: !row && !assignment.startsAt,
    };
  });
}
