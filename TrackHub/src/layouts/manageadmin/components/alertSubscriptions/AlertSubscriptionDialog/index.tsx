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
import FormDialog from "controls/Dialogs/FormDialog";
import CustomCheckbox from 'controls/Dialogs/CustomCheckbox';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import { ALERT_EVENT_TYPES, SUBSCRIPTION_CHANNELS } from 'utils/notificationsCatalog';
import { toCamelCase } from 'utils/stringUtils';

/** Sentinel select value meaning "all event types" (eventTypeFilter = null). */
export const ALL_EVENTS = 'all';

/** Dialog/form state for an alert subscription — a partial superset of the API record. */
export interface AlertSubscriptionFormValues {
  alertSubscriptionId?: string;
  principalType?: string;
  principalId?: string;
  eventTypeFilter?: string;
  channel?: string;
  contact?: string;
  enabled?: boolean;
}

interface AlertSubscriptionDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: AlertSubscriptionFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
  emailEnabled: boolean;
  whatsAppEnabled: boolean;
}

function AlertSubscriptionDialog({
  open,
  setOpen,
  handleSubmit,
  values,
  handleChange,
  errors,
  emailEnabled,
  whatsAppEnabled,
}: AlertSubscriptionDialogProps) {
  const { t } = useTranslation();

  const channelOptions = SUBSCRIPTION_CHANNELS
    .filter(channel => {
      if (channel === 'Email') return emailEnabled;
      if (channel === 'WhatsApp') return whatsAppEnabled;
      return true;
    })
    .map(channel => ({
      value: channel,
      label: t(`notificationChannels.${toCamelCase(channel)}` as 'notificationChannels.inApp', {
        defaultValue: channel,
      }),
    }));

  const eventOptions = [
    { value: ALL_EVENTS, label: t('alertSubscriptions.allEvents') },
    ...ALERT_EVENT_TYPES.map(type => ({
      value: type,
      label: t(`alertEventTypes.${toCamelCase(type)}` as 'alertEventTypes.geofenceEntered', {
        defaultValue: type,
      }),
    })),
  ];

  const principalOptions = [
    { value: 'User', label: t('alertSubscriptions.user') },
    { value: 'Driver', label: t('alertSubscriptions.driver') },
  ];

  return (
    <FormDialog
      title={values.alertSubscriptionId ? t('alertSubscriptions.update') : t('alertSubscriptions.create')}
      handleSave={handleSubmit}
      open={open}
      setOpen={setOpen}
      maxWidth="sm">
      <form>
        <CustomSelect
          list={principalOptions}
          name="principalType"
          id="principalType"
          label={t('alertSubscriptions.principalType')}
          value={values.principalType || ''}
          handleChange={handleChange}
          numericValue={false}
          required
        />
        <CustomTextField
          margin="normal"
          name="principalId"
          id="principalId"
          label={t('alertSubscriptions.principalId')}
          type="text"
          fullWidth
          value={values.principalId || ''}
          onChange={handleChange}
          required
          errorMsg={errors.principalId}
        />
        <CustomSelect
          list={eventOptions}
          name="eventTypeFilter"
          id="eventTypeFilter"
          label={t('alertSubscriptions.eventType')}
          value={values.eventTypeFilter || ALL_EVENTS}
          handleChange={handleChange}
          numericValue={false}
        />
        <CustomSelect
          list={channelOptions}
          name="channel"
          id="channel"
          label={t('alertSubscriptions.channel')}
          value={values.channel || ''}
          handleChange={handleChange}
          numericValue={false}
          required
        />
        <CustomTextField
          margin="normal"
          name="contact"
          id="contact"
          label={t('alertSubscriptions.contact')}
          type="text"
          fullWidth
          value={values.contact || ''}
          onChange={handleChange}
          errorMsg={errors.contact}
        />
        <CustomCheckbox
          name="enabled"
          id="enabled"
          value={values.enabled}
          handleChange={handleChange}
          label={t('alertSubscriptions.enabled')} />
      </form>
    </FormDialog>
  );
}

export default AlertSubscriptionDialog;
