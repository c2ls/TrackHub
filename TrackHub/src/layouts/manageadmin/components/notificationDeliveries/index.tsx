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
import Grid from '@mui/material/Grid';
import Table from "controls/Tables/Table";
import TableAccordion from "controls/Accordions/TableAccordion";
import ArgonBox from "components/ArgonBox";
import ArgonButton from "components/ArgonButton";
import ArgonBadge from "components/ArgonBadge";
import ArgonTypography from "components/ArgonTypography";
import CustomSelect from 'controls/Dialogs/CustomSelect';
import DetailedStatisticsCard from "controls/Cards/StatisticsCards/DetailedStatisticsCard";
import useForm from "controls/Dialogs/useForm";
import { getAccountByUser } from "api/manager/accounts";
import type { Account } from "api/manager/accounts";
import {
  getNotificationDeliveries,
  getDeliveryHealth,
  retryNotificationDelivery,
} from "api/manager/notificationDeliveries";
import type { NotificationDelivery, DeliveryHealth } from "api/manager/notificationDeliveries";
import { notifyApiError } from "api/core/errors";
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from "utils/dateUtils";
import { toCamelCase } from "utils/stringUtils";
import { DELIVERY_STATUSES, NOTIFICATION_CHANNELS } from 'utils/notificationsCatalog';

const ALL = 'all';
const HEALTH_WINDOW_DAYS = 7;

interface FilterValues { status?: string; channel?: string; }

const statusColor = (status: string): 'success' | 'error' | 'warning' | 'secondary' => {
  const normalized = (status || '').toLowerCase();
  if (normalized === 'sent' || normalized === 'digested') return 'success';
  if (normalized === 'failed') return 'error';
  if (normalized === 'deferred' || normalized === 'sending') return 'warning';
  return 'secondary';
};

function TextCell({ children }: { children?: ReactNode }) {
  return (
    <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
      {children || '-'}
    </ArgonTypography>
  );
}

