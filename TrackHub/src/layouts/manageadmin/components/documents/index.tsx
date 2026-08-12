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
import { getAccountFeatures } from "api/manager/accountFeatures";
import { notifyApiError } from "api/core/errors";
import { searchDocuments, getExpiringDocuments, getDocumentTypes, downloadDocument, configureDocumentType, disableDocumentType } from "api/manager/documents";
import type { DocumentVm, DocumentTypeVm, DocumentTypeDtoInput } from "api/manager/documents";
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from "utils/dateUtils";

// Change event shape emitted by the vendored dialog controls.
type FormChangeHandler = (
  event: { target: { name: string; value: string; type?: string; checked?: boolean } }
) => void;


interface FilterValues { category?: string; status?: string; }

const DOCUMENTS_FEATURE_KEY = "documents";

const scanColor = (s: string): 'success' | 'error' | 'warning' | 'secondary' => (s === 'Clean' ? 'success' : (s === 'Infected' || s === 'Failed') ? 'error' : s === 'Quarantined' ? 'warning' : 'secondary');
const cap = (v: ReactNode): ReactNode => <ArgonTypography variant="caption" color="secondary">{v ?? '-'}</ArgonTypography>;

interface ContextState { library: boolean; expiring: boolean; types: boolean; }
interface ConfirmState { open: boolean; id: string | null; }

function ManageDocuments() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [account, setAccount] = useState<Account | null>(null);
  const [enabled, setEnabled] = useState(false);
  const [ctx, setCtx] = useState<ContextState>({ library: false, expiring: false, types: false });
  const [docs, setDocs] = useState<DocumentVm[]>([]);
  const [expiring, setExpiring] = useState<DocumentVm[]>([]);
  const [types, setTypes] = useState<DocumentTypeVm[]>([]);
  const [typeOpen, setTypeOpen] = useState(false);
  const [confirm, setConfirm] = useState<ConfirmState>({ open: false, id: null });
  const [filters, handleFilterChange] = useForm<FilterValues>({});
  const [typeValues, handleTypeChange, setTypeValues, setTypeErrors, validateType, typeErrors] = useForm<DocumentTypeFormValues>({});
  const bootstrap = useRef(false);

  const ensureAccount = async (): Promise<Account | null> => {
    if (bootstrap.current) return account;
    bootstrap.current = true;
    try {
      const current = await getAccountByUser();
      setAccount(current);
      if (current?.accountId) {
        const features = await getAccountFeatures(current.accountId) || [];
        setEnabled(!!features.find(f => f.featureKey === DOCUMENTS_FEATURE_KEY)?.enabled);
      }
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
      const items = await searchDocuments({ category: filters.category || null, status: filters.status || null }, 0, 100);
      setDocs(items || []);
    } catch (error) {
      notifyApiError(error);
    } finally { setLoading(false); }
  };

  const loadExpiring = async () => {
    setLoading(true);
    try {
      await ensureAccount();
      setExpiring((await getExpiringDocuments(30, 0, 100)) || []);
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

  useEffect(() => { if (ctx.library) loadLibrary(); /* eslint-disable-next-line */ }, [ctx.library]);
  useEffect(() => { if (ctx.expiring) loadExpiring(); /* eslint-disable-next-line */ }, [ctx.expiring]);
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
      <TableAccordion title={t('documentManagement.library')} expanded={ctx.library} setExpanded={(v) => setCtx({ ...ctx, library: v })}>
        <ArgonBox display="flex" gap={2} mb={1} alignItems="flex-end" flexWrap="wrap">
          <CustomTextField margin="none" name="category" id="filterCategory" label={t('documentManagement.category')} type="text" value={filters.category || ''} onChange={handleFilterChange} />
          <CustomTextField margin="none" name="status" id="filterStatus" label={t('documentManagement.status')} type="text" value={filters.status || ''} onChange={handleFilterChange} />
          <ArgonButton color="primary" size="small" onClick={loadLibrary}><Icon>search</Icon></ArgonButton>
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
            owner: cap(`${d.ownerEntityType}:${(d.ownerEntityId || '').substring(0, 8)}`),
            category: cap(d.category),
            classification: cap(t(`documentManagement.values.classification.${(d.classification || '').toLowerCase()}` as 'documentManagement.values.classification.public', { defaultValue: d.classification })),
            status: cap(t(`documentManagement.values.status.${(d.status || '').toLowerCase()}` as 'documentManagement.values.status.active', { defaultValue: d.status })),
            scan: <ArgonBadge badgeContent={t(`documentManagement.values.scan.${(d.scanStatus || '').toLowerCase()}` as 'documentManagement.values.scan.clean', { defaultValue: d.scanStatus })} color={scanColor(d.scanStatus)} size="xs" container />,
            action: d.downloadUrl ? (
              <ArgonButton variant="text" color="dark" onClick={() => handleDownload(d.documentId, d.fileName)}><Icon>download</Icon></ArgonButton>
            ) : null,
            id: d.documentId,
          }))}
          selectedField="fileName"
        />
      </TableAccordion>

      {/* Expiration dashboard */}
      <TableAccordion title={t('documentManagement.expiring')} expanded={ctx.expiring} setExpanded={(v) => setCtx({ ...ctx, expiring: v })}>
        <Table
          columns={[
            { name: 'category', title: t('documentManagement.category'), align: 'left' },
            { name: 'owner', title: t('documentManagement.owner'), align: 'left' },
            { name: 'expires', title: t('documentManagement.expiresAt'), align: 'center' },
            { name: 'status', title: t('documentManagement.status'), align: 'center' },
            { name: 'id' },
          ]}
          rows={expiring.map(d => ({
            category: <ArgonTypography variant="caption" fontWeight="medium">{d.category}</ArgonTypography>,
            owner: cap(`${d.ownerEntityType}:${(d.ownerEntityId || '').substring(0, 8)}`),
            expires: cap(d.expiresAt ? formatDateTime(d.expiresAt) : '-'),
            status: cap(t(`documentManagement.values.status.${(d.status || '').toLowerCase()}` as 'documentManagement.values.status.active', { defaultValue: d.status })),
            id: d.documentId,
          }))}
          selectedField="category"
        />
      </TableAccordion>

      {/* Document-type configuration */}
      <TableAccordion title={t('documentManagement.types')} expanded={ctx.types} showAddIcon={enabled} setOpen={setTypeOpen} handleAddClick={handleAddType} setExpanded={(v) => setCtx({ ...ctx, types: v })}>
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
