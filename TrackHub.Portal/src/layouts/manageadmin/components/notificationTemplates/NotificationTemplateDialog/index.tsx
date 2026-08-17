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

import type { Dispatch, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import FormDialog from "controls/Dialogs/FormDialog";
import CustomCheckbox from 'controls/Dialogs/CustomCheckbox';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import { TEMPLATE_KEYS, NOTIFICATION_CHANNELS } from 'utils/notificationsCatalog';
import { toCamelCase } from 'utils/stringUtils';

/** Tokens the notification engine substitutes into subject/body. Static legend only. */
const TEMPLATE_TOKENS =
  '{eventType} {severity} {resourceType} {resourceId} {occurredAt} {link} {count} {eventTypes}';

/** Dialog/form state for an account-override notification template. */
export interface NotificationTemplateFormValues {
  notificationTemplateId?: string;
  templateKey?: string;
  channel?: string;
  locale?: string;
  subject?: string;
  body?: string;
  active?: boolean;
}

interface NotificationTemplateDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: NotificationTemplateFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

function NotificationTemplateDialog({
  open,
  setOpen,
  handleSubmit,
  values,
  handleChange,
  errors,
}: NotificationTemplateDialogProps) {
  const { t } = useTranslation();

  // Alert-event keys reuse the shared alertEventTypes labels; only the two
  // system template keys have their own labels.
  const keyLabel = (key: string): string =>
    key === 'TestNotification' || key === 'NotificationDigest'
      ? t(`notificationTemplates.keys.${toCamelCase(key)}` as 'notificationTemplates.keys.testNotification', {
          defaultValue: key,
        })
      : t(`alertEventTypes.${toCamelCase(key)}` as 'alertEventTypes.geofenceEntered', {
          defaultValue: key,
        });

  const keyOptions = TEMPLATE_KEYS.map(key => ({ value: key, label: keyLabel(key) }));

  const channelOptions = NOTIFICATION_CHANNELS.map(channel => ({
    value: channel,
    label: t(`notificationChannels.${toCamelCase(channel)}` as 'notificationChannels.inApp', {
      defaultValue: channel,
    }),
  }));

  const localeOptions = [
    { value: 'en', label: t('notificationTemplates.localeEn') },
    { value: 'es', label: t('notificationTemplates.localeEs') },
  ];

  return (
    <FormDialog
      title={values.notificationTemplateId ? t('notificationTemplates.update') : t('notificationTemplates.create')}
      handleSave={handleSubmit}
      open={open}
      setOpen={setOpen}
      maxWidth="md">
      <form>
        <CustomSelect
          list={keyOptions}
          name="templateKey"
          id="templateKey"
          label={t('notificationTemplates.templateKey')}
          value={values.templateKey || ''}
          handleChange={handleChange}
          numericValue={false}
          required
        />
        <CustomSelect
          list={channelOptions}
          name="channel"
          id="channel"
          label={t('notificationTemplates.channel')}
          value={values.channel || ''}
          handleChange={handleChange}
          numericValue={false}
          required
        />
        <CustomSelect
          list={localeOptions}
          name="locale"
          id="locale"
          label={t('notificationTemplates.locale')}
          value={values.locale || ''}
          handleChange={handleChange}
          numericValue={false}
          required
        />
        <CustomTextField
          margin="normal"
          name="subject"
          id="subject"
          label={t('notificationTemplates.subject')}
          type="text"
          fullWidth
          value={values.subject || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="body"
          id="body"
          label={t('notificationTemplates.body')}
          type="text"
          fullWidth
          multiline
          minRows={4}
          value={values.body || ''}
          onChange={handleChange}
          required
          errorMsg={errors.body}
        />
        <ArgonBox mt={1} mb={1}>
          <ArgonTypography variant="caption" fontWeight="medium">
            {t('notificationTemplates.placeholders')}
          </ArgonTypography>
          <ArgonBox>
            <ArgonTypography variant="caption" color="secondary">
              {TEMPLATE_TOKENS}
            </ArgonTypography>
          </ArgonBox>
        </ArgonBox>
        <CustomCheckbox
          name="active"
          id="active"
          value={values.active}
          handleChange={handleChange}
          label={t('notificationTemplates.active')} />
      </form>
    </FormDialog>
  );
}

export default NotificationTemplateDialog;
