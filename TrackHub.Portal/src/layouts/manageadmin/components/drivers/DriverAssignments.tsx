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
 * Time-bounded driver↔unit assignments: the selected driver's active list plus
 * the account-wide filterable history, with assign/end actions. Gated by the
 * `workforce` feature — cosmetically only; the backend carries the authoritative
 * gate (and returns 409 on overlapping same-pair assignments).
 *
 * The active list comes from `driverAssignments(driverId)` (spec 09 §7.2), which
 * is unpaged and evaluates "active" server-side (Status = Active AND
 * StartsAt <= now < EndsAt). It is NOT a client-side filter over the history
 * query: that one is paged and ordered `StartsAt DESC`, so a driver assigned two
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
  DriverPicker,
  TextCell,
  statusColor,
} from 'layouts/manageadmin/components/drivers/workforceShared';
import { buildActiveAssignmentRows } from 'layouts/manageadmin/components/drivers/activeAssignments';
import type { ActiveAssignmentRow } from 'layouts/manageadmin/components/drivers/activeAssignments';
import {
  ASSIGNMENT_TYPES,
  assignmentTypeLabel,
  assignmentStatusLabel,
} from 'layouts/manageadmin/components/drivers/qualificationConstants';
import { useAccountByUser } from 'queries/accounts';
import { useTransporterLookupByAccount } from 'queries/transporters';
import {
  useDriversByAccount,
  useDriverAssignmentHistory,
  useDriverActiveAssignments,
  useAssignDriverToTransporter,
  useEndDriverAssignment,
} from 'queries/drivers';
import type {
  DriverAssignmentHistoryFilters,
  DriverTransporterAssignment,
} from 'api/manager/drivers';
import { MAX_FETCH_ALL_ITEMS } from 'api/core/paging';
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from 'utils/dateUtils';

interface ConfirmState {
  open: boolean;
  id: string | null;
}

/** Pending filter form state (applied to the history query on "search"). */
interface FilterState {
  driverId: string;
  transporterId: string;
  from: string;
  to: string;
}

const EMPTY_FILTERS: FilterState = { driverId: '', transporterId: '', from: '', to: '' };

function toFilters(state: FilterState): DriverAssignmentHistoryFilters {
  return {
    driverId: state.driverId || null,
    transporterId: state.transporterId || null,
    from: state.from ? new Date(state.from).toISOString() : null,
    to: state.to ? new Date(state.to).toISOString() : null,
  };
}

