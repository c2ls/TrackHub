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
 * Staff credential and device administration (Security backend) for ONE staff, inside the
 * staff detail dialog. Core platform surface: never gated behind an account feature (spec 09
 * §3, AC8). Device rows show the server-masked push token only.
 */

import { useContext, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import Icon from '@mui/material/Icon';
import Tooltip from '@mui/material/Tooltip';
import Table from 'controls/Tables/Table';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import useForm from 'controls/Dialogs/useForm';
import ArgonBadge from 'components/ArgonBadge';
import ArgonBox from 'components/ArgonBox';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import StaffCredentialDialog from 'layouts/manageadmin/components/staff/StaffCredentialDialog';
import type {
  CredentialDialogMode,
  CredentialFormValues,
} from 'layouts/manageadmin/components/staff/StaffCredentialDialog';
import { TextCell, statusColor } from 'layouts/manageadmin/components/staff/workforceShared';
import type { BadgeColor } from 'layouts/manageadmin/components/staff/workforceShared';
import {
  credentialActions,
  credentialState,
} from 'layouts/manageadmin/components/staff/credentialLifecycle';
import {
  useStaffCredentials,
  useStaffDevices,
  useCreateStaffCredential,
  useActivateStaffCredential,
  useLockStaffCredential,
  useResetStaffCredential,
  useRevokeStaffCredential,
  useRevokeStaffDevice,
} from 'queries/staffIdentity';
import type { StaffCredential } from 'api/security/staffIdentity';
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from 'utils/dateUtils';

/** Who the portal records as the revoking principal on device revocation. */
const REVOKED_BY = 'portal';

interface PendingConfirm {
  open: boolean;
  kind: 'credential' | 'device';
  id: string | null;
}

interface StaffAccessPanelProps {
  accountId: string;
  staffId: string;
}

interface RowAction {
  key: string;
  icon: string;
  color: 'dark' | 'warning' | 'info' | 'success' | 'error';
  label: string;
  onClick: () => void;
}

function StaffAccessPanel({ accountId, staffId }: StaffAccessPanelProps) {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [mode, setMode] = useState<CredentialDialogMode>('create');
  const [targetCredentialId, setTargetCredentialId] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [confirm, setConfirm] = useState<PendingConfirm>({ open: false, kind: 'credential', id: null });
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<CredentialFormValues>({});

  const credentialsQuery = useStaffCredentials(accountId, staffId);
  const devicesQuery = useStaffDevices(accountId, staffId);
  const credentials = credentialsQuery.data ?? [];
  const devices = devicesQuery.data ?? [];

  const createCredential = useCreateStaffCredential();
  const activateCredential = useActivateStaffCredential();
  const lockCredential = useLockStaffCredential();
  const resetCredential = useResetStaffCredential();
  const revokeCredential = useRevokeStaffCredential();
  const revokeDevice = useRevokeStaffDevice();

  useEffect(() => {
    setLoading(credentialsQuery.isFetching || devicesQuery.isFetching);
  }, [credentialsQuery.isFetching, devicesQuery.isFetching, setLoading]);

  const openDialog = (next: CredentialDialogMode, credentialId: string | null) => {
    setMode(next);
    setTargetCredentialId(credentialId);
    setValues(next === 'create' ? { active: true, resetRequired: false } : {});
    setErrors({});
    setDialogOpen(true);
  };

  const requiredFor = (next: CredentialDialogMode): string[] => {
    if (next === 'create') return ['login', 'password'];
    if (next === 'lock') return ['lockedUntil'];
    return ['password'];
  };

  const submit = async () => {
    if (!validate(requiredFor(mode))) return;
    setLoading(true);
    try {
      if (mode === 'create') {
        await createCredential.mutateAsync({
          staffId,
          accountId,
          login: values.login as string,
          password: values.password as string,
          active: values.active !== false,
          resetRequired: !!values.resetRequired,
        });
      } else if (targetCredentialId) {
        if (mode === 'activate') {
          await activateCredential.mutateAsync({
            staffCredentialId: targetCredentialId,
            password: values.password as string,
          });
        } else if (mode === 'lock') {
          await lockCredential.mutateAsync({
            staffCredentialId: targetCredentialId,
            lockedUntil: new Date(values.lockedUntil as string).toISOString(),
          });
        } else {
          await resetCredential.mutateAsync({
            staffCredentialId: targetCredentialId,
            password: values.password as string,
            resetRequired: !!values.resetRequired,
          });
        }
      }
      setDialogOpen(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open.
    } finally {
      setLoading(false);
    }
  };

  const doConfirm = async () => {
    const { kind, id } = confirm;
    setConfirm({ open: false, kind, id: null });
    if (!id) return;
    setLoading(true);
    try {
      if (kind === 'credential') await revokeCredential.mutateAsync(id);
      else await revokeDevice.mutateAsync({ staffDeviceRegistrationId: id, revokedBy: REVOKED_BY });
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const stateBadge = (credential: StaffCredential): { label: string; color: BadgeColor } => {
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

  const rowActions = (credential: StaffCredential): RowAction[] => {
    const actions = credentialActions(credential);
    const id = credential.staffCredentialId;
    const list: RowAction[] = [];
    if (actions.activate) list.push({ key: 'activate', icon: 'check_circle', color: 'dark', label: t('workforce.credentials.activate'), onClick: () => openDialog('activate', id) });
    if (actions.lock) list.push({ key: 'lock', icon: 'lock', color: 'warning', label: t('workforce.credentials.lock'), onClick: () => openDialog('lock', id) });
    if (actions.reset) list.push({ key: 'reset', icon: 'lock_reset', color: 'info', label: t('workforce.credentials.reset'), onClick: () => openDialog('reset', id) });
    if (actions.revoke) list.push({ key: 'revoke', icon: 'block', color: 'error', label: t('workforce.credentials.revoke'), onClick: () => setConfirm({ open: true, kind: 'credential', id }) });
    return list;
  };

  const actionButtons = (actions: RowAction[]) => (
    <ArgonBox display="flex" justifyContent="center" gap={0.5}>
      {actions.map((action) => (
        <Tooltip key={action.key} title={action.label}>
          <ArgonButton variant="text" color={action.color} size="small" iconOnly onClick={action.onClick}>
            <Icon>{action.icon}</Icon>
          </ArgonButton>
        </Tooltip>
      ))}
    </ArgonBox>
  );

  return (
    <>
      <ArgonBox mb={1} display="flex" alignItems="center" justifyContent="space-between" flexWrap="wrap" gap={1}>
        <ArgonTypography variant="button" fontWeight="bold" textTransform="uppercase" color="text">
          {t('workforce.credentials.title')}
        </ArgonTypography>
        <ArgonButton color="info" size="small" onClick={() => openDialog('create', null)}>
          <Icon>add</Icon>&nbsp;{t('workforce.credentials.create')}
        </ArgonButton>
      </ArgonBox>

      {credentials.length === 0 ? (
        <ArgonTypography variant="caption" color="secondary">
          {t('workforce.credentials.empty')}
        </ArgonTypography>
      ) : (
        <Table
          columns={[
            { name: 'login', title: t('workforce.credentials.login'), align: 'left' },
            { name: 'status', title: t('workforce.credentials.status'), align: 'center' },
            { name: 'lastLoginAt', title: t('workforce.credentials.lastLoginAt'), align: 'center' },
            { name: 'lockedUntil', title: t('workforce.credentials.lockedUntil'), align: 'center' },
            { name: 'action', title: t('generic.action'), align: 'center' },
            { name: 'id' },
          ]}
          rows={credentials.map((credential) => {
            const state = stateBadge(credential);
            return {
              login: (
                <ArgonTypography variant="caption" fontWeight="medium">
                  {credential.normalizedLogin}
                </ArgonTypography>
              ),
              status: <ArgonBadge badgeContent={state.label} color={state.color} size="xs" container />,
              lastLoginAt: <TextCell>{formatDateTime(credential.lastLoginAt)}</TextCell>,
              lockedUntil: <TextCell>{formatDateTime(credential.lockedUntil)}</TextCell>,
              action: actionButtons(rowActions(credential)),
              id: credential.staffCredentialId,
            };
          })}
          selectedField="login"
        />
      )}

      <ArgonBox mt={3} mb={1}>
        <ArgonTypography variant="button" fontWeight="bold" textTransform="uppercase" color="text">
          {t('workforce.devices.title')}
        </ArgonTypography>
      </ArgonBox>
      {devices.length === 0 ? (
        <ArgonTypography variant="caption" color="secondary">
          {t('workforce.devices.empty')}
        </ArgonTypography>
      ) : (
        <Table
          columns={[
            { name: 'deviceName', title: t('workforce.devices.deviceName'), align: 'left' },
            { name: 'platform', title: t('workforce.devices.platform'), align: 'center' },
            { name: 'appVersion', title: t('workforce.devices.appVersion'), align: 'center' },
            { name: 'lastSeenAt', title: t('workforce.devices.lastSeenAt'), align: 'center' },
            { name: 'status', title: t('workforce.credentials.status'), align: 'center' },
            { name: 'action', title: t('generic.action'), align: 'center' },
            { name: 'id' },
          ]}
          rows={devices.map((device) => ({
            deviceName: (
              <ArgonTypography variant="caption" fontWeight="medium">
                {device.deviceName || device.deviceId}
              </ArgonTypography>
            ),
            platform: <TextCell>{device.platform}</TextCell>,
            appVersion: <TextCell>{device.appVersion}</TextCell>,
            lastSeenAt: <TextCell>{formatDateTime(device.lastSeenAt)}</TextCell>,
            status: (
              <ArgonBadge
                badgeContent={device.active ? t('workforce.credentials.statusActive') : t('workforce.credentials.statusRevoked')}
                color={device.active ? 'success' : 'error'}
                size="xs"
                container
              />
            ),
            action: device.active
              ? actionButtons([{ key: 'revoke', icon: 'phonelink_erase', color: 'error', label: t('staff.revokeDeviceRegistration'), onClick: () => setConfirm({ open: true, kind: 'device', id: device.staffDeviceRegistrationId }) }])
              : null,
            id: device.staffDeviceRegistrationId,
          }))}
          selectedField="deviceName"
        />
      )}

      <StaffCredentialDialog mode={mode} open={dialogOpen} setOpen={setDialogOpen} handleSubmit={submit} values={values} handleChange={handleChange} errors={errors} />
      <ConfirmDialog
        open={confirm.open}
        setOpen={(next) => setConfirm((prev) => ({ ...prev, open: typeof next === 'function' ? next(prev.open) : next }))}
        title={confirm.kind === 'credential' ? t('workforce.credentials.revokeTitle') : t('staff.revokeDeviceRegistration')}
        message={confirm.kind === 'credential' ? t('workforce.credentials.revokeConfirm') : t('workforce.devices.revokeConfirm')}
        onConfirm={doConfirm}
      />
    </>
  );
}

export default StaffAccessPanel;
