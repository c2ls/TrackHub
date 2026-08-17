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

import type { AnnouncementSeverity } from 'api/manager/platformStatus';

/** Announcement copy is author-written per language, so it is NOT an i18n key. */
export interface LocalizableAnnouncement {
  messageEn: string;
  messageEs?: string | null;
}

/**
 * Picks the message for the viewer's language. Spanish viewers get `messageEs`
 * when the author supplied it, otherwise English — the author is never forced to
 * translate, and a missing translation must never render as blank.
 */
export function announcementText(announcement: LocalizableAnnouncement, language: string): string {
  const isSpanish = (language || '').toLowerCase().startsWith('es');
  if (isSpanish && announcement.messageEs && announcement.messageEs.trim().length > 0) {
    return announcement.messageEs;
  }
  return announcement.messageEn;
}

/** Severity → MUI Alert severity (Info=blue, Warning=amber, Critical=red). */
export function severityToMuiSeverity(severity: AnnouncementSeverity): 'info' | 'warning' | 'error' {
  switch (severity) {
    case 'WARNING':
      return 'warning';
    case 'CRITICAL':
      return 'error';
    default:
      return 'info';
  }
}
