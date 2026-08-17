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

import { useContext, useEffect, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import Icon from '@mui/material/Icon';
import Table from "controls/Tables/Table";
import TableAccordion from "controls/Accordions/TableAccordion";
import ArgonBadge from "components/ArgonBadge";
import ArgonButton from "components/ArgonButton";
import ArgonTypography from "components/ArgonTypography";
import ConfirmDialog from "controls/Dialogs/ConfirmDialog";
import useForm from "controls/Dialogs/useForm";
import NotificationTemplateDialog from "layouts/manageadmin/components/notificationTemplates/NotificationTemplateDialog";
import type { NotificationTemplateFormValues } from "layouts/manageadmin/components/notificationTemplates/NotificationTemplateDialog";
import { getAccountByUser } from "api/manager/accounts";
import type { Account } from "api/manager/accounts";
import { getAccountFeatures } from "api/manager/accountFeatures";
import {
  getNotificationTemplates,
  createNotificationTemplate,
  updateNotificationTemplate,
  deleteNotificationTemplate,
} from "api/manager/notificationTemplates";
import type { NotificationTemplate, NotificationTemplateDtoInput } from "api/manager/notificationTemplates";
import { notifyApiError } from "api/core/errors";
import { LoadingContext } from 'LoadingContext';
import { toCamelCase } from "utils/stringUtils";
import { NOTIFICATIONS_FEATURE_KEY } from 'utils/notificationsCatalog';

interface ConfirmState { open: boolean; id: string | null; }

function TextCell({ children }: { children?: ReactNode }) {
  return (
    <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
      {children || '-'}
    </ArgonTypography>
  );
}

function ManageNotificationTemplates() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [account, setAccount] = useState<Account | null>(null);
  const [templates, setTemplates] = useState<NotificationTemplate[]>([]);
  const [notificationsEnabled, setNotificationsEnabled] = useState(false);
  const [open, setOpen] = useState(false);
  const [confirm, setConfirm] = useState<ConfirmState>({ open: false, id: null });
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<NotificationTemplateFormValues>({ active: true });
  const loaded = useRef(false);

  const loadTemplates = async () => {
    setLoading(true);
    try {
      const currentAccount = await getAccountByUser();
      if (!currentAccount?.accountId) return;
      setAccount(currentAccount);
      const [items, features] = await Promise.all([
        getNotificationTemplates(currentAccount.accountId),
        // The feature only gates UI affordances — the backend is authoritative.
        getAccountFeatures(currentAccount.accountId).catch(() => []),
      ]);
      setTemplates(items || []);
      setNotificationsEnabled(
        !!(features || []).find(f => f.featureKey === NOTIFICATIONS_FEATURE_KEY)?.enabled
      );
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (expanded && !loaded.current) {
      loaded.current = true;
      loadTemplates();
    }
  }, [expanded]);

  const handleAddClick = () => {
    setValues({ active: true, locale: 'en' });
    setErrors({});
  };

  const handleEdit = (template: NotificationTemplate) => {
    setValues({
      notificationTemplateId: template.notificationTemplateId,
      templateKey: template.templateKey,
      channel: template.channel,
      locale: template.locale,
      subject: template.subject || '',
      body: template.body,
      active: template.active,
    });
    setErrors({});
    setOpen(true);
  };

  const handleSubmit = async () => {
    if (!validate(['templateKey', 'channel', 'locale', 'body']) || !account?.accountId) return;
    setLoading(true);
    try {
      // validate() gates the required fields; assert the mutation input at the boundary.
      // Account overrides always carry the accountId (platform defaults are read-only).
      const template = {
        accountId: account.accountId,
        templateKey: values.templateKey,
        channel: values.channel,
        locale: values.locale,
        subject: (values.subject || '').trim() || null,
        body: values.body,
        active: values.active !== false,
      } as NotificationTemplateDtoInput;
      if (values.notificationTemplateId) {
        await updateNotificationTemplate(values.notificationTemplateId, template);
      } else {
        await createNotificationTemplate(template);
      }
      setOpen(false);
      await loadTemplates();
    } catch (error) {
      // Keep the dialog open on failure so the user can retry without re-entering.
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    const id = confirm.id;
    setConfirm({ open: false, id: null });
    if (!id) return;
    setLoading(true);
    try {
      await deleteNotificationTemplate(id);
      await loadTemplates();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <TableAccordion
        title={t('notificationTemplates.title')}
        showAddIcon={notificationsEnabled}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <Table
          columns={[
            { name: 'templateKey', title: t('notificationTemplates.templateKey'), align: 'left' },
            { name: 'channel', title: t('notificationTemplates.channel'), align: 'center' },
            { name: 'locale', title: t('notificationTemplates.locale'), align: 'center' },
            { name: 'scope', title: t('notificationTemplates.scope'), align: 'center' },
            { name: 'active', title: t('notificationTemplates.active'), align: 'center' },
            { name: 'action', title: t('generic.action'), align: 'center' },
            { name: 'id' }
          ]}
          rows={templates.map(template => {
            const isDefault = !template.accountId;
            // Alert-event keys reuse the shared alertEventTypes labels; only the
            // two system template keys have their own labels.
            const keyLabel =
              template.templateKey === 'TestNotification' || template.templateKey === 'NotificationDigest'
                ? t(`notificationTemplates.keys.${toCamelCase(template.templateKey)}` as 'notificationTemplates.keys.testNotification', {
                    defaultValue: template.templateKey,
                  })
                : t(`alertEventTypes.${toCamelCase(template.templateKey)}` as 'alertEventTypes.geofenceEntered', {
                    defaultValue: template.templateKey,
                  });
            return {
              templateKey: <TextCell>{keyLabel}</TextCell>,
              channel: (
                <TextCell>
                  {t(`notificationChannels.${toCamelCase(template.channel)}` as 'notificationChannels.inApp', {
                    defaultValue: template.channel,
                  })}
                </TextCell>
              ),
              locale: <TextCell>{template.locale}</TextCell>,
              scope: (
                <ArgonBadge
                  variant="gradient"
                  color={isDefault ? 'secondary' : 'info'}
                  size="xs"
                  container
                  badgeContent={isDefault ? t('notificationTemplates.default') : t('notificationTemplates.custom')}
                />
              ),
              active: <TextCell>{template.active ? t('generic.yes') : t('generic.no')}</TextCell>,
              // Platform defaults (accountId == null) are read-only.
              action: !isDefault && notificationsEnabled ? (
                <>
                  <ArgonButton variant="text" color="dark" onClick={() => handleEdit(template)}>
                    <Icon>edit</Icon>&nbsp;{t('generic.edit')}
                  </ArgonButton>
                  <ArgonButton
                    variant="text"
                    color="error"
                    onClick={() => setConfirm({ open: true, id: template.notificationTemplateId })}>
                    <Icon>delete</Icon>&nbsp;{t('generic.delete')}
                  </ArgonButton>
                </>
              ) : null,
              id: template.notificationTemplateId
            };
          })}
          selectedField="templateKey"
        />
      </TableAccordion>
      <NotificationTemplateDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        errors={errors}
      />
      <ConfirmDialog
        open={confirm.open}
        setOpen={(value) => setConfirm(prev => ({ ...prev, open: typeof value === 'function' ? value(prev.open) : value }))}
        title={t('notificationTemplates.deleteTitle')}
        message={t('notificationTemplates.deleteMessage')}
        onConfirm={handleDelete}
      />
    </>
  );
}

export default ManageNotificationTemplates;
