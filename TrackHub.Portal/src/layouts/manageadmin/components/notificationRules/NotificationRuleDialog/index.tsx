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
import Icon from '@mui/material/Icon';
import IconButton from '@mui/material/IconButton';
import ArgonBox from 'components/ArgonBox';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import FormDialog from "controls/Dialogs/FormDialog";
import CustomCheckbox from 'controls/Dialogs/CustomCheckbox';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import type { NotificationRule, NotificationRuleDtoInput } from 'api/manager/notificationRules';
import { ALERT_EVENT_TYPES, NOTIFICATION_CHANNELS, DIGEST_MODES } from 'utils/notificationsCatalog';
import { toCamelCase } from 'utils/stringUtils';
import { parseJson } from 'utils/jsonUtils';

/** One ad-hoc contact row of the recipient selector. */
export interface RuleContact {
  channel: 'Email' | 'WhatsApp';
  address: string;
}

/** Roles selectable as rule recipients. */
const RECIPIENT_ROLES = ['Administrator', 'Manager'] as const;

/**
 * Dialog/form state for a notification rule. The JSON columns of the record
 * (recipientSelector, channelsJson, throttlingJson, configurationJson) are
 * expanded into structured fields; {@link ruleToFormValues} /
 * {@link formValuesToRuleInput} convert between the two shapes.
 */
export interface NotificationRuleFormValues {
  notificationRuleId?: string;
  accountId?: string;
  ruleKey?: string;
  ruleType?: string;
  enabled?: boolean;
  triggerEvent?: string;
  channels?: string[];
  roles?: string[];
  subscribers?: boolean;
  contacts?: RuleContact[];
  dedupeWindowMinutes?: string;
  digest?: string;
  maxPerHour?: string;
  webhookUrl?: string;
  webhookSecret?: string;
  /** configurationJson keys other than webhookUrl/webhookSecret, preserved on edit. */
  extraConfiguration?: Record<string, unknown>;
}

/** JSON contract stored in recipientSelector. */
interface RecipientSelector {
  roles?: string[];
  subscribers?: boolean;
  contacts?: RuleContact[];
}

interface Throttling {
  dedupeWindowMinutes?: number;
  digest?: string;
  maxPerHour?: number;
}

/** Expands a stored rule into structured dialog values ('' channelsJson → none). */
export function ruleToFormValues(rule: NotificationRule): NotificationRuleFormValues {
  const channels = parseJson<string[]>(rule.channelsJson, []);
  const recipients = parseJson<RecipientSelector>(rule.recipientSelector, {});
  const throttling = parseJson<Throttling>(rule.throttlingJson, {});
  const configuration = parseJson<Record<string, unknown>>(rule.configurationJson, {});
  const { webhookUrl, webhookSecret, ...extraConfiguration } = configuration;
  return {
    notificationRuleId: rule.notificationRuleId,
    accountId: rule.accountId,
    ruleKey: rule.ruleKey,
    ruleType: rule.ruleType,
    enabled: rule.enabled,
    triggerEvent: rule.triggerEvent,
    channels: Array.isArray(channels) ? channels : [],
    roles: Array.isArray(recipients.roles) ? recipients.roles : [],
    // Absent means "notify subscribers" (the engine default) — keep it checked.
    subscribers: recipients.subscribers !== false,
    contacts: Array.isArray(recipients.contacts) ? recipients.contacts : [],
    dedupeWindowMinutes:
      throttling.dedupeWindowMinutes != null ? String(throttling.dedupeWindowMinutes) : '',
    digest: throttling.digest || 'None',
    maxPerHour: throttling.maxPerHour != null ? String(throttling.maxPerHour) : '',
    webhookUrl: typeof webhookUrl === 'string' ? webhookUrl : '',
    webhookSecret: typeof webhookSecret === 'string' ? webhookSecret : '',
    extraConfiguration,
  };
}

/** Serializes the structured dialog values back into the rule's JSON columns. */
export function formValuesToRuleInput(
  values: NotificationRuleFormValues,
  accountId: string
): NotificationRuleDtoInput {
  const channels = values.channels ?? [];

  const recipientSelector: RecipientSelector = { subscribers: values.subscribers !== false };
  if (values.roles?.length) recipientSelector.roles = values.roles;
  const contacts = (values.contacts ?? []).filter(contact => contact.address.trim() !== '');
  if (contacts.length) recipientSelector.contacts = contacts;

  const throttling: Throttling = {};
  if (values.dedupeWindowMinutes && !Number.isNaN(Number(values.dedupeWindowMinutes))) {
    throttling.dedupeWindowMinutes = Number(values.dedupeWindowMinutes);
  }
  if (values.digest && values.digest !== 'None') throttling.digest = values.digest;
  if (values.maxPerHour && !Number.isNaN(Number(values.maxPerHour))) {
    throttling.maxPerHour = Number(values.maxPerHour);
  }

  // Preserve unrelated configuration keys; own the webhook pair (drop it when
  // the Webhook channel is deselected so stale endpoints are not kept).
  const configuration: Record<string, unknown> = { ...(values.extraConfiguration ?? {}) };
  if (channels.includes('Webhook')) {
    if (values.webhookUrl) configuration.webhookUrl = values.webhookUrl;
    if (values.webhookSecret) configuration.webhookSecret = values.webhookSecret;
  }

  // validate() gates the required fields; assert the mutation input at the boundary.
  return {
    accountId,
    ruleKey: values.ruleKey,
    ruleType: values.ruleType,
    enabled: values.enabled !== false,
    triggerEvent: values.triggerEvent,
    recipientSelector: JSON.stringify(recipientSelector),
    channelsJson: JSON.stringify(channels),
    throttlingJson: Object.keys(throttling).length ? JSON.stringify(throttling) : null,
    configurationJson: Object.keys(configuration).length ? JSON.stringify(configuration) : null,
  } as NotificationRuleDtoInput;
}

