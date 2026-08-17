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
import { useTranslation } from 'react-i18next';
import Icon from '@mui/material/Icon';
import Table, { type TableColumn } from "controls/Tables/Table";
import ArgonBox from "components/ArgonBox";
import ArgonButton from "components/ArgonButton";
import ArgonBadge from "components/ArgonBadge";
import ArgonTypography from "components/ArgonTypography";
import ConfirmDialog from "controls/Dialogs/ConfirmDialog";
import DocumentUploadDialog from "layouts/manageadmin/components/documents/DocumentUploadDialog";
import type { UploadPayload } from "layouts/manageadmin/components/documents/DocumentUploadDialog";
import ShareDialog from "layouts/manageadmin/components/documents/ShareDialog";
import { getDocumentsForOwner, uploadDocument, uploadDocumentVersion, downloadDocument, voidDocument, deleteDocumentReference } from "api/manager/documents";
import type { DocumentVm, DocumentTypeVm } from "api/manager/documents";
import { notifyApiError } from "api/core/errors";
import { LoadingContext } from 'LoadingContext';
import { formatDateTime } from "utils/dateUtils";

const scanColor = (scanStatus: string): 'success' | 'error' | 'warning' | 'secondary' => {
  switch (scanStatus) {
    case 'Clean': return 'success';
    case 'Infected':
    case 'Failed': return 'error';
    case 'Quarantined': return 'warning';
    default: return 'secondary';
  }
};

interface ConfirmState { open: boolean; kind?: 'void' | 'delete'; }

interface DocumentPanelProps {
  accountId?: string | null;
  ownerEntityType: string;
  ownerEntityId?: string | null;
  canManage?: boolean;
  categories?: DocumentTypeVm[];
  /** Fired after every (re)load so an embedding screen can refresh its own
   *  document-derived state — e.g. the driver qualification document picker. */
  onDocumentsChanged?: (documents: DocumentVm[]) => void;
}

