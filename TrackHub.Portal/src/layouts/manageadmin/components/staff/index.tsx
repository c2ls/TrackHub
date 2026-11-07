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
 * Staff: one row per staff, a search box for accounts with hundreds of them, and three
 * icon actions. Credentials and devices live in the detail dialog rather than in another
 * accordion.
 */

import { useContext, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import Icon from '@mui/material/Icon';
import Tooltip from '@mui/material/Tooltip';
import Table from 'controls/Tables/Table';
import ServerPagination from 'controls/Tables/ServerPagination';
import ServerSearch from 'controls/Tables/ServerSearch';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import { useClampPage, useServerList } from 'controls/Tables/useServerList';
import TableAccordion from 'controls/Accordions/TableAccordion';
import ArgonBadge from 'components/ArgonBadge';
import ArgonBox from 'components/ArgonBox';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import useForm from 'controls/Dialogs/useForm';
import StaffDialog from 'layouts/manageadmin/components/staff/StaffDialog';
import type { StaffFormValues } from 'layouts/manageadmin/components/staff/StaffDialog';
import StaffDetailDialog from 'layouts/manageadmin/components/staff/StaffDetailDialog';
import ManageStaffQualifications from 'layouts/manageadmin/components/staff/StaffQualifications';
import QualificationExpirations from 'layouts/manageadmin/components/staff/QualificationExpirations';
import { TextCell, statusColor } from 'layouts/manageadmin/components/staff/workforceShared';
import type { BadgeColor } from 'layouts/manageadmin/components/staff/workforceShared';
import { credentialState } from 'layouts/manageadmin/components/staff/credentialLifecycle';
import { DEFAULT_STAFF_TYPE, STAFF_TYPES, staffTypeLabelKey } from 'layouts/manageadmin/components/staff/staffTypes';
import { useFeatures } from 'context/features';
import { useAccountByUser } from 'queries/accounts';
import { useStaffMembersPage, useCreateStaffMember, useUpdateStaffMember, useDeactivateStaffMember } from 'queries/staff';
import { useStaffCredentials } from 'queries/staffIdentity';
import type { Staff, StaffMemberDtoInput } from 'api/manager/staff';
import type { StaffCredential } from 'api/security/staffIdentity';
import { LoadingContext } from 'LoadingContext';

const WORKFORCE_FEATURE_KEY = 'workforce';
const PAGE_SIZE = 25;

function ManageStaff() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const { isFeatureEnabled } = useFeatures();
  const workforceEnabled = isFeatureEnabled(WORKFORCE_FEATURE_KEY);
  const [expanded, setExpanded] = useState(false);
  const [open, setOpen] = useState(false);
  const [detail, setDetail] = useState<Staff | null>(null);
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<StaffFormValues>({ active: true });

  const accountQuery = useAccountByUser({ enabled: expanded });
  const account = accountQuery.data ?? null;
  const accountId = account?.accountId;
  const { page, setPage, searchDraft, setSearchDraft, params } = useServerList(PAGE_SIZE);
  const [staffType, setStaffType] = useState('');
  const staffMembersQuery = useStaffMembersPage(accountId, { ...params, staffType: staffType || null }, { enabled: expanded && !!accountId });
  const staff = staffMembersQuery.data?.items ?? [];
  const totalCount = staffMembersQuery.data?.totalCount ?? 0;
  useClampPage(page, PAGE_SIZE, totalCount, setPage);
  // One account-wide read feeds the credential column; the per-staff detail refetches its own.
  const credentialsQuery = useStaffCredentials(accountId, null, { enabled: expanded && !!accountId });
  const createStaffMember = useCreateStaffMember();
  const updateStaffMember = useUpdateStaffMember();
  const deactivateStaffMember = useDeactivateStaffMember();

  useEffect(() => {
    setLoading(accountQuery.isFetching || staffMembersQuery.isFetching);
  }, [accountQuery.isFetching, staffMembersQuery.isFetching, setLoading]);

  // The credential that speaks for a staff: an active one wins, else the newest of the rest.
  const credentialByStaff = useMemo(() => {
    const map = new Map<string, StaffCredential>();
    for (const credential of credentialsQuery.data ?? []) {
      const current = map.get(credential.staffId);
      if (!current || (!current.active && credential.active)) map.set(credential.staffId, credential);
    }
    return map;
  }, [credentialsQuery.data]);

  const accessBadge = (staff: Staff): { label: string; color: BadgeColor } => {
    const credential = credentialByStaff.get(staff.staffId);
    if (!credential) return { label: t('staff.accessNone'), color: 'secondary' };
    switch (credentialState(credential)) {
      case 'pending':
        return { label: t('workforce.credentials.statusPending'), color: 'info' };
      case 'revoked':
        return { label: t('workforce.credentials.statusRevoked'), color: statusColor('REVOKED') };
      case 'locked':
        return { label: t('workforce.credentials.statusLocked'), color: 'warning' };
      default:
        return { label: t('workforce.credentials.statusActive'), color: statusColor('ACTIVE') };
    }
  };

  const handleAddClick = () => {
    setValues({ accountId, active: true, staffType: DEFAULT_STAFF_TYPE });
    setErrors({});
  };

  const handleEdit = (staff: Staff) => {
    setValues({ ...staff, accountId: accountId || staff.accountId });
    setErrors({});
    setOpen(true);
  };

  const handleSubmit = async () => {
    if (!validate(['name']) || !accountId) return;
    setLoading(true);
    try {
      const staff = { ...values, accountId, active: values.active !== false, staffType: values.staffType || DEFAULT_STAFF_TYPE, expectedVersion: values.version ?? null };
      if (staff.staffId) {
        await updateStaffMember.mutateAsync({ staffId: staff.staffId, staff: staff as StaffMemberDtoInput });
      } else {
        await createStaffMember.mutateAsync(staff as StaffMemberDtoInput);
      }
      setOpen(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open.
    } finally {
      setLoading(false);
    }
  };

  const handleDeactivate = async (staff: Staff) => {
    if (!staff?.staffId || !window.confirm(t('staff.deactivateConfirmation'))) return;
    setLoading(true);
    try {
      await deactivateStaffMember.mutateAsync(staff.staffId);
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const iconAction = (key: string, icon: string, color: 'dark' | 'info' | 'error', label: string, onClick: () => void) => (
    <Tooltip key={key} title={label}>
      <ArgonButton variant="text" color={color} size="small" iconOnly onClick={onClick}>
        <Icon>{icon}</Icon>
      </ArgonButton>
    </Tooltip>
  );

  return (
    <>
      <TableAccordion sectionKey="staff_members"
        title={t('staff.title')}
        showAddIcon={true}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <ArgonBox display="flex" alignItems="center" gap={2} flexWrap="wrap">
          <ArgonBox flexGrow={1} minWidth={240}>
            <ServerSearch value={searchDraft} onChange={setSearchDraft} placeholder={t('staff.search')} />
          </ArgonBox>
          <CustomSelect
            list={STAFF_TYPES.map((type) => ({ value: type, label: t(staffTypeLabelKey(type)) }))}
            handleChange={(event) => {
              setStaffType(String(event.target.value));
              setPage(0);
            }}
            name="staffTypeFilter"
            id="staffTypeFilter"
            value={staffType}
            numericValue={false}
            allowEmpty
            placeholder={t('staff.allTypes')}
            fullWidth={false}
          />
        </ArgonBox>
        <Table
          serverPaged
          columns={[
            { name: 'name', title: t('staff.name'), align: 'left' },
            { name: 'staffType', title: t('staff.staffType'), align: 'center' },
            { name: 'document', title: t('staff.document'), align: 'center' },
            { name: 'phone', title: t('staff.phone'), align: 'center' },
            { name: 'access', title: t('staff.access'), align: 'center' },
            { name: 'active', title: t('generic.active'), align: 'center' },
            { name: 'action', title: t('generic.action'), align: 'center', width: '140px' },
            { name: 'id' }
          ]}
          rows={staff.map(staff => {
            const access = accessBadge(staff);
            return {
              name: <TextCell>{staff.name}</TextCell>,
              staffType: <TextCell>{t(staffTypeLabelKey(staff.staffType))}</TextCell>,
              document: <TextCell>{staff.documentNumber}</TextCell>,
              phone: <TextCell>{staff.phone}</TextCell>,
              access: <ArgonBadge badgeContent={access.label} color={access.color} size="xs" container />,
              active: <TextCell>{staff.active ? t('generic.yes') : t('generic.no')}</TextCell>,
              action: (
                <ArgonBox display="flex" justifyContent="center" gap={0.5}>
                  {iconAction('manage', 'badge', 'info', t('staff.manage'), () => setDetail(staff))}
                  {iconAction('edit', 'edit', 'dark', t('generic.edit'), () => handleEdit(staff))}
                  {staff.active && iconAction('deactivate', 'block', 'error', t('staff.deactivate'), () => handleDeactivate(staff))}
                </ArgonBox>
              ),
              id: staff.staffId
            };
          })}
          selectedField="name"
        />
        <ServerPagination page={page} pageSize={PAGE_SIZE} totalCount={totalCount} pageLength={staff.length} onPageChange={setPage} />
        {staff.length === 0 && (
          <ArgonTypography variant="caption" color="secondary">
            {t('staff.empty')}
          </ArgonTypography>
        )}
      </TableAccordion>

      {/* Billable workforce surfaces — hidden without the feature (cosmetic
          only; the backend gate is authoritative). Assignments are day-to-day dispatch and live on
          the dashboard's staff tab. */}
      {workforceEnabled && (
        <>
          <ManageStaffQualifications />
          <QualificationExpirations />
        </>
      )}

      <StaffDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        errors={errors}
      />
      {accountId && <StaffDetailDialog accountId={accountId} staff={detail} onClose={() => setDetail(null)} />}
    </>
  );
}

export default ManageStaff;