function ManageNotificationDeliveries() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [account, setAccount] = useState<Account | null>(null);
  const [deliveries, setDeliveries] = useState<NotificationDelivery[]>([]);
  const [health, setHealth] = useState<DeliveryHealth[]>([]);
  const [filters, handleFilterChange] = useForm<FilterValues>({ status: ALL, channel: ALL });
  const loaded = useRef(false);

  const loadDeliveries = async (currentFilters: FilterValues = filters) => {
    setLoading(true);
    try {
      const currentAccount = account ?? await getAccountByUser();
      if (!currentAccount?.accountId) return;
      setAccount(currentAccount);
      const to = new Date();
      const from = new Date(to.getTime() - HEALTH_WINDOW_DAYS * 24 * 60 * 60 * 1000);
      const [items, healthItems] = await Promise.all([
        getNotificationDeliveries(currentAccount.accountId, {
          status: currentFilters.status && currentFilters.status !== ALL ? currentFilters.status : null,
          channel: currentFilters.channel && currentFilters.channel !== ALL ? currentFilters.channel : null,
        }),
        // A failed health read keeps the tiles at zero instead of failing the panel.
        getDeliveryHealth(currentAccount.accountId, from.toISOString(), to.toISOString())
          .catch((): DeliveryHealth[] => []),
      ]);
      setDeliveries(items || []);
      setHealth(healthItems || []);
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (expanded && !loaded.current) {
      loaded.current = true;
      loadDeliveries();
    }
  }, [expanded]);

  const handleRetry = async (delivery: NotificationDelivery) => {
    if (!delivery?.notificationDeliveryId) return;
    setLoading(true);
    try {
      await retryNotificationDelivery(delivery.notificationDeliveryId);
      await loadDeliveries();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const healthCount = (status: string): number =>
    health
      .filter(item => (item.status || '').toLowerCase() === status)
      .reduce((total, item) => total + item.count, 0);

  const statusOptions = [
    { value: ALL, label: t('notificationDeliveries.allStatuses') },
    ...DELIVERY_STATUSES.map(status => ({
      value: status,
      label: t(`notificationDeliveries.statuses.${toCamelCase(status)}` as 'notificationDeliveries.statuses.sent', {
        defaultValue: status,
      }),
    })),
  ];

  const channelOptions = [
    { value: ALL, label: t('notificationDeliveries.allChannels') },
    ...NOTIFICATION_CHANNELS.map(channel => ({
      value: channel,
      label: t(`notificationChannels.${toCamelCase(channel)}` as 'notificationChannels.inApp', {
        defaultValue: channel,
      }),
    })),
  ];

  return (
    <TableAccordion
      title={t('notificationDeliveries.title')}
      expanded={expanded}
      setExpanded={setExpanded}>
      <Grid container spacing={3} mb={2}>
        <Grid size={{ xs: 12, md: 4 }}>
          <DetailedStatisticsCard
            title={t('notificationDeliveries.sentCount')}
            count={healthCount('sent')}
            icon={{ color: "success", component: <i className="ni ni-send" /> }}
            percentage={{ color: "success", count: "", hide: true }}
          />
        </Grid>
        <Grid size={{ xs: 12, md: 4 }}>
          <DetailedStatisticsCard
            title={t('notificationDeliveries.failedCount')}
            count={healthCount('failed')}
            icon={{ color: "error", component: <i className="ni ni-fat-remove" /> }}
            percentage={{ color: "success", count: "", hide: true }}
          />
        </Grid>
        <Grid size={{ xs: 12, md: 4 }}>
          <DetailedStatisticsCard
            title={t('notificationDeliveries.pendingCount')}
            count={healthCount('pending')}
            icon={{ color: "warning", component: <i className="ni ni-time-alarm" /> }}
            percentage={{ color: "success", count: "", hide: true }}
          />
        </Grid>
      </Grid>
      <ArgonBox display="flex" gap={2} mb={1} alignItems="flex-end" flexWrap="wrap">
        <ArgonBox minWidth="12rem">
          <CustomSelect
            list={statusOptions}
            name="status"
            id="filterStatus"
            label={t('notificationDeliveries.filterStatus')}
            value={filters.status || ALL}
            handleChange={handleFilterChange}
            numericValue={false}
            fullWidth={false}
          />
        </ArgonBox>
        <ArgonBox minWidth="12rem">
          <CustomSelect
            list={channelOptions}
            name="channel"
            id="filterChannel"
            label={t('notificationDeliveries.filterChannel')}
            value={filters.channel || ALL}
            handleChange={handleFilterChange}
            numericValue={false}
            fullWidth={false}
          />
        </ArgonBox>
        <ArgonButton color="primary" size="small" onClick={() => loadDeliveries()}>
          <Icon>search</Icon>
        </ArgonButton>
      </ArgonBox>
      <Table
        columns={[
          { name: 'channel', title: t('notificationDeliveries.channel'), align: 'left' },
          { name: 'recipient', title: t('notificationDeliveries.recipient'), align: 'left' },
          { name: 'status', title: t('notificationDeliveries.status'), align: 'center' },
          { name: 'attempts', title: t('notificationDeliveries.attempts'), align: 'center' },
          { name: 'error', title: t('notificationDeliveries.error'), align: 'left' },
          { name: 'sentAt', title: t('notificationDeliveries.sentAt'), align: 'center' },
          { name: 'readAt', title: t('notificationDeliveries.readAt'), align: 'center' },
          { name: 'action', title: t('generic.action'), align: 'center' },
          { name: 'id' }
        ]}
        rows={deliveries.map(delivery => ({
          channel: (
            <TextCell>
              {t(`notificationChannels.${toCamelCase(delivery.channel)}` as 'notificationChannels.inApp', {
                defaultValue: delivery.channel,
              })}
            </TextCell>
          ),
          recipient: <TextCell>{delivery.recipient}</TextCell>,
          status: (
            <ArgonBadge
              variant="gradient"
              color={statusColor(delivery.status)}
              size="xs"
              container
              badgeContent={t(`notificationDeliveries.statuses.${toCamelCase(delivery.status)}` as 'notificationDeliveries.statuses.sent', {
                defaultValue: delivery.status,
              })}
            />
          ),
          attempts: <TextCell>{delivery.attempts}</TextCell>,
          error: <TextCell>{delivery.error}</TextCell>,
          sentAt: <TextCell>{delivery.sentAt ? formatDateTime(delivery.sentAt) : '-'}</TextCell>,
          readAt: <TextCell>{delivery.readAt ? formatDateTime(delivery.readAt) : '-'}</TextCell>,
          action: delivery.status === 'Failed' ? (
            <ArgonButton variant="text" color="warning" onClick={() => handleRetry(delivery)}>
              <Icon>replay</Icon>&nbsp;{t('notificationDeliveries.retry')}
            </ArgonButton>
          ) : null,
          id: delivery.notificationDeliveryId
        }))}
        selectedField="recipient"
      />
    </TableAccordion>
  );
}

export default ManageNotificationDeliveries;