interface NotificationRuleDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: NotificationRuleFormValues;
  handleChange: FormChangeHandler;
  setValues: Dispatch<SetStateAction<NotificationRuleFormValues>>;
  errors: Record<string, string>;
  emailEnabled: boolean;
  whatsAppEnabled: boolean;
}

function NotificationRuleDialog({
  open,
  setOpen,
  handleSubmit,
  values,
  handleChange,
  setValues,
  errors,
  emailEnabled,
  whatsAppEnabled,
}: NotificationRuleDialogProps) {
  const { t } = useTranslation();

  const channelVisible = (channel: string): boolean => {
    if (channel === 'Email') return emailEnabled;
    if (channel === 'WhatsApp') return whatsAppEnabled;
    return true;
  };

  const availableChannels = NOTIFICATION_CHANNELS.filter(channelVisible);
  const contactChannels = (['Email', 'WhatsApp'] as const).filter(channelVisible);

  const toggleChannel = (channel: string, checked: boolean) => {
    setValues(prev => {
      const selected = new Set(prev.channels ?? []);
      if (checked) selected.add(channel);
      else selected.delete(channel);
      return { ...prev, channels: NOTIFICATION_CHANNELS.filter(item => selected.has(item)) };
    });
  };

  const toggleRole = (role: string, checked: boolean) => {
    setValues(prev => {
      const selected = new Set(prev.roles ?? []);
      if (checked) selected.add(role);
      else selected.delete(role);
      return { ...prev, roles: RECIPIENT_ROLES.filter(item => selected.has(item)) };
    });
  };

  const addContact = () => {
    const defaultChannel = contactChannels[0] ?? 'Email';
    setValues(prev => ({
      ...prev,
      contacts: [...(prev.contacts ?? []), { channel: defaultChannel, address: '' }],
    }));
  };

  const updateContact = (index: number, patch: Partial<RuleContact>) => {
    setValues(prev => ({
      ...prev,
      contacts: (prev.contacts ?? []).map((contact, i) =>
        i === index ? { ...contact, ...patch } : contact
      ),
    }));
  };

  const removeContact = (index: number) => {
    setValues(prev => ({
      ...prev,
      contacts: (prev.contacts ?? []).filter((_, i) => i !== index),
    }));
  };

  const eventOptions = ALERT_EVENT_TYPES.map(type => ({
    value: type,
    label: t(`alertEventTypes.${toCamelCase(type)}` as 'alertEventTypes.geofenceEntered', {
      defaultValue: type,
    }),
  }));

  const digestOptions = DIGEST_MODES.map(mode => ({
    value: mode,
    label: t(`notificationRules.digestModes.${toCamelCase(mode)}` as 'notificationRules.digestModes.none', {
      defaultValue: mode,
    }),
  }));

  const channelLabel = (channel: string) =>
    t(`notificationChannels.${toCamelCase(channel)}` as 'notificationChannels.inApp', {
      defaultValue: channel,
    });

  const roleLabel = (role: string) =>
    t(`roles.${toCamelCase(role)}` as 'roles.administrator', { defaultValue: role });

  return (
    <FormDialog
      title={values.notificationRuleId ? t('notificationRules.update') : t('notificationRules.create')}
      handleSave={handleSubmit}
      open={open}
      setOpen={setOpen}
      maxWidth="md">
      <form>
        <CustomTextField
          autoFocus
          margin="dense"
          name="ruleKey"
          id="ruleKey"
          label={t('notificationRules.key')}
          type="text"
          fullWidth
          value={values.ruleKey || ''}
          onChange={handleChange}
          required
          errorMsg={errors.ruleKey}
        />
        <CustomTextField
          margin="normal"
          name="ruleType"
          id="ruleType"
          label={t('notificationRules.type')}
          type="text"
          fullWidth
          value={values.ruleType || ''}
          onChange={handleChange}
          required
          errorMsg={errors.ruleType}
        />
        <CustomSelect
          list={eventOptions}
          name="triggerEvent"
          id="triggerEvent"
          label={t('notificationRules.triggerEvent')}
          value={values.triggerEvent || ''}
          handleChange={handleChange}
          numericValue={false}
          required
        />
        {errors.triggerEvent && (
          <ArgonTypography variant="caption" color="error">
            {errors.triggerEvent}
          </ArgonTypography>
        )}

        <ArgonBox mt={2}>
          <ArgonTypography variant="button" fontWeight="medium">
            {t('notificationRules.channels')}
          </ArgonTypography>
          <ArgonBox display="flex" flexWrap="wrap" gap={1}>
            {availableChannels.map(channel => (
              <CustomCheckbox
                key={channel}
                name={`channel_${channel}`}
                id={`channel_${channel}`}
                value={(values.channels ?? []).includes(channel)}
                handleChange={(event) => toggleChannel(channel, !!event.target.checked)}
                label={channelLabel(channel)}
              />
            ))}
          </ArgonBox>
        </ArgonBox>

        {(values.channels ?? []).includes('Webhook') && (
          <>
            <CustomTextField
              margin="normal"
              name="webhookUrl"
              id="webhookUrl"
              label={t('notificationRules.webhookUrl')}
              type="text"
              fullWidth
              value={values.webhookUrl || ''}
              onChange={handleChange}
              errorMsg={errors.webhookUrl}
            />
            <CustomTextField
              margin="normal"
              name="webhookSecret"
              id="webhookSecret"
              label={t('notificationRules.webhookSecret')}
              type="text"
              fullWidth
              value={values.webhookSecret || ''}
              onChange={handleChange}
            />
          </>
        )}

        <ArgonBox mt={2}>
          <ArgonTypography variant="button" fontWeight="medium">
            {t('notificationRules.recipients')}
          </ArgonTypography>
          <ArgonBox display="flex" flexWrap="wrap" gap={1}>
            {RECIPIENT_ROLES.map(role => (
              <CustomCheckbox
                key={role}
                name={`role_${role}`}
                id={`role_${role}`}
                value={(values.roles ?? []).includes(role)}
                handleChange={(event) => toggleRole(role, !!event.target.checked)}
                label={roleLabel(role)}
              />
            ))}
            <CustomCheckbox
              name="subscribers"
              id="subscribers"
              value={values.subscribers !== false}
              handleChange={handleChange}
              label={t('notificationRules.subscribers')}
            />
          </ArgonBox>
        </ArgonBox>

        <ArgonBox mt={1}>
          <ArgonTypography variant="caption" fontWeight="medium">
            {t('notificationRules.contacts')}
          </ArgonTypography>
          {(values.contacts ?? []).map((contact, index) => (
            <ArgonBox key={index} display="flex" gap={1} alignItems="center">
              <ArgonBox minWidth="10rem">
                <CustomSelect
                  list={contactChannels.map(channel => ({
                    value: channel,
                    label: channelLabel(channel),
                  }))}
                  name={`contactChannel_${index}`}
                  id={`contactChannel_${index}`}
                  value={contact.channel}
                  handleChange={(event) =>
                    updateContact(index, { channel: String(event.target.value) as RuleContact['channel'] })
                  }
                  numericValue={false}
                />
              </ArgonBox>
              <CustomTextField
                margin="none"
                name={`contactAddress_${index}`}
                id={`contactAddress_${index}`}
                placeholder={t('notificationRules.contactAddress')}
                type="text"
                fullWidth
                value={contact.address}
                onChange={(event) => updateContact(index, { address: event.target.value })}
              />
              <IconButton
                size="small"
                color="error"
                onClick={() => removeContact(index)}
                aria-label={t('generic.delete')}
              >
                <Icon>delete</Icon>
              </IconButton>
            </ArgonBox>
          ))}
          <ArgonButton variant="text" color="info" size="small" onClick={addContact} disabled={contactChannels.length === 0}>
            <Icon>add</Icon>&nbsp;{t('notificationRules.addContact')}
          </ArgonButton>
        </ArgonBox>

        <ArgonBox mt={2}>
          <ArgonTypography variant="button" fontWeight="medium">
            {t('notificationRules.throttling')}
          </ArgonTypography>
          <ArgonBox display="flex" gap={2} flexWrap="wrap">
            <CustomTextField
              margin="none"
              name="dedupeWindowMinutes"
              id="dedupeWindowMinutes"
              label={t('notificationRules.dedupeWindowMinutes')}
              type="number"
              fullWidth={false}
              value={values.dedupeWindowMinutes || ''}
              onChange={handleChange}
            />
            <ArgonBox minWidth="10rem">
              <CustomSelect
                list={digestOptions}
                name="digest"
                id="digest"
                label={t('notificationRules.digest')}
                value={values.digest || 'None'}
                handleChange={handleChange}
                numericValue={false}
                fullWidth={false}
              />
            </ArgonBox>
            <CustomTextField
              margin="none"
              name="maxPerHour"
              id="maxPerHour"
              label={t('notificationRules.maxPerHour')}
              type="number"
              fullWidth={false}
              value={values.maxPerHour || ''}
              onChange={handleChange}
            />
          </ArgonBox>
        </ArgonBox>

        <CustomCheckbox
          name="enabled"
          id="enabled"
          value={values.enabled}
          handleChange={handleChange}
          label={t('notificationRules.enabled')} />
      </form>
    </FormDialog>
  );
}

export default NotificationRuleDialog;
