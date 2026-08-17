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
import { getAccountByUser } from "api/manager/accounts";
import { getAlertEvents, acknowledgeAlertEvent, resolveAlertEvent } from "api/manager/alertEvents";
import type { AlertEvent } from "api/manager/alertEvents";
import { notifyApiError } from "api/core/errors";
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from "utils/dateUtils";

function TextCell({ children }: { children?: ReactNode }) {
  return (
    <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
      {children || '-'}
    </ArgonTypography>
  );
}

function ManageAlertEvents() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [alerts, setAlerts] = useState<AlertEvent[]>([]);
  const loaded = useRef(false);

  const loadAlerts = async () => {
    setLoading(true);
    try {
      const account = await getAccountByUser();
      if (!account?.accountId) return;
      const items = await getAlertEvents(account.accountId);
      setAlerts(items || []);
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (expanded && !loaded.current) {
      loaded.current = true;
      loadAlerts();
    }
  }, [expanded]);

  const handleAcknowledge = async (alert: AlertEvent) => {
    if (!alert?.alertEventId) return;
    setLoading(true);
    try {
      await acknowledgeAlertEvent(alert.alertEventId);
      await loadAlerts();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const handleResolve = async (alert: AlertEvent) => {
    if (!alert?.alertEventId) return;
    setLoading(true);
    try {
      await resolveAlertEvent(alert.alertEventId);
      await loadAlerts();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <TableAccordion title={t('alertEvents.title')} expanded={expanded} setExpanded={setExpanded}>
      <Table
        columns={[
          { name: 'type', title: t('alertEvents.type'), align: 'left' },
          { name: 'status', title: t('alertEvents.status'), align: 'center' },
          { name: 'modified', title: t('generic.modified'), align: 'center' },
          { name: 'action', title: t('generic.action'), align: 'center' },
          { name: 'id' }
        ]}
        rows={alerts.map(alert => ({
          type: <TextCell>{alert.eventType}</TextCell>,
          status: <TextCell>{alert.status}</TextCell>,
          modified: <TextCell>{formatDateTime(alert.lastSeenAt)}</TextCell>,
          action: (
            <>
              {alert.status !== 'acknowledged' && alert.status !== 'resolved' && (
                <ArgonButton variant="text" color="dark" onClick={() => handleAcknowledge(alert)}>
                  <Icon>done</Icon>&nbsp;{t('alertEvents.acknowledge')}
                </ArgonButton>
              )}
              {alert.status !== 'resolved' && (
                <ArgonButton variant="text" color="success" onClick={() => handleResolve(alert)}>
                  <Icon>done_all</Icon>&nbsp;{t('alertEvents.resolve')}
                </ArgonButton>
              )}
            </>
          ),
          id: alert.alertEventId
        }))}
        selectedField="type"
      />
    </TableAccordion>
  );
}

export default ManageAlertEvents;
