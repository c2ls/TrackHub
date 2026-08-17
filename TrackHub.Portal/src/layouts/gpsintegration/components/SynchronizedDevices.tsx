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
import TableAccordion from 'controls/Accordions/TableAccordion';
import ArgonBadge from 'components/ArgonBadge';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import ArgonBox from 'components/ArgonBox';
import { getAccountByUser } from 'api/manager/accounts';
import { getSynchronizedDevices, setSynchronizedDeviceIgnored, deleteDevice } from 'api/manager/devices';
import type { SynchronizedDevice } from 'api/manager/devices';
import { notifyApiError } from 'api/core/errors';
import { useGpsOperators } from 'queries/operators';
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from 'utils/dateUtils';
import { GPS_INTEGRATION_REFRESH_EVENT } from 'layouts/gpsintegration/gpsIntegrationEvents';

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
  const [accountId, setAccountId] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState('');
  const [operatorFilter, setOperatorFilter] = useState('');
  const [search, setSearch] = useState('');
  const [unassignedOnly, setUnassignedOnly] = useState(false);
  const [recentOnly, setRecentOnly] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const loaded = useRef(false);
  // Operator name map / filter options come from the query layer; the device
  // list stays on the legacy device service (migrated in a later batch).
  const operatorsQuery = useGpsOperators({ enabled: expanded });
  const operators = operatorsQuery.data ?? [];

  const statusLabel = (status: string): string => {
    const key = (status || '').toLowerCase();
    return t(`gpsIntegration.status.${key}` as 'gpsIntegration.status.new', { defaultValue: status || '-' });
  };

  const refresh = async (acct: string | null = accountId) => {
    if (!acct) return;
    setLoading(true);
    try {
      const result = await getSynchronizedDevices(acct);
      setDevices(result);
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
          await refresh(acct.accountId);
        } catch {
          setError(t('gpsIntegration.errors.devicesLoad'));
        }
      })();
    }
  }, [expanded]);

  useEffect(() => {
    const handleRefresh = () => {
      if (loaded.current) refresh();
    };
    window.addEventListener(GPS_INTEGRATION_REFRESH_EVENT, handleRefresh);
    return () => window.removeEventListener(GPS_INTEGRATION_REFRESH_EVENT, handleRefresh);
  }, [accountId]);

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
  const filterOperators = Array.from(new Set(devices.map(d => d.operatorId).filter(Boolean)));

  const filteredRows = rows.filter(r => {
    const raw = devices.find(d => d.deviceId === r.id);
    if (!raw) return false;
    if (statusFilter && (raw.detectedStatus || '').toUpperCase() !== statusFilter) return false;
    if (operatorFilter && raw.operatorId !== operatorFilter) return false;
    if (unassignedOnly && (raw.detectedStatus || '').toUpperCase() === 'ASSIGNED') return false;
    if (recentOnly) {
      const ts = raw.firstSeenAt ? new Date(raw.firstSeenAt).getTime() : 0;
      if (!ts || (Date.now() - ts) > 24 * 60 * 60 * 1000) return false;
    }
    if (search) {
      const value = `${raw.identifier || ''} ${raw.serial || ''} ${raw.providerDisplayName || ''} ${raw.name || ''}`.toLowerCase();
      if (!value.includes(search.toLowerCase())) return false;
    }
    return true;
  });

  const bulkSetIgnored = async (ignored: boolean) => {
    if (!filteredRows.length) return;
    setLoading(true);
    try {
      await Promise.all(filteredRows.map(r => setSynchronizedDeviceIgnored(r.id, ignored)));
      await refresh();
    } catch (e) {
      notifyApiError(e);
    } finally { setLoading(false); }
  };

  return (
    <TableAccordion title={t('gpsIntegration.sections.devices')} expanded={expanded} setExpanded={setExpanded}>
      {error
        ? <ArgonBox><ArgonTypography variant="button" color="error">{error}</ArgonTypography></ArgonBox>
        : devices.length === 0 && loaded.current
          ? <ArgonTypography variant="caption" color="secondary">{t('gpsIntegration.empty.devices')}</ArgonTypography>
          : <>
              <ArgonBox mb={1}>
                <Grid container spacing={1} alignItems="center">
                  <Grid size={{ xs: 12, lg: 4 }}>
                    <TextField
                      fullWidth
                      size="small"
                      label={t('filters.search')}
                      value={search}
                      onChange={(e) => setSearch(e.target.value)}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6, lg: 4 }}>
                    <TextField
                      fullWidth select size="small"
                      label={t('gpsIntegration.columns.detectedStatus')}
                      helperText={t('gpsIntegration.filters.statusHelp')}
                      value={statusFilter}
                      onChange={(e) => setStatusFilter(e.target.value)}
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
                      onChange={(e) => setOperatorFilter(e.target.value)}
                    >
                      <MenuItem value="">--</MenuItem>
                      {filterOperators.map(s => <MenuItem key={s} value={s}>{operatorNames[s] || s}</MenuItem>)}
                    </TextField>
                  </Grid>
                  <Grid size={{ xs: 12 }}>
                    <ArgonButton variant="text" color="dark" onClick={() => bulkSetIgnored(true)}>
                      {t('gpsIntegration.actions.bulkIgnore')}
                    </ArgonButton>
                    <ArgonButton variant="text" color="dark" onClick={() => bulkSetIgnored(false)}>
                      {t('gpsIntegration.actions.bulkUnignore')}
                    </ArgonButton>
                  </Grid>
                  <Grid size={{ xs: 12 }}>
                    <FormControlLabel
                      control={<Switch checked={unassignedOnly} onChange={(e) => setUnassignedOnly(e.target.checked)} />}
                      label={t('gpsIntegration.actions.showUnassignedOnly')}
                    />
                    <FormControlLabel
                      control={<Switch checked={recentOnly} onChange={(e) => setRecentOnly(e.target.checked)} />}
                      label={t('gpsIntegration.actions.showRecentlyAddedOnly')}
                    />
                  </Grid>
                </Grid>
              </ArgonBox>
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
                rows={filteredRows}
                selectedField="name"
              />
            </>
      }
    </TableAccordion>
  );
}

ManageSynchronizedDevices.propTypes = {};

export default ManageSynchronizedDevices;
