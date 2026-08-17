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

import type { Dispatch, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import FormDialog from 'controls/Dialogs/FormDialog';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import type { SelectListItem } from 'controls/Dialogs/CustomSelect';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import ArgonTypography from 'components/ArgonTypography';
import type { FormChangeHandler, FormErrors } from 'controls/Dialogs/useForm';
import type { DocumentVm } from 'api/manager/documents';
import {
  QUALIFICATION_TYPES,
  QUALIFICATION_STATUSES,
  qualificationTypeLabel,
  qualificationStatusLabel,
} from 'layouts/manageadmin/components/drivers/qualificationConstants';

/**
 * Dialog/form state for a driver qualification. All fields are optional here;
 * the screen's validate() gate enforces the required `qualificationType`/`status`
 * before save, and driver/account ids come from the screen, not the form.
 */
export interface QualificationFormValues {
  driverQualificationId?: string;
  qualificationType?: string;
  category?: string | null;
  number?: string | null;
  issuedAt?: string | null;
  expiresAt?: string | null;
  issuingAuthority?: string | null;
  status?: string;
  documentId?: string | null;
  notes?: string | null;
}

interface QualificationDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: QualificationFormValues;
  handleChange: FormChangeHandler;
  /** Per-field validation messages from the screen's `validate()` gate. */
  errors: FormErrors;
  /** The driver's spec-04 documents (owner entity `Driver`), the link candidates. */
  documents: DocumentVm[];
}

/** Human label for a document option: its title, else the stored file name. */
function documentLabel(document: DocumentVm): string {
  const name = document.title || document.fileName;
  return document.category ? `${name} (${document.category})` : name;
}

/** Sentinel option value meaning "no linked document" (see the picker comment). */
const NO_DOCUMENT = '__none__';

function QualificationDialog({
  open,
  setOpen,
  handleSubmit,
  values,
  handleChange,
  errors,
  documents,
}: QualificationDialogProps) {
  const { t } = useTranslation();

  const options: SelectListItem[] = documents.map((document) => ({
    value: document.documentId,
    label: documentLabel(document),
  }));
  // A link to a document the picker cannot see any more (voided, removed, or
  // owned elsewhere) stays visible and truthful instead of silently blanking.
  const documentOptions: SelectListItem[] =
    values.documentId && !documents.some((document) => document.documentId === values.documentId)
      ? [
          { value: values.documentId, label: t('workforce.qualifications.documentUnavailable') },
          ...options,
          { value: NO_DOCUMENT, label: t('workforce.qualifications.documentNone') },
        ]
      : [...options, { value: NO_DOCUMENT, label: t('workforce.qualifications.documentNone') }];

  return (
    <FormDialog
      title={
        values.driverQualificationId
          ? t('workforce.qualifications.update')
          : t('workforce.qualifications.create')
      }
      handleSave={handleSubmit}
      open={open}
      setOpen={setOpen}
      maxWidth="md"
    >
      <form>
        <CustomSelect
          list={QUALIFICATION_TYPES.map((type) => ({
            value: type,
            label: qualificationTypeLabel(t, type),
          }))}
          name="qualificationType"
          id="qualificationType"
          label={t('workforce.qualifications.type')}
          value={values.qualificationType || ''}
          handleChange={handleChange}
          numericValue={false}
          required
          errorMsg={errors.qualificationType}
          placeholder={t('generic.selectItem')}
        />
        <CustomTextField
          margin="normal"
          name="category"
          id="qualificationCategory"
          label={t('workforce.qualifications.category')}
          type="text"
          fullWidth
          value={values.category || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="number"
          id="qualificationNumber"
          label={t('workforce.qualifications.number')}
          type="text"
          fullWidth
          value={values.number || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="issuedAt"
          id="qualificationIssuedAt"
          label={t('workforce.qualifications.issuedAt')}
          type="date"
          fullWidth
          value={values.issuedAt ? values.issuedAt.substring(0, 10) : ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="expiresAt"
          id="qualificationExpiresAt"
          label={t('workforce.qualifications.expiresAt')}
          type="date"
          fullWidth
          value={values.expiresAt ? values.expiresAt.substring(0, 10) : ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="issuingAuthority"
          id="qualificationIssuingAuthority"
          label={t('workforce.qualifications.issuingAuthority')}
          type="text"
          fullWidth
          value={values.issuingAuthority || ''}
          onChange={handleChange}
        />
        <CustomSelect
          list={QUALIFICATION_STATUSES.map((status) => ({
            value: status,
            label: qualificationStatusLabel(t, status),
          }))}
          name="status"
          id="qualificationStatus"
          label={t('workforce.qualifications.status')}
          value={values.status || ''}
          handleChange={handleChange}
          numericValue={false}
          required
          errorMsg={errors.status}
          placeholder={t('generic.selectItem')}
        />
        {/*
          Spec 09 §8/§11: the evidence link is a spec-04 Document owned by the
          `Driver`, picked from that driver's documents — never a hand-typed GUID.
          The `NO_DOCUMENT` sentinel exists only to give the list a "clear the
          link" entry (CustomSelect reserves the empty value for its disabled
          placeholder); it is normalized away before it reaches form state.
        */}
        <CustomSelect
          list={documentOptions}
          name="documentId"
          id="qualificationDocumentId"
          label={t('workforce.qualifications.documentId')}
          value={values.documentId || ''}
          handleChange={(event) =>
            handleChange({
              target: {
                name: 'documentId',
                value: event.target.value === NO_DOCUMENT ? '' : String(event.target.value ?? ''),
              },
            })
          }
          numericValue={false}
          placeholder={t('workforce.qualifications.documentNone')}
        />
        <ArgonTypography variant="caption" color="secondary">
          {t('workforce.qualifications.documentIdHelp')}
        </ArgonTypography>
        <CustomTextField
          margin="normal"
          name="notes"
          id="qualificationNotes"
          label={t('workforce.qualifications.notes')}
          type="text"
          fullWidth
          multiline
          minRows={2}
          value={values.notes || ''}
          onChange={handleChange}
        />
      </form>
    </FormDialog>
  );
}

export default QualificationDialog;
