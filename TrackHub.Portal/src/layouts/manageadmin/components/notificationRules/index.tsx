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
import ArgonButton from "components/ArgonButton";
import ArgonTypography from "components/ArgonTypography";
import useForm from "controls/Dialogs/useForm";
import NotificationRuleDialog, {
  ruleToFormValues,
  formValuesToRuleInput,
} from "layouts/manageadmin/components/notificationRules/NotificationRuleDialog";
import type { NotificationRuleFormValues } from "layouts/manageadmin/components/notificationRules/NotificationRuleDialog";
import { getAccountByUser } from "api/manager/accounts";
import type { Account } from "api/manager/accounts";
import { getAccountFeatures } from "api/manager/accountFeatures";
import {
  getNotificationRules,
  createNotificationRule,
  updateNotificationRule,
  disableNotificationRule,
} from "api/manager/notificationRules";
import type { NotificationRule } from "api/manager/notificationRules";
import { notifyApiError } from "api/core/errors";
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from "utils/dateUtils";
import {
  NOTIFICATIONS_FEATURE_KEY,
  NOTIFICATIONS_EMAIL_FEATURE_KEY,
  NOTIFICATIONS_WHATSAPP_FEATURE_KEY,
} from 'utils/notificationsCatalog';

function TextCell({ children }: { children?: ReactNode }) {
  return (
    <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
      {children || '-'}
    </ArgonTypography>
  );
}

function ManageNotificationRules() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [account, setAccount] = useState<Account | null>(null);
  const [rules, setRules] = useState<NotificationRule[]>([]);
  const [notificationsEnabled, setNotificationsEnabled] = useState(false);
  const [emailEnabled, setEmailEnabled] = useState(false);
  const [whatsAppEnabled, setWhatsAppEnabled] = useState(false);
  const [open, setOpen] = useState(false);
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<NotificationRuleFormValues>({ enabled: true });
  const loaded = useRef(false);

  const loadRules = async () => {
    setLoading(true);
    try {
      const currentAccount = await getAccountByUser();
      if (!currentAccount?.accountId) return;
      setAccount(currentAccount);
      const [items, features] = await Promise.all([
        getNotificationRules(currentAccount.accountId),
        // Channel entitlements only gate UI affordances — the backend is authoritative.
        getAccountFeatures(currentAccount.accountId).catch(() => []),
      ]);
      setRules(items || []);
      const enabled = (key: string) => !!(features || []).find(f => f.featureKey === key)?.enabled;
      setNotificationsEnabled(enabled(NOTIFICATIONS_FEATURE_KEY));
      setEmailEnabled(enabled(NOTIFICATIONS_EMAIL_FEATURE_KEY));
      setWhatsAppEnabled(enabled(NOTIFICATIONS_WHATSAPP_FEATURE_KEY));
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (expanded && !loaded.current) {
      loaded.current = true;
      loadRules();
    }
  }, [expanded]);

  const handleAddClick = () => {
    setValues({
      accountId: account?.accountId,
      enabled: true,
      subscribers: true,
      channels: [],
      roles: [],
      contacts: [],
      digest: 'None',
    });
    setErrors({});
  };

  const handleEdit = (rule: NotificationRule) => {
    setValues(ruleToFormValues(rule));
    setErrors({});
    setOpen(true);
  };

  const handleSubmit = async () => {
    if (!validate(['ruleKey', 'ruleType', 'triggerEvent']) || !account?.accountId) return;
    setLoading(true);
    try {
      const rule = formValuesToRuleInput(values, account.accountId);
      if (values.notificationRuleId) {
        await updateNotificationRule(values.notificationRuleId, rule);
      } else {
        await createNotificationRule(rule);
      }
      setOpen(false);
      await loadRules();
    } catch (error) {
      // Keep the dialog open on failure so the user can retry without re-entering.
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const handleDisable = async (rule: NotificationRule) => {
    if (!rule?.notificationRuleId) return;
    setLoading(true);
    try {
      await disableNotificationRule(rule.notificationRuleId);
      await loadRules();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <TableAccordion
        title={t('notificationRules.title')}
        showAddIcon={notificationsEnabled}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <Table
          columns={[
            { name: 'key', title: t('notificationRules.key'), align: 'left' },
            { name: 'type', title: t('notificationRules.type'), align: 'center' },
            { name: 'status', title: t('notificationRules.status'), align: 'center' },
            { name: 'modified', title: t('generic.modified'), align: 'center' },
            { name: 'action', title: t('generic.action'), align: 'center' },
            { name: 'id' }
          ]}
          rows={rules.map(rule => ({
            key: <TextCell>{rule.ruleKey}</TextCell>,
            type: <TextCell>{rule.ruleType}</TextCell>,
            status: <TextCell>{rule.enabled ? t('generic.yes') : t('generic.no')}</TextCell>,
            modified: <TextCell>{formatDateTime(rule.lastModified)}</TextCell>,
            action: (
              <>
                <ArgonButton variant="text" color="dark" onClick={() => handleEdit(rule)}>
                  <Icon>edit</Icon>&nbsp;{t('generic.edit')}
                </ArgonButton>
                {rule.enabled && (
                  <ArgonButton variant="text" color="error" onClick={() => handleDisable(rule)}>
                    <Icon>block</Icon>&nbsp;{t('notificationRules.disable')}
                  </ArgonButton>
                )}
              </>
            ),
            id: rule.notificationRuleId
          }))}
          selectedField="key"
        />
      </TableAccordion>
      <NotificationRuleDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        setValues={setValues}
        errors={errors}
        emailEnabled={emailEnabled}
        whatsAppEnabled={whatsAppEnabled}
      />
    </>
  );
}

export default ManageNotificationRules;
