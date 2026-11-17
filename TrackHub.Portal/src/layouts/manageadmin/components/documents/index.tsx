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
import { scanBadgeColor } from 'utils/documentScan';
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import Icon from '@mui/material/Icon';
import Table from "controls/Tables/Table";
import ServerPagination from "controls/Tables/ServerPagination";
import { useClampPage, useServerList } from "controls/Tables/useServerList";
import TableAccordion from "controls/Accordions/TableAccordion";
import ArgonBox from "components/ArgonBox";
import ArgonButton from "components/ArgonButton";
import ArgonBadge from "components/ArgonBadge";
import ArgonTypography from "components/ArgonTypography";
import CustomTextField from 'controls/Dialogs/CustomTextField';
import ConfirmDialog from "controls/Dialogs/ConfirmDialog";
import useForm from "controls/Dialogs/useForm";
import DocumentTypeDialog from "layouts/manageadmin/components/documents/DocumentTypeDialog";
import type { DocumentTypeFormValues } from "layouts/manageadmin/components/documents/DocumentTypeDialog";
import { getAccountByUser } from "api/manager/accounts";
import type { Account } from "api/manager/accounts";
import { useFeatures } from "context/features";
import { notifyApiError } from "api/core/errors";
import { searchDocuments, getExpiringDocuments, getDocumentTypes, downloadDocument, configureDocumentType, disableDocumentType } from "api/manager/documents";
import type { DocumentVm, DocumentTypeVm, DocumentTypeDtoInput } from "api/manager/documents";
import { useDocumentTypes } from "queries/documents";
import { useEntityNames } from "queries/entityNames";
import { entityLabel } from "utils/entityLabel";
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from "utils/dateUtils";

const PAGE_SIZE = 25;

// Change event shape emitted by the vendored dialog controls.

interface FilterValues { category?: string; status?: string; }

const DOCUMENTS_FEATURE_KEY = "documents";

const cap = (v: ReactNode): ReactNode => <ArgonTypography variant="caption" color="secondary">{v ?? '-'}</ArgonTypography>;

interface ContextState { library: boolean; expiring: boolean; types: boolean; }
interface ConfirmState { open: boolean; id: string | null; }

