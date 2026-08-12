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
import Table from "controls/Tables/Table";
import TableAccordion from "controls/Accordions/TableAccordion";
import ArgonTypography from "components/ArgonTypography";
import { getAccountByUser } from "api/manager/accounts";
import { getAuditTrail } from "api/manager/auditEvents";
import type { AuditEvent } from "api/manager/auditEvents";
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

function ManageAuditTrail() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [auditTrail, setAuditTrail] = useState<AuditEvent[]>([]);
  const loaded = useRef(false);

  useEffect(() => {
    if (!expanded || loaded.current) return;
    loaded.current = true;

    async function loadAuditTrail() {
      setLoading(true);
      try {
        const account = await getAccountByUser();
        if (!account?.accountId) return;
        const items = await getAuditTrail(account.accountId);
        setAuditTrail(items || []);
      } catch (error) {
        notifyApiError(error);
      } finally {
        setLoading(false);
      }
    }

    loadAuditTrail();
  }, [expanded]);

  return (
    <TableAccordion title={t('auditTrail.title')} expanded={expanded} setExpanded={setExpanded}>
      <Table
        columns={[
          { name: 'action', title: t('generic.action'), align: 'left' },
          { name: 'actor', title: t('auditTrail.actor'), align: 'center' },
          { name: 'resource', title: t('auditTrail.resource'), align: 'center' },
          { name: 'result', title: t('auditTrail.result'), align: 'center' },
          { name: 'occurredAt', title: t('auditTrail.occurredAt'), align: 'center' },
          { name: 'id' }
        ]}
        rows={auditTrail.map(item => ({
          action: <TextCell>{item.action}</TextCell>,
          actor: <TextCell>{`${item.actorType}:${item.actorId}`}</TextCell>,
          resource: <TextCell>{`${item.resourceType}:${item.resourceId}`}</TextCell>,
          result: <TextCell>{item.result}</TextCell>,
          occurredAt: <TextCell>{formatDateTime(item.occurredAt)}</TextCell>,
          id: item.auditEventId
        }))}
        selectedField="action"
      />
    </TableAccordion>
  );
}

export default ManageAuditTrail;
