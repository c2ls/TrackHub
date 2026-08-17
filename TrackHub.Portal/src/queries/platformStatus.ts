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
 * Platform status query hooks. Components consume these — not the api layer.
 *
 * Everything polls at 60 s with `refetchIntervalInBackground: false`, so a
 * backgrounded tab stops hitting the backends (AC 6).
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { probeAllServices } from 'api/core/healthProbe';
import {
  getVisibleAnnouncements,
  getPlatformAnnouncements,
  getBackgroundJobStatus,
  createPlatformAnnouncement,
  updatePlatformAnnouncement,
  deletePlatformAnnouncement,
} from 'api/manager/platformStatus';
import type { PlatformAnnouncementDtoInput } from 'api/manager/platformStatus';
import { getPlatformSyncActivity } from 'api/telemetry/platformStatus';

/** Shared poll cadence for every status source. */
export const STATUS_POLL_MS = 60_000;

export const platformStatusKeys = {
  all: ['platformStatus'] as const,
  health: () => [...platformStatusKeys.all, 'health'] as const,
  announcements: () => [...platformStatusKeys.all, 'announcements'] as const,
  managedAnnouncements: () => [...platformStatusKeys.all, 'managedAnnouncements'] as const,
  syncActivity: () => [...platformStatusKeys.all, 'syncActivity'] as const,
  jobs: () => [...platformStatusKeys.all, 'jobs'] as const,
};

/**
 * Browser-side probes of every backend `/health`. Never rejects (each probe
 * resolves to a per-service state), so a total outage still renders the page.
 */
export function useServiceHealth() {
  return useQuery({
    queryKey: platformStatusKeys.health(),
    queryFn: () => probeAllServices(),
    refetchInterval: STATUS_POLL_MS,
    refetchIntervalInBackground: false,
    // Health is the page's whole point — never serve a stale snapshot silently.
    staleTime: 0,
    // Overrides the global `false`: polling pauses while the tab is backgrounded, so without
    // this a visitor returning after an hour would stare at an hour-old snapshot for up to 60 s.
    refetchOnWindowFocus: true,
    retry: false,
    // Failure is rendered as page content, not as a toast.
    meta: { silent: true },
  });
}

/** Currently-visible announcements, anonymous. Resolves to [] when Manager is down. */
export function useVisibleAnnouncements() {
  return useQuery({
    queryKey: platformStatusKeys.announcements(),
    queryFn: getVisibleAnnouncements,
    refetchInterval: STATUS_POLL_MS,
    refetchIntervalInBackground: false,
    refetchOnWindowFocus: true,
    retry: false,
    // Failure is rendered as page content, not as a toast.
    meta: { silent: true },
  });
}

/** Administrator tier: SyncWorker liveness. Only fetched when `enabled`. */
export function usePlatformSyncActivity(enabled: boolean) {
  return useQuery({
    queryKey: platformStatusKeys.syncActivity(),
    queryFn: () => getPlatformSyncActivity(),
    enabled,
    refetchInterval: STATUS_POLL_MS,
    refetchIntervalInBackground: false,
    retry: false,
    // Failure is rendered as page content, not as a toast.
    meta: { silent: true },
  });
}

/** Administrator tier: latest run per background job. Only fetched when `enabled`. */
export function useBackgroundJobStatus(enabled: boolean) {
  return useQuery({
    queryKey: platformStatusKeys.jobs(),
    queryFn: getBackgroundJobStatus,
    enabled,
    refetchInterval: STATUS_POLL_MS,
    refetchIntervalInBackground: false,
    retry: false,
    // Failure is rendered as page content, not as a toast.
    meta: { silent: true },
  });
}

/** Administrator tier: every announcement including drafts and expired rows. */
export function useManagedAnnouncements(enabled: boolean) {
  return useQuery({
    queryKey: platformStatusKeys.managedAnnouncements(),
    queryFn: () => getPlatformAnnouncements(),
    enabled,
    retry: false,
    // Failure is rendered as page content, not as a toast.
    meta: { silent: true },
  });
}

/** Invalidates both the management list and the public banner after any write. */
function useAnnouncementInvalidation() {
  const queryClient = useQueryClient();
  return () => {
    queryClient.invalidateQueries({ queryKey: platformStatusKeys.managedAnnouncements() });
    queryClient.invalidateQueries({ queryKey: platformStatusKeys.announcements() });
  };
}

export function useCreateAnnouncement() {
  const invalidate = useAnnouncementInvalidation();
  return useMutation({
    mutationFn: (announcement: PlatformAnnouncementDtoInput) => createPlatformAnnouncement(announcement),
    onSuccess: invalidate,
  });
}

export function useUpdateAnnouncement() {
  const invalidate = useAnnouncementInvalidation();
  return useMutation({
    mutationFn: ({
      platformAnnouncementId,
      announcement,
    }: {
      platformAnnouncementId: string;
      announcement: PlatformAnnouncementDtoInput;
    }) => updatePlatformAnnouncement(platformAnnouncementId, announcement),
    onSuccess: invalidate,
  });
}

export function useDeleteAnnouncement() {
  const invalidate = useAnnouncementInvalidation();
  return useMutation({
    mutationFn: (platformAnnouncementId: string) => deletePlatformAnnouncement(platformAnnouncementId),
    onSuccess: invalidate,
  });
}
