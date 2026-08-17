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

import Alert from '@mui/material/Alert';
import ArgonBox from 'components/ArgonBox';
import type { VisibleAnnouncement } from 'api/manager/platformStatus';
import { announcementText, severityToMuiSeverity } from './announcementText';

export interface AnnouncementListProps {
  announcements: VisibleAnnouncement[];
  /** Two-letter UI language; picks MessageEs when present, else falls back to English. */
  language: string;
}

/**
 * Announcement banners for the status page. The text is author-written content,
 * rendered as PLAIN TEXT — never markdown or HTML (ST-09).
 */
const AnnouncementList = ({ announcements, language }: AnnouncementListProps) => {
  if (announcements.length === 0) return null;

  return (
    <ArgonBox mb={3} display="flex" flexDirection="column" gap={1}>
      {announcements.map((announcement) => (
        <Alert
          key={announcement.id}
          severity={severityToMuiSeverity(announcement.severity)}
          data-testid="announcement"
        >
          {announcementText(announcement, language)}
        </Alert>
      ))}
    </ArgonBox>
  );
};

export default AnnouncementList;
