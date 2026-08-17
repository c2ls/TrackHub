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
 * Driver credential and device administration (Security backend).
 *
 * This is CORE platform surface — driver identity administration is never gated
 * behind an account feature (spec 09 §3, §8 "Visibility", AC8). Device rows show
 * the server-masked push token only; the refresh-token family id is not part of
 * the VM at all.
 */

import { useContext, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import Icon from '@mui/material/Icon';
import Grid from '@mui/material/Grid';
import Table from 'controls/Tables/Table';
import TableAccordion from 'controls/Accordions/TableAccordion';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import useForm from 'controls/Dialogs/useForm';
import ArgonBadge from 'components/ArgonBadge';
import ArgonBox from 'components/ArgonBox';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import DriverCredentialDialog from 'layouts/manageadmin/components/drivers/DriverCredentialDialog';
import type {
  CredentialDialogMode,
  CredentialFormValues,
} from 'layouts/manageadmin/components/drivers/DriverCredentialDialog';
import { DriverPicker, TextCell, statusColor } from 'layouts/manageadmin/components/drivers/workforceShared';
import type { BadgeColor } from 'layouts/manageadmin/components/drivers/workforceShared';
import {
  credentialActions,
  credentialState,
} from 'layouts/manageadmin/components/drivers/credentialLifecycle';
import { useAccountByUser } from 'queries/accounts';
import { useDriversByAccount } from 'queries/drivers';
import {
  useDriverCredentials,
  useDriverDevices,
  useCreateDriverCredential,
  useActivateDriverCredential,
  useLockDriverCredential,
  useResetDriverCredential,
  useRevokeDriverCredential,
  useRevokeDriverDevice,
} from 'queries/driverIdentity';
import type { DriverCredential } from 'api/security/driverIdentity';
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from 'utils/dateUtils';

/** Who the portal records as the revoking principal on device revocation. */
const REVOKED_BY = 'portal';

interface PendingConfirm {
  open: boolean;
  kind: 'credential' | 'device';
  id: string | null;
}

function ManageDriverCredentials() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [driverId, setDriverId] = useState('');
  const [mode, setMode] = useState<CredentialDialogMode>('create');
  const [targetCredentialId, setTargetCredentialId] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [confirm, setConfirm] = useState<PendingConfirm>({ open: false, kind: 'credential', id: null });
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<CredentialFormValues>({});

  const accountQuery = useAccountByUser({ enabled: expanded });
  const accountId = accountQuery.data?.accountId;
  const driversQuery = useDriversByAccount(accountId, { enabled: expanded && !!accountId });
  const drivers = driversQuery.data ?? [];

  const hasDriver = !!driverId;
  const credentialsQuery = useDriverCredentials(accountId, driverId || null, {
    enabled: expanded && hasDriver,
  });
  const devicesQuery = useDriverDevices(accountId, driverId || null, {
    enabled: expanded && hasDriver,
  });
  const credentials = credentialsQuery.data ?? [];
  const devices = devicesQuery.data ?? [];

  const createCredential = useCreateDriverCredential();
  const activateCredential = useActivateDriverCredential();
  const lockCredential = useLockDriverCredential();
  const resetCredential = useResetDriverCredential();
  const revokeCredential = useRevokeDriverCredential();
  const revokeDevice = useRevokeDriverDevice();

  useEffect(() => {
    setLoading(credentialsQuery.isFetching || devicesQuery.isFetching || driversQuery.isFetching);
  }, [credentialsQuery.isFetching, devicesQuery.isFetching, driversQuery.isFetching, setLoading]);

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
    if (!validate(requiredFor(mode)) || !accountId) return;
    setLoading(true);
    try {
      if (mode === 'create') {
        // validate() above guarantees login/password are present.
        await createCredential.mutateAsync({
          driverId,
          accountId,
          login: values.login as string,
          password: values.password as string,
          active: values.active !== false,
          resetRequired: !!values.resetRequired,
        });
      } else if (targetCredentialId) {
        if (mode === 'activate') {
          await activateCredential.mutateAsync({
            driverCredentialId: targetCredentialId,
            password: values.password as string,
          });
        } else if (mode === 'lock') {
          await lockCredential.mutateAsync({
            driverCredentialId: targetCredentialId,
            lockedUntil: new Date(values.lockedUntil as string).toISOString(),
          });
        } else {
          await resetCredential.mutateAsync({
            driverCredentialId: targetCredentialId,
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
      else await revokeDevice.mutateAsync({ driverDeviceRegistrationId: id, revokedBy: REVOKED_BY });
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  /** Presentation for each state resolved by the (pure) lifecycle module. */
  const stateBadge = (credential: DriverCredential): { label: string; color: BadgeColor } => {
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

  return (
    <>
      <TableAccordion
        title={t('workforce.credentials.title')}
        expanded={expanded}
        setExpanded={setExpanded}
      >
        <ArgonBox mb={2}>
          <Grid container spacing={1} sx={{ alignItems: "center" }}>
            <Grid size={{ xs: 12, md: 6 }}>
              <DriverPicker
                id="credentialDriverId"
                drivers={drivers}
                value={driverId}
                onChange={setDriverId}
                label={t('workforce.selectDriver')}
                placeholder={t('workforce.selectDriverPlaceholder')}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <ArgonButton
                color="info"
                disabled={!hasDriver}
                onClick={() => openDialog('create', null)}
              >
                <Icon>add</Icon>&nbsp;{t('workforce.credentials.create')}
              </ArgonButton>
            </Grid>
          </Grid>
        </ArgonBox>

        {!hasDriver ? (
          <ArgonTypography variant="caption" color="secondary">
            {t('workforce.selectDriverHint')}
          </ArgonTypography>
        ) : (
          <>
            {credentials.length === 0 ? (
              <ArgonTypography variant="caption" color="secondary">
                {t('workforce.credentials.empty')}
              </ArgonTypography>
            ) : (
              <Table
                columns={[
                  { name: 'login', title: t('workforce.credentials.login'), align: 'left' },
                  { name: 'status', title: t('workforce.credentials.status'), align: 'center' },
                  { name: 'failedAttempts', title: t('workforce.credentials.failedAttempts'), align: 'center' },
                  { name: 'lockedUntil', title: t('workforce.credentials.lockedUntil'), align: 'center' },
                  { name: 'lastLoginAt', title: t('workforce.credentials.lastLoginAt'), align: 'center' },
                  { name: 'resetRequired', title: t('workforce.credentials.resetRequired'), align: 'center' },
                  { name: 'action', title: t('generic.action'), align: 'center' },
                  { name: 'id' },
                ]}
                rows={credentials.map((credential) => {
                  const state = stateBadge(credential);
                  const actions = credentialActions(credential);
                  return {
                    login: (
                      <ArgonTypography variant="caption" fontWeight="medium">
                        {credential.normalizedLogin}
                      </ArgonTypography>
                    ),
                    status: (
                      <ArgonBadge badgeContent={state.label} color={state.color} size="xs" container />
                    ),
                    failedAttempts: <TextCell>{credential.failedAttempts}</TextCell>,
                    lockedUntil: <TextCell>{formatDateTime(credential.lockedUntil)}</TextCell>,
                    lastLoginAt: <TextCell>{formatDateTime(credential.lastLoginAt)}</TextCell>,
                    resetRequired: (
                      <TextCell>{credential.resetRequired ? t('generic.yes') : t('generic.no')}</TextCell>
                    ),
                    action: (
                      <>
                        {actions.activate && (
                          <ArgonButton
                            variant="text"
                            color="dark"
                            onClick={() => openDialog('activate', credential.driverCredentialId)}
                          >
                            <Icon>check_circle</Icon>&nbsp;{t('workforce.credentials.activate')}
                          </ArgonButton>
                        )}
                        {actions.lock && (
                          <ArgonButton
                            variant="text"
                            color="warning"
                            onClick={() => openDialog('lock', credential.driverCredentialId)}
                          >
                            <Icon>lock</Icon>&nbsp;{t('workforce.credentials.lock')}
                          </ArgonButton>
                        )}
                        {actions.reset && (
                          <ArgonButton
                            variant="text"
                            color="info"
                            onClick={() => openDialog('reset', credential.driverCredentialId)}
                          >
                            <Icon>lock_reset</Icon>&nbsp;{t('workforce.credentials.reset')}
                          </ArgonButton>
                        )}
                        {actions.revoke && (
                          <ArgonButton
                            variant="text"
                            color="error"
                            onClick={() =>
                              setConfirm({
                                open: true,
                                kind: 'credential',
                                id: credential.driverCredentialId,
                              })
                            }
                          >
                            <Icon>block</Icon>&nbsp;{t('workforce.credentials.revoke')}
                          </ArgonButton>
                        )}
                      </>
                    ),
                    id: credential.driverCredentialId,
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
                  { name: 'pushToken', title: t('workforce.devices.pushToken'), align: 'center' },
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
                  pushToken: <TextCell>{device.pushToken}</TextCell>,
                  lastSeenAt: <TextCell>{formatDateTime(device.lastSeenAt)}</TextCell>,
                  status: (
                    <ArgonBadge
                      badgeContent={
                        device.active
                          ? t('workforce.credentials.statusActive')
                          : t('workforce.credentials.statusRevoked')
                      }
                      color={device.active ? 'success' : 'error'}
                      size="xs"
                      container
                    />
                  ),
                  action: device.active ? (
                    <ArgonButton
                      variant="text"
                      color="error"
                      onClick={() =>
                        setConfirm({
                          open: true,
                          kind: 'device',
                          id: device.driverDeviceRegistrationId,
                        })
                      }
                    >
                      {/* Wires the long-dangling driver.revokeDeviceRegistration key. */}
                      <Icon>phonelink_erase</Icon>&nbsp;{t('driver.revokeDeviceRegistration')}
                    </ArgonButton>
                  ) : null,
                  id: device.driverDeviceRegistrationId,
                }))}
                selectedField="deviceName"
              />
            )}
          </>
        )}
      </TableAccordion>

      <DriverCredentialDialog
        mode={mode}
        open={dialogOpen}
        setOpen={setDialogOpen}
        handleSubmit={submit}
        values={values}
        handleChange={handleChange}
        errors={errors}
      />
      <ConfirmDialog
        open={confirm.open}
        setOpen={(next) =>
          setConfirm((prev) => ({
            ...prev,
            open: typeof next === 'function' ? next(prev.open) : next,
          }))
        }
        title={
          confirm.kind === 'credential'
            ? t('workforce.credentials.revokeTitle')
            : t('driver.revokeDeviceRegistration')
        }
        message={
          confirm.kind === 'credential'
            ? t('workforce.credentials.revokeConfirm')
            : t('workforce.devices.revokeConfirm')
        }
        onConfirm={doConfirm}
      />
    </>
  );
}

export default ManageDriverCredentials;
