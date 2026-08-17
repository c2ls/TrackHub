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
 * Account-wide qualification expirations inside the next 30 days, severity
 * coloured by remaining days (the same 30/15/7/0 thresholds the backend scan
 * alerts on). Gated by the `workforce` feature — cosmetically only.
 */

import { useContext, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import Table from 'controls/Tables/Table';
import TableAccordion from 'controls/Accordions/TableAccordion';
import ArgonBadge from 'components/ArgonBadge';
import ArgonTypography from 'components/ArgonTypography';
import {
  TextCell,
  daysUntil,
  expiryColor,
  statusColor,
} from 'layouts/manageadmin/components/drivers/workforceShared';
import {
  qualificationTypeLabel,
  qualificationStatusLabel,
} from 'layouts/manageadmin/components/drivers/qualificationConstants';
import { useAccountByUser } from 'queries/accounts';
import { useDriverQualifications } from 'queries/drivers';
import { LoadingContext } from 'LoadingContext';
import { formatDateOnly } from 'utils/dateUtils';

/** The spec's expirations view window (spec 09 §8). */
const EXPIRING_WITHIN_DAYS = 30;

function QualificationExpirations() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);

  const accountQuery = useAccountByUser({ enabled: expanded });
  const accountId = accountQuery.data?.accountId;
  const expiringQuery = useDriverQualifications(accountId, null, EXPIRING_WITHIN_DAYS, {
    enabled: expanded && !!accountId,
  });
  const expiring = expiringQuery.data ?? [];

  useEffect(() => {
    setLoading(expiringQuery.isFetching);
  }, [expiringQuery.isFetching, setLoading]);

  return (
    <TableAccordion
      title={t('workforce.expirations.title')}
      expanded={expanded}
      setExpanded={setExpanded}
    >
      {expiring.length === 0 ? (
        <ArgonTypography variant="caption" color="secondary">
          {t('workforce.expirations.empty')}
        </ArgonTypography>
      ) : (
        <Table
          columns={[
            { name: 'driver', title: t('workforce.expirations.driver'), align: 'left' },
            { name: 'type', title: t('workforce.qualifications.type'), align: 'left' },
            { name: 'category', title: t('workforce.qualifications.category'), align: 'center' },
            { name: 'number', title: t('workforce.qualifications.number'), align: 'center' },
            { name: 'expiresAt', title: t('workforce.qualifications.expiresAt'), align: 'center' },
            { name: 'daysLeft', title: t('workforce.expirations.daysLeft'), align: 'center' },
            { name: 'status', title: t('workforce.qualifications.status'), align: 'center' },
            { name: 'id' },
          ]}
          rows={expiring.map((qualification) => {
            const days = daysUntil(qualification.expiresAt);
            return {
              driver: (
                <ArgonTypography variant="caption" fontWeight="medium">
                  {qualification.driverName}
                </ArgonTypography>
              ),
              type: <TextCell>{qualificationTypeLabel(t, qualification.qualificationType)}</TextCell>,
              category: <TextCell>{qualification.category}</TextCell>,
              number: <TextCell>{qualification.number}</TextCell>,
              expiresAt: <TextCell>{formatDateOnly(qualification.expiresAt)}</TextCell>,
              daysLeft: (
                <ArgonBadge
                  badgeContent={
                    days === null
                      ? '-'
                      : days < 0
                        ? t('workforce.expirations.expired')
                        : String(days)
                  }
                  color={expiryColor(qualification.expiresAt)}
                  size="xs"
                  container
                />
              ),
              status: (
                <ArgonBadge
                  badgeContent={qualificationStatusLabel(t, qualification.status)}
                  color={statusColor(qualification.status)}
                  size="xs"
                  container
                />
              ),
              id: qualification.driverQualificationId,
            };
          })}
          selectedField="driver"
        />
      )}
    </TableAccordion>
  );
}

export default QualificationExpirations;
