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
 * Platform status API (Manager backend): plain typed async functions.
 * Failures THROW ApiError — fallbacks and toasts belong to the caller layer.
 */

import axios from 'axios';
import { executeGraphQL } from 'api/core/graphqlClient';
import { REST_ENDPOINTS } from 'api/core/endpoints';
import type {
  PlatformAnnouncementItemFragment,
  PlatformAnnouncementDtoInput,
  AnnouncementSeverity,
} from './generated/graphql';
import {
  GetPlatformAnnouncementsDocument,
  GetBackgroundJobStatusDocument,
  CreatePlatformAnnouncementDocument,
  UpdatePlatformAnnouncementDocument,
  DeletePlatformAnnouncementDocument,
} from './platformStatusOperations';

export type PlatformAnnouncement = PlatformAnnouncementItemFragment;
export type { PlatformAnnouncementDtoInput, AnnouncementSeverity };

/**
 * The anonymous projection returned by `GET /api/PlatformStatus/announcements`.
 * Narrower than the administrator VM on purpose: no `active`, no `lastModified`.
 */
export interface VisibleAnnouncement {
  id: string;
  messageEn: string;
  messageEs: string | null;
  severity: AnnouncementSeverity;
  startsAt: string | null;
  endsAt: string | null;
}

/**
 * Currently-visible announcements, read WITHOUT authentication.
 *
 * Sanctioned anonymous fetch: `restClient` always acquires a token, which is
 * exactly wrong here — this must work for a signed-out visitor and for one whose
 * sign-in service is down. Returns [] rather than throwing when Manager is
 * unreachable: per ST-09 a Manager outage removes the banner, never the page.
 */
export async function getVisibleAnnouncements(): Promise<VisibleAnnouncement[]> {
  try {
    const response = await axios.get<VisibleAnnouncement[]>(
      REST_ENDPOINTS.managerPlatformAnnouncements,
      { timeout: 6000, withCredentials: false }
    );
    return Array.isArray(response.data) ? response.data : [];
  } catch {
    return [];
  }
}

/** All announcements including drafts and expired rows — Administrator only. */
export async function getPlatformAnnouncements(skip = 0, take = 50): Promise<PlatformAnnouncement[]> {
  const data = await executeGraphQL('manager', GetPlatformAnnouncementsDocument, { skip, take });
  return data.platformAnnouncements;
}

/** Latest run per background JobKey, platform-wide — Administrator only. */
export async function getBackgroundJobStatus() {
  const data = await executeGraphQL('manager', GetBackgroundJobStatusDocument);
  return data.backgroundJobStatus;
}

export async function createPlatformAnnouncement(
  announcement: PlatformAnnouncementDtoInput
): Promise<PlatformAnnouncement> {
  const data = await executeGraphQL('manager', CreatePlatformAnnouncementDocument, { announcement });
  return data.createPlatformAnnouncement;
}

export async function updatePlatformAnnouncement(
  platformAnnouncementId: string,
  announcement: PlatformAnnouncementDtoInput
): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdatePlatformAnnouncementDocument, {
    platformAnnouncementId,
    announcement,
  });
  return data.updatePlatformAnnouncement;
}

export async function deletePlatformAnnouncement(platformAnnouncementId: string): Promise<string> {
  const data = await executeGraphQL('manager', DeletePlatformAnnouncementDocument, {
    platformAnnouncementId,
  });
  return data.deletePlatformAnnouncement;
}
