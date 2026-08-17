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

import { useContext, useEffect, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import Icon from '@mui/material/Icon';
import Grid from '@mui/material/Grid';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import FormControlLabel from '@mui/material/FormControlLabel';
import Switch from '@mui/material/Switch';
import Table from 'controls/Tables/Table';
import ServerPagination from 'controls/Tables/ServerPagination';
import useServerList, { useClampPage } from 'controls/Tables/useServerList';
import TableAccordion from 'controls/Accordions/TableAccordion';
import ArgonBadge from 'components/ArgonBadge';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import ArgonBox from 'components/ArgonBox';
import { getAccountByUser } from 'api/manager/accounts';
import { getSynchronizedDevices, setSynchronizedDeviceIgnored, deleteDevice, registerManualDevice } from 'api/manager/devices';
import type { SynchronizedDevice } from 'api/manager/devices';
import type { DetectedStatus } from 'api/manager/generated/graphql';
import { notifyApiError } from 'api/core/errors';
import { useGpsOperators } from 'queries/operators';
import { useProviderCapabilities } from 'queries/router';
import useForm from 'controls/Dialogs/useForm';
import DeviceFormDialog from 'layouts/gpsintegration/components/devices/DeviceDialog';
import type { ManualDeviceFormValues } from 'layouts/gpsintegration/components/devices/DeviceDialog';
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from 'utils/dateUtils';
import { GPS_INTEGRATION_REFRESH_EVENT } from 'layouts/gpsintegration/gpsIntegrationEvents';

const PAGE_SIZE = 10;

function TextCell({ children }: { children?: ReactNode }) {
  return (
    <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
      {children || '-'}
    </ArgonTypography>
  );
}

type BadgeColor = 'primary' | 'secondary' | 'info' | 'success' | 'warning' | 'error' | 'light' | 'dark';

function statusColor(status: string): BadgeColor {
  switch ((status || '').toUpperCase()) {
    case 'NEW':
    case 'AVAILABLE': return 'warning';
    case 'ASSIGNED': return 'info';
    case 'IGNORED': return 'secondary';
    case 'REMOVED': return 'error';
    default: return 'secondary';
  }
}

function ManageSynchronizedDevices() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [devices, setDevices] = useState<SynchronizedDevice[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [accountId, setAccountId] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState('');
  const [operatorFilter, setOperatorFilter] = useState('');
  const [unassignedOnly, setUnassignedOnly] = useState(false);
  const [recentOnly, setRecentOnly] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const loaded = useRef(false);
  // The device list is one SERVER page: status, operator and free-text search
  // are query arguments, not post-filters over the loaded rows.
  const { page, setPage, searchDraft, setSearchDraft, params } = useServerList(PAGE_SIZE);
  useClampPage(page, PAGE_SIZE, totalCount, setPage);
  const operatorsQuery = useGpsOperators({ enabled: expanded });
  const operators = operatorsQuery.data ?? [];
  const capabilitiesQuery = useProviderCapabilities({ enabled: expanded });
  const [dialogOpen, setDialogOpen] = useState(false);
  const [deviceValues, handleDeviceChange, setDeviceValues, setDeviceErrors, validateDevice, deviceErrors] =
    useForm<ManualDeviceFormValues>({});

  // Manual registration is meant for providers whose API exposes no device catalog
  // (e.g. Prosegur): sync can never discover their devices. Offering it for a
  // catalog provider is a footgun — the next sync, which keys removal on the
  // provider's identifier, would flag the hand-entered device as removed.
  const catalogLessProtocolIds = new Set(
    (capabilitiesQuery.data ?? []).filter((p) => !p.deviceCatalog).map((p) => p.protocolTypeId)
  );
  const manualOperators = operators.filter((o) => catalogLessProtocolIds.has(o.protocolTypeId));
  const canAddManual = manualOperators.length > 0;

  const statusLabel = (status: string): string => {
    const key = (status || '').toLowerCase();
    return t(`gpsIntegration.status.${key}` as 'gpsIntegration.status.new', { defaultValue: status || '-' });
  };

  const refresh = async (acct: string | null = accountId) => {
    if (!acct) return;
    setLoading(true);
    try {
      const result = await getSynchronizedDevices(acct, {
        ...params,
        detectedStatus: (statusFilter || null) as DetectedStatus | null,
        operatorId: operatorFilter || null,
        unassignedOnly,
        recentOnly,
      });
      setDevices(result.items);
      setTotalCount(result.totalCount);
    } catch (e) {
      // Preserve the legacy toast-on-error behavior; keep the inline notice too.
      notifyApiError(e);
      setError(t('gpsIntegration.errors.devicesLoad'));
    } finally { setLoading(false); }
  };

  useEffect(() => {
    if (expanded && !loaded.current) {
      loaded.current = true;
      (async () => {
        try {
          const acct = await getAccountByUser();
          if (!acct?.accountId) {
            setError(t('gpsIntegration.errors.devicesLoad'));
            return;
          }
          setAccountId(acct.accountId);
        } catch {
          setError(t('gpsIntegration.errors.devicesLoad'));
        }
      })();
    }
  }, [expanded]);

  // Every filter and the page index are server arguments, so any change is a
  // refetch — never a narrowing of the rows already on screen.
  useEffect(() => {
    if (accountId) refresh(accountId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [accountId, params, statusFilter, operatorFilter, unassignedOnly, recentOnly]);

  useEffect(() => {
    const handleRefresh = () => {
      if (loaded.current) refresh();
    };
    window.addEventListener(GPS_INTEGRATION_REFRESH_EVENT, handleRefresh);
    return () => window.removeEventListener(GPS_INTEGRATION_REFRESH_EVENT, handleRefresh);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [accountId, params, statusFilter, operatorFilter, unassignedOnly, recentOnly]);

  const handleIgnore = async (device: SynchronizedDevice, ignore: boolean) => {
    setLoading(true);
    try {
      await setSynchronizedDeviceIgnored(device.deviceId, ignore);
      await refresh();
    } catch (e) {
      notifyApiError(e);
    } finally { setLoading(false); }
  };

  const handleDelete = async (device: SynchronizedDevice) => {
    if (!window.confirm(t('gpsIntegration.actions.deleteDeviceConfirm'))) return;
    setLoading(true);
    try {
      await deleteDevice(device.deviceId);
      await refresh();
    } catch (e) {
      notifyApiError(e);
    } finally { setLoading(false); }
  };

  const handleAddClick = () => {
    setDeviceValues({
      operatorId: manualOperators[0]?.operatorId ?? '',
      deviceTypeId: 4, // Cellular — the usual GPS tracker
      identifier: 0,
      autoAssign: true,
    });
    setDeviceErrors({});
  };

  const handleRegisterDevice = async () => {
    if (!accountId) return;
    if (!validateDevice(['operatorId', 'name', 'serial', 'deviceTypeId'])) return;
    setLoading(true);
    try {
      await registerManualDevice(
        {
          accountId,
          operatorId: deviceValues.operatorId!,
          name: (deviceValues.name ?? '').trim(),
          serial: (deviceValues.serial ?? '').trim(),
          deviceTypeId: Number(deviceValues.deviceTypeId),
          identifier: Number(deviceValues.identifier ?? 0),
          description: deviceValues.description?.trim() || null,
        },
        deviceValues.autoAssign !== false
      );
      setDialogOpen(false);
      await refresh();
    } catch (e) {
      // Keep the dialog open so the operator can correct a duplicate identifier etc.
      notifyApiError(e);
    } finally { setLoading(false); }
  };

  const operatorNames = operators.reduce<Record<string, string>>((acc, operator) => {
    acc[operator.operatorId] = operator.name;
    return acc;
  }, {});

  const rows = devices.map(d => ({
    name: <TextCell>{d.name || d.providerDisplayName}</TextCell>,
    identifier: <TextCell>{d.identifier}</TextCell>,
    serial: <TextCell>{d.serial}</TextCell>,
    status: (
      <ArgonBadge
        variant="gradient"
        badgeContent={statusLabel(d.detectedStatus)}
        color={statusColor(d.detectedStatus)}
        size="xs"
        container
      />
    ),
    firstSeen: <TextCell>{formatDateTime(d.firstSeenAt)}</TextCell>,
    lastSeen: <TextCell>{formatDateTime(d.lastSeenAt)}</TextCell>,
    actions: (
      <>
        <ArgonButton variant="text" color="dark" onClick={() => handleIgnore(d, (d.detectedStatus || '').toUpperCase() !== 'IGNORED')}>
          <Icon>{(d.detectedStatus || '').toUpperCase() === 'IGNORED' ? 'visibility' : 'visibility_off'}</Icon>
          &nbsp;{(d.detectedStatus || '').toUpperCase() === 'IGNORED' ? t('gpsIntegration.actions.unignore') : t('gpsIntegration.actions.ignore')}
        </ArgonButton>
        <ArgonButton variant="text" color="error" onClick={() => handleDelete(d)}>
          <Icon>delete</Icon>&nbsp;{t('generic.delete')}
        </ArgonButton>
      </>
    ),
    id: d.deviceId
  }));

  const statuses = ['AVAILABLE', 'ASSIGNED', 'IGNORED', 'REMOVED'];

  // The bulk actions apply to the devices actually on screen — one server page,
  // the only set the operator has reviewed. The buttons say "(this page)" so the
  // scope is explicit rather than implying the whole filtered result set.
  const bulkSetIgnored = async (ignored: boolean) => {
    if (!devices.length) return;
    setLoading(true);
    try {
      await Promise.all(devices.map(d => setSynchronizedDeviceIgnored(d.deviceId, ignored)));
      await refresh();
    } catch (e) {
      notifyApiError(e);
    } finally { setLoading(false); }
  };

  return (
    <>
    <TableAccordion
      title={t('gpsIntegration.sections.devices')}
      expanded={expanded}
      setExpanded={setExpanded}
      showAddIcon={canAddManual}
      setOpen={setDialogOpen}
      handleAddClick={handleAddClick}>
      {error
        ? <ArgonBox><ArgonTypography variant="button" color="error">{error}</ArgonTypography></ArgonBox>
        : <>
              {/* The filter bar stays mounted even on an empty result: the
                  filters are server arguments now, so hiding them would strand
                  the user on a search that matched nothing. */}
              <ArgonBox mb={1}>
                <Grid container spacing={1} sx={{ alignItems: "center" }}>
                  <Grid size={{ xs: 12, lg: 4 }}>
                    <TextField
                      fullWidth
                      size="small"
                      label={t('filters.search')}
                      value={searchDraft}
                      onChange={(e) => setSearchDraft(e.target.value)}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6, lg: 4 }}>
                    <TextField
                      fullWidth select size="small"
                      label={t('gpsIntegration.columns.detectedStatus')}
                      helperText={t('gpsIntegration.filters.statusHelp')}
                      value={statusFilter}
                      onChange={(e) => { setPage(0); setStatusFilter(e.target.value); }}
                    >
                      <MenuItem value="">--</MenuItem>
                      {statuses.map(s => <MenuItem key={s} value={s}>{statusLabel(s)}</MenuItem>)}
                    </TextField>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6, lg: 4 }}>
                    <TextField
                      fullWidth select size="small"
                      label={t('operator.title')}
                      helperText={t('gpsIntegration.filters.operatorHelp')}
                      value={operatorFilter}
                      onChange={(e) => { setPage(0); setOperatorFilter(e.target.value); }}
                    >
                      <MenuItem value="">--</MenuItem>
                      {operators.map(o => <MenuItem key={o.operatorId} value={o.operatorId}>{operatorNames[o.operatorId] || o.operatorId}</MenuItem>)}
                    </TextField>
                  </Grid>
                  <Grid size={{ xs: 12 }}>
                    <FormControlLabel
                      control={<Switch checked={unassignedOnly} onChange={(e) => { setPage(0); setUnassignedOnly(e.target.checked); }} />}
                      label={t('gpsIntegration.actions.showUnassignedOnly')}
                    />
                    <FormControlLabel
                      control={<Switch checked={recentOnly} onChange={(e) => { setPage(0); setRecentOnly(e.target.checked); }} />}
                      label={t('gpsIntegration.actions.showRecentlyAddedOnly')}
                    />
                  </Grid>
                  <Grid size={{ xs: 12 }}>
                    <ArgonButton variant="text" color="dark" onClick={() => bulkSetIgnored(true)}>
                      {t('gpsIntegration.actions.bulkIgnore')}
                    </ArgonButton>
                    <ArgonButton variant="text" color="dark" onClick={() => bulkSetIgnored(false)}>
                      {t('gpsIntegration.actions.bulkUnignore')}
                    </ArgonButton>
                    <ArgonTypography variant="caption" color="secondary" display="block">
                      {t('gpsIntegration.actions.bulkScopeHint')}
                    </ArgonTypography>
                  </Grid>
                </Grid>
              </ArgonBox>
              {devices.length === 0 && loaded.current && (
                <ArgonTypography variant="caption" color="secondary">
                  {t('gpsIntegration.empty.devices')}
                </ArgonTypography>
              )}
              <Table
                columns={[
                  { name: 'name', title: t('device.name'), align: 'left' },
                  { name: 'identifier', title: t('device.identifier'), align: 'left' },
                  { name: 'serial', title: t('device.serial'), align: 'left' },
                  { name: 'status', title: t('gpsIntegration.columns.detectedStatus'), align: 'center' },
                  { name: 'firstSeen', title: t('gpsIntegration.columns.firstSeen'), align: 'center' },
                  { name: 'lastSeen', title: t('gpsIntegration.columns.lastSeen'), align: 'center' },
                  { name: 'actions', title: t('generic.action'), align: 'center' },
                  { name: 'id' }
                ]}
                rows={rows}
                selectedField="name"
                serverPaged
              />
              <ServerPagination
                page={page}
                pageSize={PAGE_SIZE}
                totalCount={totalCount}
                pageLength={rows.length}
                onPageChange={setPage}
              />
            </>
      }
    </TableAccordion>
    <DeviceFormDialog
      open={dialogOpen}
      setOpen={setDialogOpen}
      handleSubmit={handleRegisterDevice}
      values={deviceValues}
      handleChange={handleDeviceChange}
      errors={deviceErrors}
      operators={manualOperators}
    />
    </>
  );
}

ManageSynchronizedDevices.propTypes = {};

export default ManageSynchronizedDevices;
