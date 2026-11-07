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

import { useContext, useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import Icon from '@mui/material/Icon';
import Table from "controls/Tables/Table";
import ServerPagination from "controls/Tables/ServerPagination";
import { useClampPage, useServerList } from "controls/Tables/useServerList";
import TableAccordion from "controls/Accordions/TableAccordion";
import ArgonBadge from "components/ArgonBadge";
import ArgonButton from "components/ArgonButton";
import ArgonTypography from "components/ArgonTypography";
import ConfirmDialog from "controls/Dialogs/ConfirmDialog";
import useForm from "controls/Dialogs/useForm";
import AlertSubscriptionDialog, { ALL_EVENTS } from "layouts/manageadmin/components/alertSubscriptions/AlertSubscriptionDialog";
import type { AlertSubscriptionFormValues } from "layouts/manageadmin/components/alertSubscriptions/AlertSubscriptionDialog";
import { getAccountByUser } from "api/manager/accounts";
import type { Account } from "api/manager/accounts";
import { useFeatures } from "context/features";
import {
  getAlertSubscriptions,
  createAlertSubscription,
  updateAlertSubscription,
  deleteAlertSubscription,
} from "api/manager/alertSubscriptions";
import type { AlertSubscription, AlertSubscriptionDtoInput } from "api/manager/alertSubscriptions";
import { notifyApiError } from "api/core/errors";
import { LoadingContext } from 'LoadingContext';
import { validateEmail } from 'utils/validationUtils';
import { toCamelCase } from 'utils/stringUtils';
import {
  NOTIFICATIONS_FEATURE_KEY,
  NOTIFICATIONS_EMAIL_FEATURE_KEY,
  NOTIFICATIONS_WHATSAPP_FEATURE_KEY,
} from 'utils/notificationsCatalog';

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
// Light phone check for WhatsApp contacts: optional +, 7–15 digits.
const PHONE_PATTERN = /^\+?\d{7,15}$/;

interface ConfirmState { open: boolean; id: string | null; }

function TextCell({ children }: { children?: ReactNode }) {
  return (
    <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
      {children || '-'}
    </ArgonTypography>
  );
}

const PAGE_SIZE = 25;

function ManageAlertSubscriptions() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [account, setAccount] = useState<Account | null>(null);
  const [subscriptions, setSubscriptions] = useState<AlertSubscription[]>([]);
  const { isFeatureEnabled } = useFeatures();
  const notificationsEnabled = isFeatureEnabled(NOTIFICATIONS_FEATURE_KEY);
  const emailEnabled = isFeatureEnabled(NOTIFICATIONS_EMAIL_FEATURE_KEY);
  const whatsAppEnabled = isFeatureEnabled(NOTIFICATIONS_WHATSAPP_FEATURE_KEY);
  const [open, setOpen] = useState(false);
  const [confirm, setConfirm] = useState<ConfirmState>({ open: false, id: null });
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<AlertSubscriptionFormValues>({ enabled: true });
  const [totalCount, setTotalCount] = useState(0);
  const [pageLength, setPageLength] = useState(0);
  const { page, setPage, params } = useServerList(PAGE_SIZE);
  useClampPage(page, PAGE_SIZE, totalCount, setPage);

  const loadSubscriptions = async () => {
    setLoading(true);
    try {
      const currentAccount = await getAccountByUser();
      if (!currentAccount?.accountId) return;
      setAccount(currentAccount);
      const items = await getAlertSubscriptions(currentAccount.accountId, null, params.skip, params.take);
      setSubscriptions(items.items);
      setTotalCount(items.totalCount);
      setPageLength(items.items.length);
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (expanded) {
      loadSubscriptions();
    }
  }, [expanded, params]);

  const handleAddClick = () => {
    setValues({ enabled: true, eventTypeFilter: ALL_EVENTS });
    setErrors({});
  };

  const handleEdit = (subscription: AlertSubscription) => {
    setValues({
      alertSubscriptionId: subscription.alertSubscriptionId,
      principalType: subscription.principalType,
      principalId: subscription.principalId,
      eventTypeFilter: subscription.eventTypeFilter || ALL_EVENTS,
      channel: subscription.channel,
      contact: subscription.contact || '',
      enabled: subscription.enabled,
    });
    setErrors({});
    setOpen(true);
  };

  const validateExtra = (): boolean => {
    const extra: Record<string, string> = {};
    if (values.principalId && !GUID_PATTERN.test(values.principalId.trim())) {
      extra.principalId = t('alertSubscriptions.invalidPrincipalId');
    }
    const contact = (values.contact || '').trim();
    if (contact) {
      if (values.channel === 'Email' && !validateEmail(contact)) {
        extra.contact = t('alertSubscriptions.invalidContact');
      }
      if (values.channel === 'WhatsApp' && !PHONE_PATTERN.test(contact)) {
        extra.contact = t('alertSubscriptions.invalidContact');
      }
    }
    if (Object.keys(extra).length) {
      setErrors(extra);
      return false;
    }
    return true;
  };

  const handleSubmit = async () => {
    if (!validate(['principalType', 'principalId', 'channel']) || !validateExtra() || !account?.accountId) return;
    setLoading(true);
    try {
      // validate() gates the required fields; assert the mutation input at the boundary.
      const subscription = {
        accountId: account.accountId,
        principalType: values.principalType,
        principalId: values.principalId?.trim(),
        eventTypeFilter: values.eventTypeFilter && values.eventTypeFilter !== ALL_EVENTS
          ? values.eventTypeFilter
          : null,
        channel: values.channel,
        contact: (values.contact || '').trim() || null,
        enabled: values.enabled !== false,
      } as AlertSubscriptionDtoInput;
      if (values.alertSubscriptionId) {
        await updateAlertSubscription(values.alertSubscriptionId, subscription);
      } else {
        await createAlertSubscription(subscription);
      }
      setOpen(false);
      await loadSubscriptions();
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
      await deleteAlertSubscription(id);
      await loadSubscriptions();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const eventLabel = (eventTypeFilter?: string | null): string =>
    eventTypeFilter
      ? t(`alertEventTypes.${toCamelCase(eventTypeFilter)}` as 'alertEventTypes.geofenceEntered', {
          defaultValue: eventTypeFilter,
        })
      : t('alertSubscriptions.allEvents');

  return (
    <>
      <TableAccordion sectionKey="alert-subscriptions"
        title={t('alertSubscriptions.title')}
        showAddIcon={notificationsEnabled}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <Table
          columns={[
            { name: 'principalType', title: t('alertSubscriptions.principalType'), align: 'left' },
            { name: 'principalId', title: t('alertSubscriptions.principalId'), align: 'left' },
            { name: 'eventType', title: t('alertSubscriptions.eventType'), align: 'center' },
            { name: 'channel', title: t('alertSubscriptions.channel'), align: 'center' },
            { name: 'contact', title: t('alertSubscriptions.contact'), align: 'center' },
            { name: 'enabled', title: t('alertSubscriptions.enabled'), align: 'center' },
            { name: 'action', title: t('generic.action'), align: 'center' },
            { name: 'id' }
          ]}
          rows={subscriptions.map(subscription => ({
            principalType: (
              <TextCell>
                {subscription.principalType === 'Staff'
                  ? t('alertSubscriptions.staff')
                  : t('alertSubscriptions.user')}
              </TextCell>
            ),
            principalId: <TextCell>{(subscription.principalId || '').substring(0, 8)}</TextCell>,
            eventType: <TextCell>{eventLabel(subscription.eventTypeFilter)}</TextCell>,
            channel: (
              <TextCell>
                {t(`notificationChannels.${toCamelCase(subscription.channel)}` as 'notificationChannels.inApp', {
                  defaultValue: subscription.channel,
                })}
              </TextCell>
            ),
            contact: <TextCell>{subscription.contact}</TextCell>,
            enabled: (
              <ArgonBadge
                variant="gradient"
                color={subscription.enabled ? 'success' : 'secondary'}
                size="xs"
                container
                badgeContent={subscription.enabled ? t('generic.yes') : t('generic.no')}
              />
            ),
            action: (
              <>
                <ArgonButton variant="text" color="dark" onClick={() => handleEdit(subscription)}>
                  <Icon>edit</Icon>&nbsp;{t('generic.edit')}
                </ArgonButton>
                <ArgonButton
                  variant="text"
                  color="error"
                  onClick={() => setConfirm({ open: true, id: subscription.alertSubscriptionId })}>
                  <Icon>delete</Icon>&nbsp;{t('generic.delete')}
                </ArgonButton>
              </>
            ),
            id: subscription.alertSubscriptionId
          }))}
          selectedField="principalId"
          serverPaged
        />
        <ServerPagination page={page} pageSize={PAGE_SIZE} totalCount={totalCount} pageLength={pageLength} onPageChange={setPage} />
      </TableAccordion>
      <AlertSubscriptionDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        errors={errors}
        emailEnabled={emailEnabled}
        whatsAppEnabled={whatsAppEnabled}
      />
      <ConfirmDialog
        open={confirm.open}
        setOpen={(value) => setConfirm(prev => ({ ...prev, open: typeof value === 'function' ? value(prev.open) : value }))}
        title={t('alertSubscriptions.deleteTitle')}
        message={t('alertSubscriptions.deleteMessage')}
        onConfirm={handleDelete}
      />
    </>
  );
}

export default ManageAlertSubscriptions;