function ManageDocuments() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const { isFeatureEnabled } = useFeatures();
  const enabled = isFeatureEnabled(DOCUMENTS_FEATURE_KEY);
  const [account, setAccount] = useState<Account | null>(null);
  const [ctx, setCtx] = useState<ContextState>({ library: false, expiring: false, types: false });
  const [docs, setDocs] = useState<DocumentVm[]>([]);
  const [expiring, setExpiring] = useState<DocumentVm[]>([]);
  const [types, setTypes] = useState<DocumentTypeVm[]>([]);
  const [typeOpen, setTypeOpen] = useState(false);
  const [confirm, setConfirm] = useState<ConfirmState>({ open: false, id: null });
  const [filters, handleFilterChange] = useForm<FilterValues>({});
  const [typeValues, handleTypeChange, setTypeValues, setTypeErrors, validateType, typeErrors] = useForm<DocumentTypeFormValues>({});
  const bootstrap = useRef(false);
  const [docsTotal, setDocsTotal] = useState(0);
  const [expiringTotal, setExpiringTotal] = useState(0);
  const library = useServerList(PAGE_SIZE);
  const expiringList = useServerList(PAGE_SIZE);
  useClampPage(library.page, PAGE_SIZE, docsTotal, library.setPage);
  useClampPage(expiringList.page, PAGE_SIZE, expiringTotal, expiringList.setPage);
  const { data: typeCatalog = [] } = useDocumentTypes(account?.accountId, { includeDisabled: true });
  const ownerNames = useEntityNames([...docs, ...expiring].map(d => ({ type: d.ownerEntityType, id: d.ownerEntityId })));
  const ownerLabel = (d: DocumentVm) => entityLabel(t, ownerNames, d.ownerEntityType, d.ownerEntityId);
  const categoryName = (category: string) => typeCatalog.find(type => type.category === category)?.displayName || category;

  const ensureAccount = async (): Promise<Account | null> => {
    if (bootstrap.current) return account;
    bootstrap.current = true;
    try {
      const current = await getAccountByUser();
      setAccount(current);
      return current;
    } catch (error) {
      notifyApiError(error);
      return null;
    }
  };

  const loadLibrary = async () => {
    setLoading(true);
    try {
      const current = await ensureAccount();
      if (!current?.accountId) return;
      const result = await searchDocuments({ category: filters.category || null, status: filters.status || null }, library.params.skip, library.params.take);
      setDocs(result.items);
      setDocsTotal(result.totalCount);
    } catch (error) {
      notifyApiError(error);
    } finally { setLoading(false); }
  };

  const loadExpiring = async () => {
    setLoading(true);
    try {
      await ensureAccount();
      const result = await getExpiringDocuments(30, expiringList.params.skip, expiringList.params.take);
      setExpiring(result.items);
      setExpiringTotal(result.totalCount);
    } catch (error) {
      notifyApiError(error);
    } finally { setLoading(false); }
  };

  const loadTypes = async () => {
    setLoading(true);
    try {
      const current = await ensureAccount();
      if (!current?.accountId) return;
      setTypes((await getDocumentTypes(current.accountId, true)) || []);
    } catch (error) {
      notifyApiError(error);
    } finally { setLoading(false); }
  };

  const handleDownload = async (documentId: string, fileName: string) => {
    try {
      await downloadDocument(documentId, fileName);
    } catch (error) {
      notifyApiError(error);
    }
  };

  useEffect(() => { if (ctx.library) loadLibrary(); /* eslint-disable-next-line */ }, [ctx.library, library.params]);
  useEffect(() => { if (ctx.expiring) loadExpiring(); /* eslint-disable-next-line */ }, [ctx.expiring, expiringList.params]);
  useEffect(() => { if (ctx.types) loadTypes(); /* eslint-disable-next-line */ }, [ctx.types]);

  const handleAddType = () => { setTypeValues({}); setTypeErrors({}); };

  const submitType = async () => {
    if (!validateType(['category']) || !account?.accountId) return;
    setLoading(true);
    try {
      // validateType(['category']) gates the required field; assert the mutation input at the boundary.
      await configureDocumentType({
        accountId: account.accountId,
        category: typeValues.category,
        displayName: typeValues.displayName,
        required: !!typeValues.required,
        expiring: !!typeValues.expiring,
        defaultValidityDays: typeValues.defaultValidityDays ? Number(typeValues.defaultValidityDays) : null,
      } as DocumentTypeDtoInput);
      setTypeOpen(false);
      await loadTypes();
    } catch (error) {
      notifyApiError(error);
    } finally { setLoading(false); }
  };

  const doDisableType = async () => {
    const id = confirm.id;
    setConfirm({ open: false, id: null });
    if (!id) return;
    setLoading(true);
    try { await disableDocumentType(id); await loadTypes(); }
    catch (error) { notifyApiError(error); }
    finally { setLoading(false); }
  };

  return (
    <>
      {/* Document library / global search */}
      <TableAccordion sectionKey="documents-library" title={t('documentManagement.library')} expanded={ctx.library} setExpanded={(v) => setCtx({ ...ctx, library: v })}>
        <ArgonBox display="flex" gap={2} mb={1} alignItems="flex-end" flexWrap="wrap">
          <CustomTextField margin="none" name="category" id="filterCategory" label={t('documentManagement.category')} type="text" value={filters.category || ''} onChange={handleFilterChange} />
          <CustomTextField margin="none" name="status" id="filterStatus" label={t('documentManagement.status')} type="text" value={filters.status || ''} onChange={handleFilterChange} />
          <ArgonButton color="primary" size="small" onClick={() => (library.page === 0 ? loadLibrary() : library.setPage(0))} aria-label={t('filters.search')}><Icon>search</Icon></ArgonButton>
        </ArgonBox>
        <Table
          columns={[
            { name: 'fileName', title: t('documentManagement.fileName'), align: 'left' },
            { name: 'owner', title: t('documentManagement.owner'), align: 'left' },
            { name: 'category', title: t('documentManagement.category'), align: 'center' },
            { name: 'classification', title: t('documentManagement.classification'), align: 'center' },
            { name: 'status', title: t('documentManagement.status'), align: 'center' },
            { name: 'scan', title: t('documentManagement.scanStatus'), align: 'center' },
            { name: 'action', title: t('generic.action'), align: 'center' },
            { name: 'id' },
          ]}
          rows={docs.map(d => ({
            fileName: <ArgonTypography variant="caption" fontWeight="medium">{d.title || d.fileName}</ArgonTypography>,
            owner: cap(ownerLabel(d)),
            category: cap(categoryName(d.category)),
            classification: cap(t(`documentManagement.values.classification.${(d.classification || '').toLowerCase()}` as 'documentManagement.values.classification.public', { defaultValue: d.classification })),
            status: cap(t(`documentManagement.values.status.${(d.status || '').toLowerCase()}` as 'documentManagement.values.status.active', { defaultValue: d.status })),
            scan: <ArgonBadge badgeContent={t(`documentManagement.values.scan.${(d.scanStatus || '').toLowerCase()}` as 'documentManagement.values.scan.clean', { defaultValue: d.scanStatus })} color={scanBadgeColor(d.scanStatus)} size="xs" container />,
            action: d.downloadUrl ? (
              <ArgonButton variant="text" color="dark" onClick={() => handleDownload(d.documentId, d.fileName)}><Icon>download</Icon></ArgonButton>
            ) : null,
            id: d.documentId,
          }))}
          selectedField="fileName"
          serverPaged
        />
        <ServerPagination page={library.page} pageSize={PAGE_SIZE} totalCount={docsTotal} pageLength={docs.length} onPageChange={library.setPage} />
      </TableAccordion>

      {/* Expiration dashboard */}
      <TableAccordion sectionKey="documents-expiring" title={t('documentManagement.expiring')} expanded={ctx.expiring} setExpanded={(v) => setCtx({ ...ctx, expiring: v })}>
        <Table
          columns={[
            { name: 'category', title: t('documentManagement.category'), align: 'left' },
            { name: 'owner', title: t('documentManagement.owner'), align: 'left' },
            { name: 'expires', title: t('documentManagement.expiresAt'), align: 'center' },
            { name: 'status', title: t('documentManagement.status'), align: 'center' },
            { name: 'id' },
          ]}
          rows={expiring.map(d => ({
            category: <ArgonTypography variant="caption" fontWeight="medium">{categoryName(d.category)}</ArgonTypography>,
            owner: cap(ownerLabel(d)),
            expires: cap(d.expiresAt ? formatDateTime(d.expiresAt) : '-'),
            status: cap(t(`documentManagement.values.status.${(d.status || '').toLowerCase()}` as 'documentManagement.values.status.active', { defaultValue: d.status })),
            id: d.documentId,
          }))}
          selectedField="category"
          serverPaged
        />
        <ServerPagination page={expiringList.page} pageSize={PAGE_SIZE} totalCount={expiringTotal} pageLength={expiring.length} onPageChange={expiringList.setPage} />
      </TableAccordion>

      {/* Document-type configuration */}
      <TableAccordion sectionKey="document-types" title={t('documentManagement.types')} expanded={ctx.types} showAddIcon={enabled} setOpen={setTypeOpen} handleAddClick={handleAddType} setExpanded={(v) => setCtx({ ...ctx, types: v })}>
        <Table
          columns={[
            { name: 'category', title: t('documentManagement.category'), align: 'left' },
            { name: 'required', title: t('documentManagement.required'), align: 'center' },
            { name: 'expiring', title: t('documentManagement.expiringFlag'), align: 'center' },
            { name: 'validity', title: t('documentManagement.validityDays'), align: 'center' },
            { name: 'status', title: t('documentManagement.status'), align: 'center' },
            { name: 'action', title: t('generic.action'), align: 'center' },
            { name: 'id' },
          ]}
          rows={types.map(ty => ({
            category: <ArgonTypography variant="caption" fontWeight="medium">{ty.displayName || ty.category}</ArgonTypography>,
            required: cap(ty.required ? '✓' : '-'),
            expiring: cap(ty.expiring ? '✓' : '-'),
            validity: cap(ty.defaultValidityDays || '-'),
            status: cap(ty.enabled ? t('generic.active') : t('generic.inactive', { defaultValue: 'Disabled' })),
            action: ty.enabled ? (
              <ArgonButton variant="text" color="error" onClick={() => setConfirm({ open: true, id: ty.documentTypeId })}><Icon>block</Icon></ArgonButton>
            ) : null,
            id: ty.documentTypeId,
          }))}
          selectedField="category"
        />
      </TableAccordion>

      <DocumentTypeDialog open={typeOpen} setOpen={setTypeOpen} handleSubmit={submitType} values={typeValues} handleChange={handleTypeChange} errors={typeErrors} />
      <ConfirmDialog
        open={confirm.open}
        setOpen={(v) => setConfirm(prev => ({ ...prev, open: typeof v === 'function' ? v(prev.open) : v }))}
        title={t('documentManagement.types')}
        message={t('generic.confirmDelete')}
        onConfirm={doDisableType}
      />
    </>
  );
}

export default ManageDocuments;
