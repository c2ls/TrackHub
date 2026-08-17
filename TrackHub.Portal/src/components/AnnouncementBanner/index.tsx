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
 * Platform announcement bar for the signed-in shell (ST-10).
 *
 * Mounted once, above the content, on every screen and for every role — a
 * message "everyone can see" must not require visiting the status page.
 * Dismissal is per announcement, per browser session (sessionStorage), so a
 * still-active notice returns on the next sign-in.
 */

import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import Alert from '@mui/material/Alert';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import { useVisibleAnnouncements } from 'queries/platformStatus';
import { announcementText, severityToMuiSeverity } from 'layouts/platformstatus/announcementText';

const STORAGE_KEY = 'dismissed_announcements';

function readDismissed(): string[] {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    const parsed: unknown = raw ? JSON.parse(raw) : [];
    return Array.isArray(parsed) ? parsed.filter((id): id is string => typeof id === 'string') : [];
  } catch {
    // A hostile/unavailable sessionStorage must never break the shell.
    return [];
  }
}

const AnnouncementBanner = () => {
  const { t, i18n } = useTranslation();
  const { data } = useVisibleAnnouncements();
  const [dismissed, setDismissed] = useState<string[]>(readDismissed);

  // Pure updater — React may invoke it twice under StrictMode, so the persist lives in an effect.
  const dismiss = useCallback((id: string) => {
    setDismissed((previous) => (previous.includes(id) ? previous : [...previous, id]));
  }, []);

  useEffect(() => {
    try {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(dismissed));
    } catch {
      // Non-fatal: dismissal just does not survive a reload.
    }
  }, [dismissed]);

  const visible = (data ?? []).filter((announcement) => !dismissed.includes(announcement.id));
  if (visible.length === 0) return null;

  return (
    <ArgonBox px={3} pt={2} display="flex" flexDirection="column" gap={1}>
      {visible.map((announcement) => (
        <Alert
          key={announcement.id}
          severity={severityToMuiSeverity(announcement.severity)}
          onClose={() => dismiss(announcement.id)}
          data-testid="shell-announcement"
        >
          {announcementText(announcement, i18n.language)}
          <ArgonTypography variant="caption" component={Link} to="/status" sx={{ ml: 1 }}>
            {t('platformStatus.viewStatus')}
          </ArgonTypography>
        </Alert>
      ))}
    </ArgonBox>
  );
};

export default AnnouncementBanner;
