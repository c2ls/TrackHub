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
 * Public platform status page (`/status`).
 *
 * Renders WITHOUT authentication and outside the signed-in shell: the scenario
 * it exists for is "nobody can log in — is it us or the platform?". Every data
 * source degrades independently; a failed probe or a Manager outage must never
 * blank the page.
 */

import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import Card from '@mui/material/Card';
import Container from '@mui/material/Container';
import Grid from '@mui/material/Grid';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import ArgonButton from 'components/ArgonButton';
import { PROBED_SERVICES, overallState } from 'api/core/healthProbe';
import type { HealthProbeResult, ProbedService, ServiceState } from 'api/core/healthProbe';
import { deriveSyncWorkerState } from 'api/telemetry/platformStatus';
import {
  useServiceHealth,
  useVisibleAnnouncements,
  usePlatformSyncActivity,
  useBackgroundJobStatus,
  useManagedAnnouncements,
} from 'queries/platformStatus';
import { useAuth } from 'AuthContext';
import { isAdmin, isManager } from 'api/security/users';
import ServiceTile from './ServiceTile';
import AnnouncementList from './AnnouncementList';
import BackgroundJobsTable from './BackgroundJobsTable';
import AnnouncementManagerDialog from './AnnouncementManagerDialog';
import { relativeAge } from './jobStatus';

