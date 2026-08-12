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
import SupportGrantDialog from "layouts/systemadmin/components/accountSupportGrants/SupportGrantDialog";
import { getCurrentPrincipal } from "api/manager/principals";
import type { CurrentPrincipal } from "api/manager/principals";
import {
  getAccountSupportGrants,
  createAccountSupportGrant,
  approveAccountSupportGrant,
  revokeAccountSupportGrant,
} from "api/manager/supportGrants";
import type { AccountSupportGrant, AccountSupportGrantDtoInput } from "api/manager/supportGrants";
import { notifyApiError } from "api/core/errors";
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from "utils/dateUtils";

/** Support-grant request form state (loose until the validate() gate). */
export interface SupportGrantFormValues {
  accountId?: string;
  supportUserId?: string;
  reason?: string;
  ticketReference?: string;
  accessLevel?: string;
  startsAt?: string;
  endsAt?: string;
}

function TextCell({ children }: { children?: ReactNode }) {
  return (
    <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
      {children || '-'}
    </ArgonTypography>
  );
}

function ManageAccountSupportGrants() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [grants, setGrants] = useState<AccountSupportGrant[]>([]);
  const [principal, setPrincipal] = useState<CurrentPrincipal | null>(null);
  const [open, setOpen] = useState(false);
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<SupportGrantFormValues>({ accessLevel: 'read' });
  const loaded = useRef(false);
  const principalId = principal?.userId || principal?.driverId || principal?.clientId || principal?.subjectId || '';

  const loadGrants = async () => {
    setLoading(true);
    try {
      const [items, current] = await Promise.all([
        getAccountSupportGrants(null),
        getCurrentPrincipal()
      ]);
      setGrants(items || []);
      setPrincipal(current);
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (expanded && !loaded.current) {
      loaded.current = true;
      loadGrants();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [expanded]);

  const handleAddClick = () => {
    setValues({ accessLevel: 'read', supportUserId: principalId });
    setErrors({});
  };

  const handleSubmit = async () => {
    if (!validate(['accountId', 'supportUserId', 'reason', 'ticketReference', 'startsAt', 'endsAt'])) return;
    setLoading(true);
    try {
      // The validate() gate above guarantees the required fields are present.
      await createAccountSupportGrant({
        accountId: values.accountId,
        supportUserId: values.supportUserId,
        reason: values.reason,
        ticketReference: values.ticketReference,
        accessLevel: values.accessLevel || 'read',
        startsAt: values.startsAt ? new Date(values.startsAt).toISOString() : null,
        endsAt: values.endsAt ? new Date(values.endsAt).toISOString() : null
      } as AccountSupportGrantDtoInput);
      setOpen(false);
      await loadGrants();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async (grant: AccountSupportGrant) => {
    if (!grant?.accountSupportGrantId || !principalId) return;
    setLoading(true);
    try {
      await approveAccountSupportGrant(grant.accountSupportGrantId, principalId);
      await loadGrants();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const handleRevoke = async (grant: AccountSupportGrant) => {
    if (!grant?.accountSupportGrantId || !principalId) return;
    setLoading(true);
    try {
      await revokeAccountSupportGrant(grant.accountSupportGrantId, principalId);
      await loadGrants();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <TableAccordion
        title={t('supportGrants.title')}
        showAddIcon={true}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <Table
          columns={[
            { name: 'account', title: t('account.title'), align: 'left' },
            { name: 'supportUser', title: t('supportGrants.supportUserId'), align: 'center' },
            { name: 'reason', title: t('supportGrants.reason'), align: 'left' },
            { name: 'ticket', title: t('supportGrants.ticketReference'), align: 'center' },
            { name: 'accessLevel', title: t('supportGrants.accessLevel'), align: 'center' },
            { name: 'window', title: t('supportGrants.startsAt'), align: 'center' },
            { name: 'status', title: t('supportGrants.status'), align: 'center' },
            { name: 'action', title: t('generic.action'), align: 'center' },
            { name: 'id' }
          ]}
          rows={grants.map(grant => {
            const status = grant.revokedAt
              ? t('supportGrants.revokedAt')
              : (grant.approvedAt ? t('supportGrants.approvedAt') : t('supportGrants.request'));
            return {
              account: <TextCell>{grant.accountId}</TextCell>,
              supportUser: <TextCell>{grant.supportUserId}</TextCell>,
              reason: <TextCell>{grant.reason}</TextCell>,
              ticket: <TextCell>{grant.ticketReference}</TextCell>,
              accessLevel: <TextCell>{grant.accessLevel}</TextCell>,
              window: <TextCell>{`${formatDateTime(grant.startsAt)} -> ${formatDateTime(grant.endsAt)}`}</TextCell>,
              status: <TextCell>{status}</TextCell>,
              action: (
                <>
                  {!grant.approvedAt && !grant.revokedAt && (
                    <ArgonButton variant="text" color="success" onClick={() => handleApprove(grant)}>
                      <Icon>verified</Icon>&nbsp;{t('supportGrants.approve')}
                    </ArgonButton>
                  )}
                  {!grant.revokedAt && (
                    <ArgonButton variant="text" color="error" onClick={() => handleRevoke(grant)}>
                      <Icon>block</Icon>&nbsp;{t('supportGrants.revoke')}
                    </ArgonButton>
                  )}
                </>
              ),
              id: grant.accountSupportGrantId
            };
          })}
          selectedField="reason"
        />
      </TableAccordion>
      <SupportGrantDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        errors={errors}
      />
    </>
  );
}

export default ManageAccountSupportGrants;
