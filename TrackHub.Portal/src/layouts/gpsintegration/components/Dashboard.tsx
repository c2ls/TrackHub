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
import { useTranslation } from 'react-i18next';
import { useQueryClient } from '@tanstack/react-query';
import Grid from '@mui/material/Grid';
import Card from '@mui/material/Card';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import SummaryCard from 'layouts/gpsintegration/components/dashboard/SummaryCard';
import ProviderStatusBreakdown from 'layouts/gpsintegration/components/dashboard/ProviderStatusBreakdown';
import { getAccountByUser } from 'api/manager/accounts';
import { useGpsDashboard, gpsDashboardKeys } from 'queries/gpsDashboard';
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from 'utils/dateUtils';
import { GPS_INTEGRATION_REFRESH_EVENT } from 'layouts/gpsintegration/gpsIntegrationEvents';

function GpsDashboard() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const queryClient = useQueryClient();
  const [accountId, setAccountId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const loaded = useRef(false);

  const dashboardQuery = useGpsDashboard(accountId ?? undefined);
  const dashboard = dashboardQuery.data ?? null;

  // Keep the global spinner UX for the dashboard load / invalidation refetch.
  useEffect(() => {
    setLoading(dashboardQuery.isFetching);
  }, [dashboardQuery.isFetching, setLoading]);

  useEffect(() => {
    if (loaded.current) return;
    loaded.current = true;
    (async () => {
      try {
        const acct = await getAccountByUser();
        if (!acct?.accountId) {
          setError(t('gpsIntegration.errors.dashboardLoad'));
          return;
        }
        setAccountId(acct.accountId);
      } catch {
        setError(t('gpsIntegration.errors.dashboardLoad'));
      }
    })();
  }, [t]);

  // The refresh bus (operator toggle/sync/credential save) now drives a cache
  // invalidation; the query refetches. A read failure is surfaced by the toast.
  useEffect(() => {
    const handleRefresh = () => queryClient.invalidateQueries({ queryKey: gpsDashboardKeys.all });
    window.addEventListener(GPS_INTEGRATION_REFRESH_EVENT, handleRefresh);
    return () => window.removeEventListener(GPS_INTEGRATION_REFRESH_EVENT, handleRefresh);
  }, [queryClient]);

  const errorMessage = error || (dashboardQuery.isError ? t('gpsIntegration.errors.dashboardLoad') : null);
  if (errorMessage) {
    return (
      <Card>
        <ArgonBox p={3}>
          <ArgonTypography variant="button" color="error">{errorMessage}</ArgonTypography>
        </ArgonBox>
      </Card>
    );
  }
  if (!dashboard) {
    return (
      <Card>
        <ArgonBox p={3}>
          <ArgonTypography variant="caption" color="secondary">{t('gpsIntegration.empty.dashboard')}</ArgonTypography>
        </ArgonBox>
      </Card>
    );
  }

  return (
    <ArgonBox>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
          <SummaryCard
            title={t('gpsIntegration.dashboard.operators')}
            primary={`${dashboard.operatorsEnabled}/${dashboard.operatorsTotal}`}
            primaryLabel={t('gpsIntegration.dashboard.enabledOfTotal')}
            rows={[
              { label: t('gpsIntegration.dashboard.operatorsHealthy'), value: dashboard.operatorsHealthy, color: 'success' },
              { label: t('gpsIntegration.dashboard.operatorsDegraded'), value: dashboard.operatorsDegraded, color: 'warning' },
              { label: t('gpsIntegration.dashboard.operatorsOffline'), value: dashboard.operatorsOffline, color: 'error' },
            ]}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
          <SummaryCard
            title={t('gpsIntegration.dashboard.devices')}
            primary={dashboard.devicesTotal}
            primaryLabel={t('generic.total')}
            rows={[
              { label: t('gpsIntegration.status.assigned'), value: dashboard.devicesAssigned },
              { label: t('gpsIntegration.status.available'), value: dashboard.unassignedDevicesCount, color: 'warning' },
              { label: t('gpsIntegration.status.ignored'), value: dashboard.devicesIgnored },
              { label: t('gpsIntegration.dashboard.recentlyAdded24h'), value: dashboard.recentlyAddedDevicesLast24h },
            ]}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 12, md: 4 }}>
          <SummaryCard
            title={t('gpsIntegration.dashboard.sync')}
            primary={`${dashboard.syncRunsSucceededLast24h}/${dashboard.syncRunsFailedLast24h}`}
            primaryLabel={t('gpsIntegration.dashboard.okFailed24h')}
            rows={[
              { label: t('gpsIntegration.dashboard.averageSyncDuration'),
                value: dashboard.averageSyncDurationSeconds == null ? '-' : `${Math.round(dashboard.averageSyncDurationSeconds)} s` },
              { label: t('gpsIntegration.dashboard.lastAutoSync'), value: formatDateTime(dashboard.lastAutomaticSyncAt) },
              { label: t('gpsIntegration.dashboard.lastManualSync'), value: formatDateTime(dashboard.lastManualSyncAt) },
            ]}
          />
        </Grid>
      </Grid>
      <ArgonBox mt={2}>
        <ProviderStatusBreakdown items={dashboard.deviceCountsByProviderStatus} />
      </ArgonBox>
    </ArgonBox>
  );
}

export default GpsDashboard;