function ManageDriverAssignments() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [pending, setPending] = useState<FilterState>(EMPTY_FILTERS);
  const [applied, setApplied] = useState<FilterState>(EMPTY_FILTERS);
  const [assignDriverId, setAssignDriverId] = useState('');
  const [assignTransporterId, setAssignTransporterId] = useState('');
  const [assignType, setAssignType] = useState<string>(ASSIGNMENT_TYPES[0]);
  const [confirm, setConfirm] = useState<ConfirmState>({ open: false, id: null });

  const accountQuery = useAccountByUser({ enabled: expanded });
  const accountId = accountQuery.data?.accountId;
  const driversQuery = useDriversByAccount(accountId, { enabled: expanded && !!accountId });
  const drivers = driversQuery.data ?? [];
  const transportersQuery = useTransporterLookupByAccount({ enabled: expanded });
  const transporters = transportersQuery.data ?? [];

  // Active list: the server-side, unpaged, time-aware projection for the driver
  // currently selected in the assign form.
  const activeQuery = useDriverActiveAssignments(assignDriverId, {
    enabled: expanded && !!assignDriverId,
  });
  // The active VM carries no assignment row id (its `resourceId` is the
  // transporter id), so the driver's own history — bounded, and paged to
  // exhaustion — supplies the row id that "End Assignment" needs.
  const driverHistoryQuery = useDriverAssignmentHistory(
    accountId,
    { driverId: assignDriverId || null },
    { enabled: expanded && !!accountId && !!assignDriverId }
  );
  const historyQuery = useDriverAssignmentHistory(accountId, toFilters(applied), {
    enabled: expanded && !!accountId,
  });
  const history = historyQuery.data ?? [];

  const active = useMemo<ActiveAssignmentRow[]>(
    () =>
      buildActiveAssignmentRows(activeQuery.data ?? [], {
        driverName: drivers.find((driver) => driver.driverId === assignDriverId)?.name ?? '',
        historyRows: driverHistoryQuery.data ?? [],
        transporterNames: new Map(
          transporters.map((transporter) => [transporter.transporterId, transporter.name])
        ),
      }),
    [activeQuery.data, driverHistoryQuery.data, drivers, transporters, assignDriverId]
  );

  const assignDriver = useAssignDriverToTransporter();
  const endAssignment = useEndDriverAssignment();

  useEffect(() => {
    setLoading(
      activeQuery.isFetching ||
        driverHistoryQuery.isFetching ||
        historyQuery.isFetching ||
        driversQuery.isFetching
    );
  }, [
    activeQuery.isFetching,
    driverHistoryQuery.isFetching,
    historyQuery.isFetching,
    driversQuery.isFetching,
    setLoading,
  ]);

  const handleAssign = async () => {
    if (!assignDriverId || !assignTransporterId) return;
    setLoading(true);
    try {
      await assignDriver.mutateAsync({
        driverId: assignDriverId,
        transporterId: assignTransporterId,
        startsAt: new Date().toISOString(),
        assignmentType: assignType,
      });
      setAssignTransporterId('');
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
      await endAssignment.mutateAsync({ driverTransporterAssignmentId: id, endsAt: null });
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
    action: row.assignmentId ? (
      <ArgonButton
        variant="text"
        color="error"
        onClick={() => setConfirm({ open: true, id: row.assignmentId })}
      >
        <Icon>stop_circle</Icon>&nbsp;{t('workforce.assignments.end')}
      </ArgonButton>
    ) : (
      <TextCell>{t('workforce.assignments.defaultTransporterHint')}</TextCell>
    ),
    id: row.key,
  });

  const historyColumns = [
    { name: 'driver', title: t('workforce.expirations.driver'), align: 'left' as const },
    { name: 'transporter', title: t('workforce.assignments.transporter'), align: 'left' as const },
    { name: 'startsAt', title: t('workforce.assignments.startsAt'), align: 'center' as const },
    { name: 'endsAt', title: t('workforce.assignments.endsAt'), align: 'center' as const },
    { name: 'type', title: t('workforce.assignments.type'), align: 'center' as const },
    { name: 'status', title: t('workforce.assignments.status'), align: 'center' as const },
    { name: 'createdBy', title: t('workforce.assignments.createdBy'), align: 'center' as const },
    { name: 'id' },
  ];

  const toHistoryRow = (assignment: DriverTransporterAssignment): Record<string, unknown> => ({
    driver: (
      <ArgonTypography variant="caption" fontWeight="medium">
        {assignment.driverName}
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
    id: assignment.driverTransporterAssignmentId,
  });

  return (
    <>
      <TableAccordion
        title={t('workforce.assignments.title')}
        expanded={expanded}
        setExpanded={setExpanded}
      >
        {/* Assign form */}
        <ArgonBox mb={2}>
          <Grid container spacing={1} sx={{ alignItems: "center" }}>
            <Grid size={{ xs: 12, md: 4 }}>
              <DriverPicker
                id="assignDriverId"
                drivers={drivers}
                value={assignDriverId}
                onChange={setAssignDriverId}
                label={t('workforce.selectDriver')}
                placeholder={t('workforce.selectDriverPlaceholder')}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <CustomSelect
                list={transporters.map((transporter) => ({
                  value: transporter.transporterId,
                  label: transporter.name,
                }))}
                name="assignTransporterId"
                id="assignTransporterId"
                label={t('workforce.assignments.transporter')}
                value={assignTransporterId}
                handleChange={(event) => setAssignTransporterId(String(event.target.value ?? ''))}
                numericValue={false}
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
                disabled={!assignDriverId || !assignTransporterId}
              >
                {t('workforce.assignments.assign')}
              </ArgonButton>
            </Grid>
          </Grid>
        </ArgonBox>

        {/* Active assignments */}
        <ArgonBox mb={1}>
          <ArgonTypography variant="button" fontWeight="bold" textTransform="uppercase" color="text">
            {t('workforce.assignments.active')}
          </ArgonTypography>
        </ArgonBox>
        {!assignDriverId ? (
          <ArgonTypography variant="caption" color="secondary">
            {t('workforce.selectDriverHint')}
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
              <DriverPicker
                id="filterDriverId"
                drivers={drivers}
                value={pending.driverId}
                onChange={(value) => setPending((prev) => ({ ...prev, driverId: value }))}
                label={t('workforce.selectDriver')}
                placeholder={t('workforce.selectDriverPlaceholder')}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 3 }}>
              <CustomSelect
                list={transporters.map((transporter) => ({
                  value: transporter.transporterId,
                  label: transporter.name,
                }))}
                name="filterTransporterId"
                id="filterTransporterId"
                label={t('workforce.assignments.transporter')}
                value={pending.transporterId}
                handleChange={(event) =>
                  setPending((prev) => ({
                    ...prev,
                    transporterId: String(event.target.value ?? ''),
                  }))
                }
                numericValue={false}
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
              <ArgonButton color="primary" size="small" onClick={() => setApplied(pending)}>
                <Icon>search</Icon>&nbsp;{t('workforce.assignments.search')}
              </ArgonButton>
            </Grid>
          </Grid>
        </ArgonBox>
        {history.length === 0 ? (
          <ArgonTypography variant="caption" color="secondary">
            {t('workforce.assignments.empty')}
          </ArgonTypography>
        ) : (
          <>
            {history.length >= MAX_FETCH_ALL_ITEMS && (
              <ArgonBox mb={1}>
                <ArgonTypography variant="caption" color="error">
                  {t('workforce.assignments.tooManyResults', { count: MAX_FETCH_ALL_ITEMS })}
                </ArgonTypography>
              </ArgonBox>
            )}
            <Table columns={historyColumns} rows={history.map(toHistoryRow)} selectedField="driver" />
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

export default ManageDriverAssignments;