const PlatformStatus = () => {
  const { t, i18n } = useTranslation();
  const { isAuthenticated, login } = useAuth();
  const [manageOpen, setManageOpen] = useState(false);

  /**
   * The page resolves the admin tier itself rather than taking a prop: it is
   * rendered from the route table for signed-out visitors too, where no shell
   * state exists to pass down. A failed/absent check degrades to the public tier.
   */
  // isAdmin/isManager are silent ops elsewhere too (see api/security/users): a Security outage
  // must degrade this page to its public tier, not raise a toast on top of the outage it reports.
  const adminCheck = useQuery({
    queryKey: ['platformStatus', 'isAdmin'],
    queryFn: () => isAdmin(),
    enabled: isAuthenticated,
    retry: false,
    meta: { silent: true },
  });

  const managerCheck = useQuery({
    queryKey: ['platformStatus', 'isManager'],
    queryFn: () => isManager(),
    enabled: isAuthenticated,
    retry: false,
    meta: { silent: true },
  });

  /**
   * Two distinct tiers above the public one — these gate UX only; the backend enforces the same
   * split through the authorization resources.
   *
   * - syncTier (SuperAdministrator OR Manager): the GPS synchronisation tile. It pairs with the
   *   gpsIntegration dashboard Managers already own, and its query returns timestamps only.
   *   Guarded server-side by `OperatorSyncRuns/Read`.
   * - adminTier (SuperAdministrator only): background jobs and announcement management —
   *   operational internals spanning every account. Guarded by `Administrative/Read|Write`.
   */
  const adminTier = isAuthenticated && adminCheck.data === true;
  const syncTier = isAuthenticated && (adminCheck.data === true || managerCheck.data === true);

  const health = useServiceHealth();
  const announcements = useVisibleAnnouncements();
  const syncActivity = usePlatformSyncActivity(syncTier);
  const jobs = useBackgroundJobStatus(adminTier);
  const managedAnnouncements = useManagedAnnouncements(adminTier);

  const results = health.data;
  const overall: ServiceState = results ? overallState(results) : 'unknown';

  const detailFor = (result: HealthProbeResult | undefined): string | undefined => {
    if (!result?.reason) return undefined;
    return t(`platformStatus.detail.${result.reason}` as const);
  };

  // Re-rendered every 30 s so "last checked" ages visibly between polls rather than sitting
  // frozen at "0 minutes ago" until the next fetch.
  const [now, setNow] = useState(() => Date.now());
  useEffect(() => {
    const timer = setInterval(() => setNow(Date.now()), 30_000);
    return () => clearInterval(timer);
  }, []);

  const lastChecked = health.dataUpdatedAt
    ? relativeAge(new Date(health.dataUpdatedAt).toISOString(), now)
    : null;

  const syncState = syncTier ? deriveSyncWorkerState(syncActivity.data ?? undefined) : 'unknown';

  return (
    <ArgonBox minHeight="100vh" py={5} sx={{ backgroundColor: ({ palette }) => palette.background.default }}>
      <Container maxWidth="lg">
        {/* Announcements first: a maintenance notice outranks the tiles. */}
        <AnnouncementList announcements={announcements.data ?? []} language={i18n.language} />

        <Card sx={{ mb: 3 }}>
          <ArgonBox
            p={3}
            display="flex"
            flexWrap="wrap"
            gap={2}
            alignItems="center"
            justifyContent="space-between"
          >
            <ArgonBox>
              <ArgonTypography variant="h4">{t('platformStatus.title')}</ArgonTypography>
              <ArgonTypography
                variant="button"
                fontWeight="bold"
                textTransform="none"
                color={overall === 'up' ? 'success' : overall === 'down' ? 'error' : 'secondary'}
              >
                {t(`platformStatus.overall.${overall}` as const)}
              </ArgonTypography>
              {/* Sessions are in-memory only, so arriving here by bookmark, typed URL or reload
                  means no session even for a SuperAdministrator — and this page deliberately never
                  auto-redirects to login. Say so, rather than silently showing only the public
                  tier and leaving the viewer to wonder where their tiles went. */}
              {!isAuthenticated && (
                <ArgonTypography variant="caption" color="text" display="block">
                  {t('platformStatus.signInHint')}
                </ArgonTypography>
              )}
              <ArgonTypography variant="caption" color="text" display="block">
                {health.isFetching
                  ? t('platformStatus.checking')
                  : lastChecked
                    ? t('platformStatus.lastChecked', {
                        age: t(`platformStatus.jobs.ago.${lastChecked.unit}` as const, { count: lastChecked.count }),
                      })
                    : ''}
              </ArgonTypography>
            </ArgonBox>
            <ArgonBox display="flex" gap={1} flexWrap="wrap">
              <ArgonButton
                variant="gradient"
                color="info"
                size="small"
                onClick={() => health.refetch()}
                disabled={health.isFetching}
              >
                {t('platformStatus.refresh')}
              </ArgonButton>
              {isAuthenticated ? (
                <ArgonButton variant="outlined" color="dark" size="small" component={Link} to="/dashboard">
                  {t('platformStatus.backToPortal')}
                </ArgonButton>
              ) : (
                // Sessions are in-memory only, so opening /status directly (a bookmark, a reload)
                // means no session even for an administrator — and this page deliberately never
                // auto-redirects to login. Offer it as an explicit choice instead, otherwise the
                // administrator tier is silently unreachable from the very entry point the help
                // topic recommends.
                <ArgonButton variant="outlined" color="dark" size="small" onClick={() => login()}>
                  {t('platformStatus.signIn')}
                </ArgonButton>
              )}
            </ArgonBox>
          </ArgonBox>
        </Card>

        <Grid container spacing={2}>
          {PROBED_SERVICES.map((service: ProbedService) => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={service}>
              <ServiceTile
                name={t(`platformStatus.services.${service}.name` as const)}
                description={t(`platformStatus.services.${service}.description` as const)}
                state={results?.[service]?.state ?? 'unknown'}
                detail={detailFor(results?.[service])}
              />
            </Grid>
          ))}

          {syncTier && (
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
              <ServiceTile
                name={t('platformStatus.services.syncWorker.name')}
                description={t('platformStatus.services.syncWorker.description')}
                state={syncState}
                detail={
                  syncState === 'unknown' && syncActivity.data && !syncActivity.data.hasEnabledGpsIntegration
                    ? t('platformStatus.detail.nothingToSync')
                    : undefined
                }
              />
            </Grid>
          )}
        </Grid>

        {adminTier && (
          <ArgonBox mt={3} display="flex" flexDirection="column" gap={3}>
            <BackgroundJobsTable jobs={jobs.data ?? []} />

            <ArgonBox display="flex" justifyContent="flex-end">
              <ArgonButton variant="gradient" color="warning" size="small" onClick={() => setManageOpen(true)}>
                {t('platformStatus.manage.open')}
              </ArgonButton>
            </ArgonBox>

            <AnnouncementManagerDialog
              open={manageOpen}
              setOpen={setManageOpen}
              announcements={managedAnnouncements.data ?? []}
            />
          </ArgonBox>
        )}

        <ArgonBox mt={4} textAlign="center">
          <ArgonTypography variant="caption" color="text">
            {t('platformStatus.bookmarkHint')}
          </ArgonTypography>
        </ArgonBox>
      </Container>
    </ArgonBox>
  );
};

export default PlatformStatus;
