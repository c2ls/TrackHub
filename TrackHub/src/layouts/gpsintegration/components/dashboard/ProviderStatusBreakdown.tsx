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

import { useState } from 'react';
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import Table from 'controls/Tables/Table';
import TableAccordion from 'controls/Accordions/TableAccordion';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import { pivotProviderStatus } from 'layouts/gpsintegration/data/providerStatusData';
import type { ProviderStatusItem } from 'layouts/gpsintegration/data/providerStatusData';

function CountCell({ value, color }: { value: number; color?: 'success' | 'warning' | 'error' }) {
  return (
    <ArgonTypography
      variant="caption"
      fontWeight={value > 0 ? 'bold' : 'regular'}
      color={value > 0 ? (color ?? 'text') : 'secondary'}
    >
      {value}
    </ArgonTypography>
  );
}

interface ProviderStatusBreakdownProps {
  items?: ProviderStatusItem[];
}

/**
 * One pivoted table row per operator (collapsed by default): unassigned /
 * assigned / ignored / removed counts plus a total, sorted by total so the
 * screen stays usable with many providers.
 */
function ProviderStatusBreakdown({ items }: ProviderStatusBreakdownProps) {
  const { t } = useTranslation();
  const [expanded, setExpanded] = useState(false);
  const rows = pivotProviderStatus(items);

  const tableRows: Record<string, ReactNode>[] = rows.map((row) => ({
    operator: (
      <ArgonTypography variant="caption" fontWeight="medium">
        {row.operatorName}
      </ArgonTypography>
    ),
    unassigned: <CountCell value={row.unassigned} color="warning" />,
    assigned: <CountCell value={row.assigned} />,
    ignored: <CountCell value={row.ignored} />,
    removed: <CountCell value={row.removed} color="error" />,
    total: <CountCell value={row.total} />,
    id: row.operatorId,
  }));

  return (
    <TableAccordion
      title={`${t('gpsIntegration.dashboard.deviceCountsByProviderStatus')} (${rows.length})`}
      expanded={expanded}
      setExpanded={setExpanded}
    >
      {rows.length === 0 ? (
        <ArgonTypography variant="caption" color="secondary">
          {t('gpsIntegration.empty.dashboard')}
        </ArgonTypography>
      ) : (
        <ArgonBox sx={{ maxHeight: 420, overflowY: 'auto' }}>
          <Table
            columns={[
              { name: 'operator', title: t('operator.singleTitle'), align: 'left' },
              { name: 'unassigned', title: t('gpsIntegration.status.available'), align: 'center' },
              { name: 'assigned', title: t('gpsIntegration.status.assigned'), align: 'center' },
              { name: 'ignored', title: t('gpsIntegration.status.ignored'), align: 'center' },
              { name: 'removed', title: t('gpsIntegration.status.removed'), align: 'center' },
              { name: 'total', title: t('generic.total'), align: 'center' },
              { name: 'id' },
            ]}
            rows={tableRows}
            selectedField="operator"
          />
        </ArgonBox>
      )}
    </TableAccordion>
  );
}

export default ProviderStatusBreakdown;
