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
 * Time-bounded staff↔unit assignments: the selected staff's active list plus
 * the account-wide filterable history, with assign/end actions. Gated by the
 * `workforce` feature — cosmetically only; the backend carries the authoritative
 * gate (and returns 409 on overlapping same-pair assignments).
 *
 * The active list comes from `staffAssignments(staffId)` (spec 09 §7.2), which
 * is unpaged and evaluates "active" server-side (Status = Active AND
 * StartsAt <= now < EndsAt). It is NOT a client-side filter over the history
 * query: that one is paged and ordered `StartsAt DESC`, so a staff assigned two
 * years ago and still active sorts below the page boundary, silently vanishes
 * from the active table, loses its "End Assignment" button, and makes any
 * reassignment of that pair fail with an unexplained 409.
 */

import { useContext, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import Icon from '@mui/material/Icon';
import Grid from '@mui/material/Grid';
import Table from 'controls/Tables/Table';
import TableAccordion from 'controls/Accordions/TableAccordion';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import ArgonBadge from 'components/ArgonBadge';
import ArgonBox from 'components/ArgonBox';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import {
  StaffPicker,
  TextCell,
  statusColor,
} from 'layouts/manageadmin/components/staff/workforceShared';
import { buildActiveAssignmentRows } from 'layouts/manageadmin/components/staff/activeAssignments';
import type { ActiveAssignmentRow } from 'layouts/manageadmin/components/staff/activeAssignments';
import {
  ASSIGNMENT_TYPES,
  assignmentTypeLabel,
  assignmentStatusLabel,
} from 'layouts/manageadmin/components/staff/qualificationConstants';
import {
  assignmentHistoryWindowInvalid,
  defaultAssignmentHistoryWindow,
  MAX_ASSIGNMENT_HISTORY_DAYS,
  toAssignmentHistoryFilters,
} from 'layouts/manageadmin/components/staff/assignmentHistory';
import SearchSelect from 'edition/SearchSelect';
import type { SearchOption } from 'edition/SearchSelect';
import { useAccountTransporterOptions } from 'edition/pickerOptions';
import { useAccountCalendar } from 'context/account';
import { useAccountByUser } from 'queries/accounts';
import { useTransporterNames } from 'queries/transporters';
import {
  useStaffAssignmentHistory,
  useStaffAssignmentHistoryPage,
  useStaffActiveAssignments,
  useAssignStaffToTransporter,
  useEndStaffAssignment,
} from 'queries/staff';
import type { StaffTransporterAssignment } from 'api/manager/staff';
import ServerPagination from 'controls/Tables/ServerPagination';
import { useClampPage, useServerList } from 'controls/Tables/useServerList';
import { usePermissions } from 'context/permissions';
import { PermissionActions, PermissionResources } from 'constants/permissions';
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from 'utils/dateUtils';

interface ConfirmState {
  open: boolean;
  id: string | null;
}

/** Pending filter form state (applied to the history query on "search"). */
interface FilterState {
  staff: SearchOption | null;
  transporter: SearchOption | null;
  from: string;
  to: string;
}

const HISTORY_PAGE_SIZE = 25;

function ManageStaffAssignments() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const calendar = useAccountCalendar();
  const [expanded, setExpanded] = useState(false);
  const [pending, setPending] = useState<FilterState>(() => ({
    staff: null,
    transporter: null,
    ...defaultAssignmentHistoryWindow(calendar),
  }));
  const [applied, setApplied] = useState<FilterState>(pending);
  const [assignStaff, setAssignStaff] = useState<SearchOption | null>(null);
  const assignStaffId = assignStaff?.value ?? '';
  const [assignTransporter, setAssignTransporter] = useState<SearchOption | null>(null);
  const [assignType, setAssignType] = useState<string>(ASSIGNMENT_TYPES[0]);
  const [confirm, setConfirm] = useState<ConfirmState>({ open: false, id: null });
  const { can } = usePermissions();
  const canAssign = can(PermissionResources.StaffOperations, PermissionActions.Write);
  const canEnd = can(PermissionResources.StaffOperations, PermissionActions.Edit);
  const pendingWindowInvalid = assignmentHistoryWindowInvalid(pending);
  const appliedWindowInvalid = assignmentHistoryWindowInvalid(applied);

  const accountQuery = useAccountByUser({ enabled: expanded });
  const accountId = accountQuery.data?.accountId;

  // Active list: the server-side, unpaged, time-aware projection for the staff
  // currently selected in the assign form.
  const activeQuery = useStaffActiveAssignments(assignStaffId, {
    enabled: expanded && !!assignStaffId,
  });
  // The active VM carries no assignment row id (its `resourceId` is the
  // transporter id), so the staff's own history — bounded, and paged to
  // exhaustion — supplies the row id that "End Assignment" needs.
  const staffHistoryQuery = useStaffAssignmentHistory(
    accountId,
    { staffId: assignStaffId || null },
    { enabled: expanded && !!accountId && !!assignStaffId }
  );
  const { page, setPage, params } = useServerList(HISTORY_PAGE_SIZE);
  const historyFilters = toAssignmentHistoryFilters(
    {
      staffId: applied.staff?.value ?? null,
      transporterId: applied.transporter?.value ?? null,
      from: applied.from,
      to: applied.to,
    },
    calendar
  );
  const historyQuery = useStaffAssignmentHistoryPage(accountId, historyFilters, params, {
    enabled: expanded && !!accountId && !appliedWindowInvalid,
  });
  const history = historyQuery.data?.items ?? [];
  const historyTotal = historyQuery.data?.totalCount ?? 0;
  useClampPage(page, HISTORY_PAGE_SIZE, historyTotal, setPage);

  // Only the synthesized default-unit entry lacks a history row to take its name from.
  const activeTransporterIds = useMemo(
    () => (activeQuery.data ?? []).map((assignment) => assignment.resourceId),
    [activeQuery.data]
  );
  const transporterNames = useTransporterNames(activeTransporterIds);

  const active = useMemo<ActiveAssignmentRow[]>(
    () =>
      buildActiveAssignmentRows(activeQuery.data ?? [], {
        staffName: assignStaff?.label ?? '',
        historyRows: staffHistoryQuery.data ?? [],
        transporterNames,
      }),
    [activeQuery.data, staffHistoryQuery.data, assignStaff, transporterNames]
  );

  const assignStaffToTransporter = useAssignStaffToTransporter();
  const endAssignment = useEndStaffAssignment();

  useEffect(() => {
    setLoading(activeQuery.isFetching || staffHistoryQuery.isFetching || historyQuery.isFetching);
  }, [activeQuery.isFetching, staffHistoryQuery.isFetching, historyQuery.isFetching, setLoading]);

  const handleAssign = async () => {
    if (!assignStaffId || !assignTransporter) return;
    setLoading(true);
    try {
      await assignStaffToTransporter.mutateAsync({
        staffId: assignStaffId,
        transporterId: assignTransporter.value,
        startsAt: new Date().toISOString(),
        assignmentType: assignType,
      });
      setAssignTransporter(null);
    } catch {
      // Failure (including the 409 on an overlapping same-pair assignment) is
      // surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const doEnd = async () => {
    const id = confirm.id;
    setConfirm({ open: false, id: null });
    if (!id) return;
    setLoading(true);
    try {
      await endAssignment.mutateAsync({ staffTransporterAssignmentId: id, endsAt: null });
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const activeColumns = [
    { name: 'transporter', title: t('workforce.assignments.transporter'), align: 'left' as const },
    { name: 'startsAt', title: t('workforce.assignments.startsAt'), align: 'center' as const },
    { name: 'endsAt', title: t('workforce.assignments.endsAt'), align: 'center' as const },
    { name: 'type', title: t('workforce.assignments.type'), align: 'center' as const },
    { name: 'createdBy', title: t('workforce.assignments.createdBy'), align: 'center' as const },
    { name: 'action', title: t('generic.action'), align: 'center' as const },
    { name: 'id' },
  ];

  const toActiveRow = (row: ActiveAssignmentRow): Record<string, unknown> => ({
    transporter: (
      <ArgonTypography variant="caption" fontWeight="medium">
        {row.transporterName}
      </ArgonTypography>
    ),
    startsAt: <TextCell>{formatDateTime(row.startsAt)}</TextCell>,
    endsAt: <TextCell>{formatDateTime(row.endsAt)}</TextCell>,
    type: (
      <TextCell>
        {row.isDefault
          ? t('workforce.assignments.defaultTransporter')
          : assignmentTypeLabel(t, row.assignmentType)}
      </TextCell>
    ),
    createdBy: <TextCell>{row.createdByPrincipal}</TextCell>,
    action: !row.assignmentId ? (
      <TextCell>{t('workforce.assignments.defaultTransporterHint')}</TextCell>
    ) : canEnd ? (
      <ArgonButton
        variant="text"
        color="error"
        onClick={() => setConfirm({ open: true, id: row.assignmentId })}
      >
        <Icon>stop_circle</Icon>&nbsp;{t('workforce.assignments.end')}
      </ArgonButton>
    ) : (
      <TextCell />
    ),
    id: row.key,
  });

  const historyColumns = [
    { name: 'staff', title: t('workforce.expirations.staff'), align: 'left' as const },
    { name: 'transporter', title: t('workforce.assignments.transporter'), align: 'left' as const },
    { name: 'startsAt', title: t('workforce.assignments.startsAt'), align: 'center' as const },
    { name: 'endsAt', title: t('workforce.assignments.endsAt'), align: 'center' as const },
    { name: 'type', title: t('workforce.assignments.type'), align: 'center' as const },
    { name: 'status', title: t('workforce.assignments.status'), align: 'center' as const },
    { name: 'createdBy', title: t('workforce.assignments.createdBy'), align: 'center' as const },
    { name: 'id' },
  ];

  const toHistoryRow = (assignment: StaffTransporterAssignment): Record<string, unknown> => ({
    staff: (
      <ArgonTypography variant="caption" fontWeight="medium">
        {assignment.staffName}
      </ArgonTypography>
    ),
    transporter: <TextCell>{assignment.transporterName}</TextCell>,
    startsAt: <TextCell>{formatDateTime(assignment.startsAt)}</TextCell>,
    endsAt: <TextCell>{formatDateTime(assignment.endsAt)}</TextCell>,
    type: <TextCell>{assignmentTypeLabel(t, assignment.assignmentType)}</TextCell>,
    status: (
      <ArgonBadge
        badgeContent={assignmentStatusLabel(t, assignment.status)}
        color={statusColor(assignment.status)}
        size="xs"
        container
      />
    ),
    createdBy: <TextCell>{assignment.createdByPrincipal}</TextCell>,
    id: assignment.staffTransporterAssignmentId,
  });

  return (
    <>
      <TableAccordion sectionKey="staff-assignments"
        title={t('workforce.assignments.title')}
        expanded={expanded}
        setExpanded={setExpanded}
      >
        {/* Assign form */}
        <ArgonBox mb={2}>
          <Grid container spacing={1} sx={{ alignItems: "center" }}>
            <Grid size={{ xs: 12, md: 4 }}>
              <StaffPicker
                id="assignStaffId"
                value={assignStaff}
                onChange={setAssignStaff}
                label={t('workforce.selectStaff')}
                placeholder={t('workforce.selectStaffPlaceholder')}
              />
            </Grid>
            {canAssign && (
              <>
                <Grid size={{ xs: 12, md: 4 }}>
                  <SearchSelect
                    id="assignTransporterId"
                    label={t('workforce.assignments.transporter')}
                    value={assignTransporter?.value ?? null}
                    valueLabel={assignTransporter?.label}
                    onChange={setAssignTransporter}
                    useOptions={useAccountTransporterOptions}
                    placeholder={t('workforce.assignments.selectTransporter')}
                  />
                </Grid>
                <Grid size={{ xs: 12, md: 2 }}>
                  <CustomSelect
                    list={ASSIGNMENT_TYPES.map((type) => ({
                      value: type,
                      label: assignmentTypeLabel(t, type),
                    }))}
                    name="assignType"
                    id="assignType"
                    label={t('workforce.assignments.type')}
                    value={assignType}
                    handleChange={(event) => setAssignType(String(event.target.value ?? ''))}
                    numericValue={false}
                  />
                </Grid>
                <Grid size={{ xs: 12, md: 2 }}>
                  <ArgonButton
                    color="info"
                    onClick={handleAssign}
                    disabled={!assignStaffId || !assignTransporter}
                  >
                    {t('workforce.assignments.assign')}
                  </ArgonButton>
                </Grid>
              </>
            )}
          </Grid>
        </ArgonBox>

        {/* Active assignments */}
        <ArgonBox mb={1}>
          <ArgonTypography variant="button" fontWeight="bold" textTransform="uppercase" color="text">
            {t('workforce.assignments.active')}
          </ArgonTypography>
        </ArgonBox>
        {!assignStaffId ? (
          <ArgonTypography variant="caption" color="secondary">
            {t('workforce.selectStaffHint')}
          </ArgonTypography>
        ) : active.length === 0 ? (
          <ArgonTypography variant="caption" color="secondary">
            {t('workforce.assignments.emptyActive')}
          </ArgonTypography>
        ) : (
          <Table
            columns={activeColumns}
            rows={active.map(toActiveRow)}
            selectedField="transporter"
          />
        )}

        {/* History + filters */}
        <ArgonBox mt={3} mb={1}>
          <ArgonTypography variant="button" fontWeight="bold" textTransform="uppercase" color="text">
            {t('workforce.assignments.history')}
          </ArgonTypography>
        </ArgonBox>
        <ArgonBox mb={2}>
          <Grid container spacing={1} sx={{ alignItems: "flex-end" }}>
            <Grid size={{ xs: 12, md: 3 }}>
              <StaffPicker
                id="filterStaffId"
                value={pending.staff}
                onChange={(staff) => setPending((prev) => ({ ...prev, staff }))}
                label={t('workforce.selectStaff')}
                placeholder={t('workforce.selectStaffPlaceholder')}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 3 }}>
              <SearchSelect
                id="filterTransporterId"
                label={t('workforce.assignments.transporter')}
                value={pending.transporter?.value ?? null}
                valueLabel={pending.transporter?.label}
                onChange={(transporter) => setPending((prev) => ({ ...prev, transporter }))}
                useOptions={useAccountTransporterOptions}
                placeholder={t('workforce.assignments.selectTransporter')}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 2 }}>
              <CustomTextField
                margin="none"
                name="from"
                id="filterFrom"
                label={t('workforce.assignments.from')}
                type="date"
                value={pending.from}
                onChange={(event) =>
                  setPending((prev) => ({ ...prev, from: event.target.value }))
                }
              />
            </Grid>
            <Grid size={{ xs: 12, md: 2 }}>
              <CustomTextField
                margin="none"
                name="to"
                id="filterTo"
                label={t('workforce.assignments.to')}
                type="date"
                value={pending.to}
                onChange={(event) => setPending((prev) => ({ ...prev, to: event.target.value }))}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 2 }}>
              <ArgonButton
                color="primary"
                size="small"
                disabled={pendingWindowInvalid}
                onClick={() => { setPage(0); setApplied(pending); }}
              >
                <Icon>search</Icon>&nbsp;{t('workforce.assignments.search')}
              </ArgonButton>
            </Grid>
          </Grid>
        </ArgonBox>
        {pendingWindowInvalid ? (
          <ArgonTypography variant="caption" color="error">
            {t('workforce.assignments.windowInvalid', { days: MAX_ASSIGNMENT_HISTORY_DAYS })}
          </ArgonTypography>
        ) : history.length === 0 ? (
          <ArgonTypography variant="caption" color="secondary">
            {t('workforce.assignments.empty')}
          </ArgonTypography>
        ) : (
          <>
            <Table columns={historyColumns} rows={history.map(toHistoryRow)} selectedField="staff" serverPaged />
            <ServerPagination page={page} pageSize={HISTORY_PAGE_SIZE} totalCount={historyTotal} pageLength={history.length} onPageChange={setPage} />
          </>
        )}
      </TableAccordion>

      <ConfirmDialog
        open={confirm.open}
        setOpen={(next) =>
          setConfirm((prev) => ({
            ...prev,
            open: typeof next === 'function' ? next(prev.open) : next,
          }))
        }
        title={t('workforce.assignments.end')}
        message={t('workforce.assignments.endConfirm')}
        onConfirm={doEnd}
      />
    </>
  );
}

export default ManageStaffAssignments;
