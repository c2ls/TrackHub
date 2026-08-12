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
import PublicLinkDialog from "layouts/manageadmin/components/publicLinks/PublicLinkDialog";
import type { PublicLinkFormValues } from "layouts/manageadmin/components/publicLinks/PublicLinkDialog";
import { getAccountByUser } from "api/manager/accounts";
import type { Account } from "api/manager/accounts";
import {
  getPublicLinkGrantsByAccount,
  createPublicLinkGrant,
  revokePublicLinkGrant,
} from "api/manager/publicLinks";
import type { PublicLinkGrant, PublicLinkGrantDtoInput } from "api/manager/publicLinks";
import { getAccountFeatures } from "api/manager/accountFeatures";
import { getCurrentPrincipal } from "api/manager/principals";
import { notifyApiError } from "api/core/errors";
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from "utils/dateUtils";

// Change event shape emitted by the vendored dialog controls.
type FormChangeHandler = (
  event: { target: { name: string; value: string; type?: string; checked?: boolean } }
) => void;


const PUBLIC_LINKS_FEATURE_KEY = "public-links";

function TextCell({ children }: { children?: ReactNode }) {
  return (
    <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
      {children || '-'}
    </ArgonTypography>
  );
}

function ManagePublicLinks() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [account, setAccount] = useState<Account | null>(null);
  const [links, setLinks] = useState<PublicLinkGrant[]>([]);
  const [open, setOpen] = useState(false);
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<PublicLinkFormValues>({});
  const [mintedToken, setMintedToken] = useState<string | null>(null);
  const [createEnabled, setCreateEnabled] = useState(false);
  const loaded = useRef(false);
  const [revokedBy, setRevokedBy] = useState('');

  const loadLinks = async () => {
    setLoading(true);
    try {
      const principal = await getCurrentPrincipal();
      setRevokedBy(principal?.userId || principal?.driverId || principal?.clientId || principal?.subjectId || '');
      const currentAccount = await getAccountByUser();
      if (!currentAccount?.accountId) return;
      setAccount(currentAccount);
      // Creation is feature-gated (backend enforces FEATURE_DISABLED); listing/revoking stay available.
      const features = await getAccountFeatures(currentAccount.accountId) || [];
      const feature = features.find(item => item.featureKey === PUBLIC_LINKS_FEATURE_KEY);
      setCreateEnabled(!!feature?.enabled);
      const items = await getPublicLinkGrantsByAccount(currentAccount.accountId);
      setLinks(items || []);
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (expanded && !loaded.current) {
      loaded.current = true;
      loadLinks();
    }
  }, [expanded]);

  const handleAddClick = () => {
    setValues({});
    setErrors({});
    setMintedToken(null);
  };

  const handleSubmit = async () => {
    if (!validate(['resourceType', 'resourceId', 'scopes', 'expiresAt']) || !account?.accountId) return;
    setLoading(true);
    try {
      // createdByPrincipalId is required (String!) by the backend; the old
      // string-built mutation never sent it, so create always failed. Source it
      // from the current principal (same value used for revokedBy). validate()
      // gates the required fields, so assert the mutation input at the boundary.
      const grant = {
        accountId: account.accountId,
        resourceType: values.resourceType,
        resourceId: values.resourceId,
        scopes: values.scopes,
        purpose: values.purpose || '',
        subjectTokenIdHash: null,
        expiresAt: values.expiresAt ? new Date(values.expiresAt).toISOString() : null,
        createdByPrincipalId: revokedBy,
      } as PublicLinkGrantDtoInput;
      const result = await createPublicLinkGrant(grant);
      if (result?.token) {
        setMintedToken(result.token);
      } else {
        setOpen(false);
      }
      await loadLinks();
    } catch (error) {
      // Keep the dialog open on failure so the user can retry.
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const handleRevoke = async (link: PublicLinkGrant) => {
    if (!link?.publicLinkGrantId) return;
    setLoading(true);
    try {
      await revokePublicLinkGrant(link.publicLinkGrantId, revokedBy);
      await loadLinks();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <TableAccordion
        title={t('publicLinks.title')}
        showAddIcon={createEnabled}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <Table
          columns={[
            { name: 'resource', title: t('publicLinks.resource'), align: 'left' },
            { name: 'scopes', title: t('publicLinks.scopes'), align: 'center' },
            { name: 'expires', title: t('publicLinks.expiresAt'), align: 'center' },
            { name: 'accessCount', title: t('publicLinks.accessCount'), align: 'center' },
            { name: 'status', title: t('publicLinks.status'), align: 'center' },
            { name: 'action', title: t('generic.action'), align: 'center' },
            { name: 'id' }
          ]}
          rows={links.map(link => ({
            resource: <TextCell>{`${link.resourceType}:${link.resourceId}`}</TextCell>,
            scopes: <TextCell>{link.scopes}</TextCell>,
            expires: <TextCell>{formatDateTime(link.expiresAt)}</TextCell>,
            accessCount: <TextCell>{link.accessCount}</TextCell>,
            status: <TextCell>{link.revokedAt ? t('publicLinks.revokedAt') : t('generic.active')}</TextCell>,
            action: (
              !link.revokedAt && (
                <ArgonButton variant="text" color="error" onClick={() => handleRevoke(link)}>
                  <Icon>block</Icon>&nbsp;{t('publicLinks.revoke')}
                </ArgonButton>
              )
            ),
            id: link.publicLinkGrantId
          }))}
          selectedField="resource"
        />
      </TableAccordion>
      <PublicLinkDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        errors={errors}
        mintedToken={mintedToken}
      />
    </>
  );
}

export default ManagePublicLinks;
