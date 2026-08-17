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
import type { Dispatch, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import Grid from '@mui/material/Grid';
import Divider from '@mui/material/Divider';
import IconButton from '@mui/material/IconButton';
import Icon from '@mui/material/Icon';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import Switch from '@mui/material/Switch';
import FormControlLabel from '@mui/material/FormControlLabel';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import ArgonInput from 'components/ArgonInput';
import ArgonButton from 'components/ArgonButton';
import FormDialog from 'controls/Dialogs/FormDialog';
import { toDateTimeLocalInput, fromDateTimeLocalInput } from 'utils/dateUtils';
import type {
  PlatformAnnouncement,
  PlatformAnnouncementDtoInput,
  AnnouncementSeverity,
} from 'api/manager/platformStatus';
import {
  useCreateAnnouncement,
  useUpdateAnnouncement,
  useDeleteAnnouncement,
} from 'queries/platformStatus';

const SEVERITIES: AnnouncementSeverity[] = ['INFO', 'WARNING', 'CRITICAL'];
const MESSAGE_MAX_LENGTH = 500;

export interface AnnouncementManagerDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  announcements: PlatformAnnouncement[];
}

interface DraftState {
  platformAnnouncementId: string | null;
  messageEn: string;
  messageEs: string;
  severity: AnnouncementSeverity;
  startsAt: string;
  endsAt: string;
  active: boolean;
}

const emptyDraft = (): DraftState => ({
  platformAnnouncementId: null,
  messageEn: '',
  messageEs: '',
  severity: 'INFO',
  startsAt: '',
  endsAt: '',
  active: true,
});

/**
 * `datetime-local` value ⇄ UTC ISO. Empty string means "unscheduled" (null).
 * Both directions come from `utils/dateUtils` so this dialog and the trip manager cannot drift apart.
 */
const toIsoOrNull = (value: string): string | null => fromDateTimeLocalInput(value);

const toLocalInput = (value: string | null | undefined): string => toDateTimeLocalInput(value);

/**
 * Administrator-only announcement CRUD. Rendered only when the current principal
 * is an Administrator; the backend enforces the same restriction through the
 * `Administrative` resource, so hiding this is UX, not the security boundary.
 */