// Reusable, owner-scoped document panel. Lists an owner's documents and provides upload
// (drag-drop), download, new-version, void, remove-reference, and share. Embeddable on any owner detail
// surface; gated by the OWNER module's feature (pass canManage), not the `documents` feature.
function DocumentPanel({ accountId = null, ownerEntityType, ownerEntityId = null, canManage = true, categories = [], onDocumentsChanged }: DocumentPanelProps) {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [docs, setDocs] = useState<DocumentVm[]>([]);
  const [uploadOpen, setUploadOpen] = useState(false);
  const [replaceOpen, setReplaceOpen] = useState(false);
  const [shareOpen, setShareOpen] = useState(false);
  const [confirm, setConfirm] = useState<ConfirmState>({ open: false });
  const [active, setActive] = useState<DocumentVm | null>(null);
  const changedRef = useRef(onDocumentsChanged);
  changedRef.current = onDocumentsChanged;

  const load = async () => {
    if (!accountId || !ownerEntityId) { setDocs([]); return; }
    setLoading(true);
    try {
      const items = await getDocumentsForOwner(accountId, ownerEntityType, ownerEntityId);
      setDocs(items || []);
      changedRef.current?.(items || []);
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  // Reload whenever the owner changes: the panel is embeddable on surfaces that
  // switch owner in place (e.g. the driver picker on the qualifications screen),
  // where a one-shot load would keep showing the previous owner's documents.
  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [accountId, ownerEntityType, ownerEntityId]);

  const handleUpload = async ({ file, category, classification, title, description, expiresAt }: UploadPayload) => {
    setLoading(true);
    try {
      await uploadDocument(file, { accountId, ownerEntityType, ownerEntityId, category, classification, title, description, expiresAt });
      await load();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const handleReplace = async ({ file, reason }: UploadPayload) => {
    if (!active) return;
    setLoading(true);
    try {
      await uploadDocumentVersion(active.documentId, file, { reason });
      await load();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const handleDownload = async (doc: DocumentVm) => {
    try {
      await downloadDocument(doc.documentId, doc.fileName);
    } catch (error) {
      notifyApiError(error);
    }
  };

  const runConfirmed = async (action: (doc: DocumentVm) => Promise<unknown>) => {
    setConfirm({ open: false });
    if (!active) return;
    setLoading(true);
    try {
      await action(active);
      await load();
    } catch (error) {
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const columns: TableColumn[] = [
    { name: 'fileName', title: t('documentManagement.fileName'), align: 'left' },
    { name: 'category', title: t('documentManagement.category'), align: 'center' },
    { name: 'classification', title: t('documentManagement.classification'), align: 'center' },
    { name: 'status', title: t('documentManagement.status'), align: 'center' },
    { name: 'scan', title: t('documentManagement.scanStatus'), align: 'center' },
    { name: 'expires', title: t('documentManagement.expiresAt'), align: 'center' },
    { name: 'version', title: t('documentManagement.currentVersion'), align: 'center' },
    { name: 'action', title: t('generic.action'), align: 'center' },
    { name: 'id' },
  ];

  const rows = docs.map(doc => ({
    fileName: <ArgonTypography variant="caption" fontWeight="medium">{doc.title || doc.fileName}</ArgonTypography>,
    category: <ArgonTypography variant="caption" color="secondary">{doc.category}</ArgonTypography>,
    classification: <ArgonTypography variant="caption" color="secondary">{t(`documentManagement.values.classification.${(doc.classification || '').toLowerCase()}` as 'documentManagement.values.classification.public', { defaultValue: doc.classification })}</ArgonTypography>,
    status: <ArgonTypography variant="caption" color="secondary">{t(`documentManagement.values.status.${(doc.status || '').toLowerCase()}` as 'documentManagement.values.status.active', { defaultValue: doc.status })}</ArgonTypography>,
    scan: <ArgonBadge badgeContent={t(`documentManagement.values.scan.${(doc.scanStatus || '').toLowerCase()}` as 'documentManagement.values.scan.clean', { defaultValue: doc.scanStatus })} color={scanColor(doc.scanStatus)} size="xs" container />,
    expires: <ArgonTypography variant="caption" color="secondary">{doc.expiresAt ? formatDateTime(doc.expiresAt) : '-'}</ArgonTypography>,
    version: <ArgonTypography variant="caption" color="secondary">{doc.currentVersion}</ArgonTypography>,
    action: (
      <ArgonBox display="flex" justifyContent="center" flexWrap="wrap">
        {doc.downloadUrl && (
          <ArgonButton variant="text" color="dark" onClick={() => handleDownload(doc)}>
            <Icon>download</Icon>
          </ArgonButton>
        )}
        {canManage && (
          <>
            <ArgonButton variant="text" color="info" onClick={() => { setActive(doc); setReplaceOpen(true); }}>
              <Icon>upload_file</Icon>
            </ArgonButton>
            <ArgonButton variant="text" color="secondary" onClick={() => { setActive(doc); setShareOpen(true); }}>
              <Icon>share</Icon>
            </ArgonButton>
            <ArgonButton variant="text" color="warning" onClick={() => { setActive(doc); setConfirm({ open: true, kind: 'void' }); }}>
              <Icon>block</Icon>
            </ArgonButton>
            <ArgonButton variant="text" color="error" onClick={() => { setActive(doc); setConfirm({ open: true, kind: 'delete' }); }}>
              <Icon>delete</Icon>
            </ArgonButton>
          </>
        )}
      </ArgonBox>
    ),
    id: doc.documentId,
  }));

  return (
    <ArgonBox>
      {canManage && (
        <ArgonBox mb={1} display="flex" justifyContent="flex-end">
          <ArgonButton color="primary" size="small" onClick={() => setUploadOpen(true)}>
            <Icon>add</Icon>&nbsp;{t('documentManagement.upload')}
          </ArgonButton>
        </ArgonBox>
      )}
      <Table columns={columns} rows={rows} selectedField="fileName" />

      <DocumentUploadDialog open={uploadOpen} setOpen={setUploadOpen} onUpload={handleUpload} categories={categories} />
      <DocumentUploadDialog open={replaceOpen} setOpen={setReplaceOpen} onUpload={handleReplace} replaceMode />
      <ShareDialog open={shareOpen} setOpen={setShareOpen} accountId={accountId} document={active} />
      <ConfirmDialog
        open={confirm.open}
        setOpen={(v) => setConfirm(prev => ({ ...prev, open: typeof v === 'function' ? v(prev.open) : v }))}
        title={confirm.kind === 'delete' ? t('documentManagement.delete') : t('documentManagement.void')}
        message={confirm.kind === 'delete' ? t('generic.confirmDelete') : t('documentManagement.void')}
        onConfirm={() => runConfirmed(confirm.kind === 'delete'
          ? (d) => deleteDocumentReference(d.documentId)
          : (d) => voidDocument(d.documentId, 'Voided from panel'))}
      />
    </ArgonBox>
  );
}

export default DocumentPanel;
