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
import { useQueryClient } from '@tanstack/react-query';
import Table from 'controls/Tables/Table';
import TableAccordion from 'controls/Accordions/TableAccordion';
import ArgonBadge from 'components/ArgonBadge';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import { getAccountByUser } from 'api/manager/accounts';
import { useGpsOperators, useOperatorSyncRuns, operatorTelemetryKeys } from 'queries/operators';
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

function resultColor(result: string): BadgeColor {
  switch ((result || '').toUpperCase()) {
    case 'SUCCEEDED': return 'success';
    case 'PARTIAL': return 'warning';
    case 'FAILED': return 'error';
    case 'RUNNING':
    case 'PENDING': return 'info';
    default: return 'secondary';
  }
}

function RecentSyncRuns() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const queryClient = useQueryClient();
  const [expanded, setExpanded] = useState(false);
  const [accountId, setAccountId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const accountRequested = useRef(false);

  // Reads run only once the accordion is expanded. The sync-run query also
  // waits for the account id (resolved imperatively below).
  const operatorsQuery = useGpsOperators({ enabled: expanded });
  const syncRunsQuery = useOperatorSyncRuns(accountId, null, 20, { enabled: expanded });
  const operators = operatorsQuery.data ?? [];
  const runs = syncRunsQuery.data ?? [];

  useEffect(() => {
    setLoading(operatorsQuery.isFetching || syncRunsQuery.isFetching);
  }, [operatorsQuery.isFetching, syncRunsQuery.isFetching, setLoading]);

  useEffect(() => {
    if (!expanded || accountRequested.current) return;
    accountRequested.current = true;
    (async () => {
      try {
        const acct = await getAccountByUser();
        if (!acct?.accountId) {
          setError(t('gpsIntegration.errors.syncRunsLoad'));
          return;
        }
        setAccountId(acct.accountId);
      } catch {
        setError(t('gpsIntegration.errors.syncRunsLoad'));
      }
    })();
  }, [expanded, t]);

  // The refresh bus now drives a cache invalidation; the sync-run query
  // refetches when it is active (accordion expanded). A read failure is
  // surfaced by the global toast.
  useEffect(() => {
    const handleRefresh = () =>
      queryClient.invalidateQueries({ queryKey: operatorTelemetryKeys.all });
    window.addEventListener(GPS_INTEGRATION_REFRESH_EVENT, handleRefresh);
    return () => window.removeEventListener(GPS_INTEGRATION_REFRESH_EVENT, handleRefresh);
  }, [queryClient]);

  const operatorNames = operators.reduce<Record<string, string>>((acc, operator) => {
    acc[operator.operatorId] = operator.name;
    return acc;
  }, {});

  const rows = runs.map(r => ({
    // NOTE: the sync-run type carries no `operatorName`; the old `|| r.operatorName`
    // fallback was always undefined (dead) and is dropped — render-identical.
    operator: <TextCell>{operatorNames[r.operatorId] || r.operatorId}</TextCell>,
    trigger: <TextCell>{r.triggerType}</TextCell>,
    result: (
      <ArgonBadge variant="gradient" badgeContent={t(`gpsIntegration.result.${(r.result || '').toLowerCase()}` as 'gpsIntegration.result.succeeded', { defaultValue: r.result })} color={resultColor(r.result)} size="xs" container />
    ),
    startedAt: <TextCell>{formatDateTime(r.startedAt)}</TextCell>,
    completedAt: <TextCell>{formatDateTime(r.completedAt)}</TextCell>,
    devices: <TextCell>{`+${r.devicesAdded ?? 0} / ~${r.devicesUpdated ?? 0} / -${r.devicesRemoved ?? 0}`}</TextCell>,
    positions: <TextCell>{`${r.positionsAccepted ?? 0}/${r.positionsRead ?? 0}`}</TextCell>,
    error: <TextCell>{r.errorCode}</TextCell>,
    id: r.operatorSyncRunId
  }));

  return (
    <TableAccordion title={t('gpsIntegration.sections.recentSyncRuns')} expanded={expanded} setExpanded={setExpanded}>
      {error
        ? <ArgonBox><ArgonTypography variant="button" color="error">{error}</ArgonTypography></ArgonBox>
        : runs.length === 0 && syncRunsQuery.isFetched
          ? <ArgonTypography variant="caption" color="secondary">{t('gpsIntegration.empty.syncRuns')}</ArgonTypography>
          : <Table
              columns={[
                { name: 'operator', title: t('operator.title'), align: 'left' },
                { name: 'trigger', title: t('gpsIntegration.columns.trigger'), align: 'center' },
                { name: 'result', title: t('gpsIntegration.columns.result'), align: 'center' },
                { name: 'startedAt', title: t('gpsIntegration.columns.startedAt'), align: 'center' },
                { name: 'completedAt', title: t('gpsIntegration.columns.completedAt'), align: 'center' },
                { name: 'devices', title: t('gpsIntegration.columns.devicesDelta'), align: 'center' },
                { name: 'positions', title: t('gpsIntegration.columns.positions'), align: 'center' },
                { name: 'error', title: t('gpsIntegration.columns.errorCode'), align: 'center' },
                { name: 'id' }
              ]}
              rows={rows}
              selectedField="operator"
            />
      }
    </TableAccordion>
  );
}

export default RecentSyncRuns;