const AnnouncementManagerDialog = ({ open, setOpen, announcements }: AnnouncementManagerDialogProps) => {
  const { t } = useTranslation();
  const [draft, setDraft] = useState<DraftState>(emptyDraft);
  const [error, setError] = useState<string | null>(null);

  const createAnnouncement = useCreateAnnouncement();
  const updateAnnouncement = useUpdateAnnouncement();
  const deleteAnnouncement = useDeleteAnnouncement();

  const resetDraft = () => {
    setDraft(emptyDraft());
    setError(null);
  };

  const startEdit = (announcement: PlatformAnnouncement) => {
    setError(null);
    setDraft({
      platformAnnouncementId: announcement.platformAnnouncementId,
      messageEn: announcement.messageEn,
      messageEs: announcement.messageEs ?? '',
      severity: announcement.severity,
      startsAt: toLocalInput(announcement.startsAt),
      endsAt: toLocalInput(announcement.endsAt),
      active: announcement.active,
    });
  };

  // Mirrors the server-side validator so the author sees the problem inline.
  const validate = (): string | null => {
    if (!draft.messageEn.trim()) return t('platformStatus.manage.errors.messageRequired');
    if (draft.messageEn.length > MESSAGE_MAX_LENGTH || draft.messageEs.length > MESSAGE_MAX_LENGTH) {
      return t('platformStatus.manage.errors.messageTooLong', { max: MESSAGE_MAX_LENGTH });
    }
    if (draft.startsAt && draft.endsAt && new Date(draft.endsAt) <= new Date(draft.startsAt)) {
      return t('platformStatus.manage.errors.windowInvalid');
    }
    return null;
  };

  const handleSave = async () => {
    const validationError = validate();
    if (validationError) {
      setError(validationError);
      return;
    }

    const announcement: PlatformAnnouncementDtoInput = {
      messageEn: draft.messageEn.trim(),
      messageEs: draft.messageEs.trim() ? draft.messageEs.trim() : null,
      severity: draft.severity,
      startsAt: toIsoOrNull(draft.startsAt),
      endsAt: toIsoOrNull(draft.endsAt),
      active: draft.active,
    };

    if (draft.platformAnnouncementId) {
      await updateAnnouncement.mutateAsync({
        platformAnnouncementId: draft.platformAnnouncementId,
        announcement,
      });
    } else {
      await createAnnouncement.mutateAsync(announcement);
    }
    resetDraft();
  };

  return (
    <FormDialog
      title={t('platformStatus.manage.title')}
      open={open}
      setOpen={setOpen}
      handleSave={handleSave}
      handleCancel={resetDraft}
      maxWidth="md"
    >
      <ArgonBox display="flex" flexDirection="column" gap={2} pt={1}>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, md: 6 }}>
            <ArgonTypography variant="caption" fontWeight="bold">
              {t('platformStatus.manage.messageEn')}
            </ArgonTypography>
            <ArgonInput
              multiline
              rows={3}
              value={draft.messageEn}
              inputProps={{ maxLength: MESSAGE_MAX_LENGTH }}
              onChange={(event: React.ChangeEvent<HTMLInputElement>) =>
                setDraft({ ...draft, messageEn: event.target.value })
              }
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <ArgonTypography variant="caption" fontWeight="bold">
              {t('platformStatus.manage.messageEs')}
            </ArgonTypography>
            <ArgonInput
              multiline
              rows={3}
              value={draft.messageEs}
              inputProps={{ maxLength: MESSAGE_MAX_LENGTH }}
              onChange={(event: React.ChangeEvent<HTMLInputElement>) =>
                setDraft({ ...draft, messageEs: event.target.value })
              }
            />
            <ArgonTypography variant="caption" color="text">
              {t('platformStatus.manage.messageEsHint')}
            </ArgonTypography>
          </Grid>

          <Grid size={{ xs: 12, md: 4 }}>
            <ArgonTypography variant="caption" fontWeight="bold">
              {t('platformStatus.manage.severity')}
            </ArgonTypography>
            <Select
              fullWidth
              size="small"
              value={draft.severity}
              onChange={(event) => setDraft({ ...draft, severity: event.target.value as AnnouncementSeverity })}
            >
              {SEVERITIES.map((severity) => (
                <MenuItem key={severity} value={severity}>
                  {t(`platformStatus.severity.${severity}` as const)}
                </MenuItem>
              ))}
            </Select>
          </Grid>
          <Grid size={{ xs: 12, md: 4 }}>
            <ArgonTypography variant="caption" fontWeight="bold">
              {t('platformStatus.manage.startsAt')}
            </ArgonTypography>
            <ArgonInput
              type="datetime-local"
              value={draft.startsAt}
              onChange={(event: React.ChangeEvent<HTMLInputElement>) =>
                setDraft({ ...draft, startsAt: event.target.value })
              }
            />
          </Grid>
          <Grid size={{ xs: 12, md: 4 }}>
            <ArgonTypography variant="caption" fontWeight="bold">
              {t('platformStatus.manage.endsAt')}
            </ArgonTypography>
            <ArgonInput
              type="datetime-local"
              value={draft.endsAt}
              onChange={(event: React.ChangeEvent<HTMLInputElement>) =>
                setDraft({ ...draft, endsAt: event.target.value })
              }
            />
          </Grid>

          <Grid size={{ xs: 12 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={draft.active}
                  onChange={(event) => setDraft({ ...draft, active: event.target.checked })}
                />
              }
              label={
                <ArgonTypography variant="caption">
                  {t('platformStatus.manage.active')}
                </ArgonTypography>
              }
            />
            {draft.platformAnnouncementId && (
              <ArgonButton variant="outlined" color="dark" size="small" onClick={resetDraft} sx={{ ml: 2 }}>
                {t('platformStatus.manage.newAnnouncement')}
              </ArgonButton>
            )}
          </Grid>
        </Grid>

        {error && (
          <ArgonTypography variant="caption" color="error">
            {error}
          </ArgonTypography>
        )}

        <Divider />

        <ArgonTypography variant="caption" fontWeight="bold">
          {t('platformStatus.manage.existing')}
        </ArgonTypography>
        {announcements.length === 0 ? (
          <ArgonTypography variant="caption" color="text">
            {t('platformStatus.manage.none')}
          </ArgonTypography>
        ) : (
          announcements.map((announcement) => (
            <ArgonBox
              key={announcement.platformAnnouncementId}
              display="flex"
              alignItems="center"
              justifyContent="space-between"
              gap={1}
            >
              <ArgonBox minWidth={0}>
                <ArgonTypography variant="button" textTransform="none" display="block" sx={{ wordBreak: 'break-word' }}>
                  {announcement.messageEn}
                </ArgonTypography>
                <ArgonTypography variant="caption" color="text">
                  {t(`platformStatus.severity.${announcement.severity}` as const)}
                  {' · '}
                  {announcement.active
                    ? t('platformStatus.manage.stateActive')
                    : t('platformStatus.manage.stateInactive')}
                </ArgonTypography>
              </ArgonBox>
              <ArgonBox flexShrink={0}>
                <IconButton size="small" onClick={() => startEdit(announcement)} aria-label={t('generic.edit')}>
                  <Icon fontSize="small">edit</Icon>
                </IconButton>
                <IconButton
                  size="small"
                  onClick={() => deleteAnnouncement.mutate(announcement.platformAnnouncementId)}
                  aria-label={t('generic.delete')}
                >
                  <Icon fontSize="small">delete</Icon>
                </IconButton>
              </ArgonBox>
            </ArgonBox>
          ))
        )}
      </ArgonBox>
    </FormDialog>
  );
};

export default AnnouncementManagerDialog;
