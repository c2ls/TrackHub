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

import { useTranslation } from 'react-i18next';
import Card from '@mui/material/Card';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Tooltip from '@mui/material/Tooltip';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import ArgonBadge from 'components/ArgonBadge';
import { deriveJobState, relativeAge, JOB_STATE_COLOR } from './jobStatus';

export interface BackgroundJobRow {
  jobKey: string;
  status: string;
  startedAt: string;
  completedAt?: string | null;
  attempts: number;
  errorCode?: string | null;
  recordsEveryCycle: boolean;
}

export interface BackgroundJobsTableProps {
  jobs: BackgroundJobRow[];
}

/** Known JobKeys get a friendly localized name; anything else falls back to the raw key. */
const FRIENDLY_KEYS = new Set([
  'alert-evaluation',
  'notification-dispatch',
  'notification-digest',
  'delivery-retention',
  'document-scan',
  'document-expiration',
  'document-retention-cleanup',
  'trial-expiration',
  'geofence-dwell-evaluation',
]);

const BackgroundJobsTable = ({ jobs }: BackgroundJobsTableProps) => {
  const { t } = useTranslation();

  const friendlyName = (jobKey: string): string =>
    FRIENDLY_KEYS.has(jobKey) ? t(`platformStatus.jobs.names.${jobKey}` as never) : jobKey;

  const lastActivity = (startedAt: string): string => {
    const age = relativeAge(startedAt);
    if (!age) return t('platformStatus.jobs.never');
    return t(`platformStatus.jobs.ago.${age.unit}` as const, { count: age.count });
  };

  return (
    <Card>
      <ArgonBox p={2}>
        <ArgonTypography variant="h6">{t('platformStatus.jobs.title')}</ArgonTypography>
        <ArgonTypography variant="caption" color="text">
          {t('platformStatus.jobs.subtitle')}
        </ArgonTypography>
      </ArgonBox>
      {jobs.length === 0 ? (
        <ArgonBox px={2} pb={2}>
          <ArgonTypography variant="button" color="text" textTransform="none">
            {t('platformStatus.jobs.empty')}
          </ArgonTypography>
        </ArgonBox>
      ) : (
        // Wide content scrolls inside its own container; the page never scrolls sideways.
        <TableContainer sx={{ overflowX: 'auto' }}>
          <Table size="small">
            <TableHead sx={{ display: 'table-header-group' }}>
              <TableRow>
                <TableCell>{t('platformStatus.jobs.columns.job')}</TableCell>
                <TableCell>{t('platformStatus.jobs.columns.state')}</TableCell>
                <TableCell>{t('platformStatus.jobs.columns.lastActivity')}</TableCell>
                <TableCell>{t('platformStatus.jobs.columns.lastOutcome')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {jobs.map((job) => {
                const state = deriveJobState(job);
                return (
                  <TableRow key={job.jobKey} data-testid={`job-row-${state}`}>
                    <TableCell>
                      <ArgonTypography variant="button" fontWeight="medium" textTransform="none">
                        {friendlyName(job.jobKey)}
                      </ArgonTypography>
                    </TableCell>
                    <TableCell>
                      <ArgonBadge
                        variant="gradient"
                        badgeContent={t(`platformStatus.jobs.state.${state}` as const)}
                        color={JOB_STATE_COLOR[state]}
                        size="xs"
                        container
                      />
                    </TableCell>
                    <TableCell>
                      <ArgonTypography variant="caption" color="text">
                        {lastActivity(job.startedAt)}
                      </ArgonTypography>
                    </TableCell>
                    <TableCell>
                      {job.errorCode ? (
                        <Tooltip title={job.errorCode}>
                          <ArgonTypography variant="caption" color="error">
                            {job.status}
                          </ArgonTypography>
                        </Tooltip>
                      ) : (
                        <ArgonTypography variant="caption" color="text">
                          {job.status}
                        </ArgonTypography>
                      )}
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Card>
  );
};

export default BackgroundJobsTable;
