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
import { useFeatures } from "context/features";
import { useEntityNames } from "queries/entityNames";
import { entityLabel } from "utils/entityLabel";
import { notifyApiError } from "api/core/errors";
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from "utils/dateUtils";

// Change event shape emitted by the vendored dialog controls.

const PUBLIC_LINKS_FEATURE_KEY = "public-links";

function TextCell({ children }: { children?: ReactNode }) {
  return (
    <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
      {children || '-'}
    </ArgonTypography>
  );
}

const PAGE_SIZE = 25;

function ManagePublicLinks() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [account, setAccount] = useState<Account | null>(null);
  const [links, setLinks] = useState<PublicLinkGrant[]>([]);
  const [open, setOpen] = useState(false);
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<PublicLinkFormValues>({});
  const [mintedToken, setMintedToken] = useState<string | null>(null);
  const { isFeatureEnabled } = useFeatures();
  const createEnabled = isFeatureEnabled(PUBLIC_LINKS_FEATURE_KEY);
  const [totalCount, setTotalCount] = useState(0);
  const [pageLength, setPageLength] = useState(0);
  const { page, setPage, params } = useServerList(PAGE_SIZE);
  useClampPage(page, PAGE_SIZE, totalCount, setPage);
  const resourceNames = useEntityNames(links.map(link => ({ type: link.resourceType, id: link.resourceId })));

  const loadLinks = async () => {
    setLoading(true);
    try {
      const currentAccount = await getAccountByUser();
      if (!currentAccount?.accountId) return;
      setAccount(currentAccount);
      const items = await getPublicLinkGrantsByAccount(currentAccount.accountId, params.skip, params.take);
      setLinks(items.items);
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
      loadLinks();
    }
  }, [expanded, params]);

  const handleAddClick = () => {
    setValues({});
    setErrors({});
    setMintedToken(null);
  };

  const handleSubmit = async () => {
    if (!validate(['resourceType', 'resourceId', 'scopes', 'expiresAt']) || !account?.accountId) return;
    setLoading(true);
    try {
      // validate() gates the required fields, so assert the mutation input at the boundary.
      const grant = {
        accountId: account.accountId,
        resourceType: values.resourceType,
        resourceId: values.resourceId,
        scopes: values.scopes,
        purpose: values.purpose || '',
        expiresAt: values.expiresAt ? new Date(values.expiresAt).toISOString() : null,
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
      await revokePublicLinkGrant(link.publicLinkGrantId);
      await loadLinks();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <TableAccordion sectionKey="public-links"
        title={t('publicLinks.title')}
        showAddIcon={createEnabled}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <Table
          columns={[
            { name: 'resource', title: t('publicLinks.resource'), align: 'left' },
            { name: 'scopes', title: t('publicLinks.scopesHeader'), align: 'center' },
            { name: 'expires', title: t('publicLinks.expiresAt'), align: 'center' },
            { name: 'accessCount', title: t('publicLinks.accessCount'), align: 'center' },
            { name: 'status', title: t('publicLinks.status'), align: 'center' },
            { name: 'action', title: t('generic.action'), align: 'center' },
            { name: 'id' }
          ]}
          rows={links.map(link => ({
            resource: <TextCell>{entityLabel(t, resourceNames, link.resourceType, link.resourceId)}</TextCell>,
            scopes: <TextCell>{link.scopes}</TextCell>,
            expires: <TextCell>{formatDateTime(link.expiresAt)}</TextCell>,
            accessCount: <TextCell>{link.accessCount}</TextCell>,
            status: <TextCell>{link.revokedAt ? t('publicLinks.revoked') : t('generic.active')}</TextCell>,
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
          serverPaged
        />
        <ServerPagination page={page} pageSize={PAGE_SIZE} totalCount={totalCount} pageLength={pageLength} onPageChange={setPage} />
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
