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

import { useRef, useState } from 'react';
import type { ChangeEvent, DragEvent, Dispatch, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import FormDialog from "controls/Dialogs/FormDialog";
import CustomTextField from 'controls/Dialogs/CustomTextField';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import useForm from "controls/Dialogs/useForm";
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import type { DocumentTypeVm } from "api/manager/documents";

/** The payload emitted by {@link DocumentUploadDialog} on save (file + metadata). */
export interface UploadPayload {
  file: File;
  category?: string;
  classification?: string;
  title?: string;
  description?: string;
  expiresAt?: string | null;
  reason?: string;
}

// Internal dialog/form state.
interface UploadFormValues {
  category?: string;
  classification?: string;
  title?: string;
  description?: string;
  expiresAt?: string;
  reason?: string;
}


const CLASSIFICATIONS = ['Public', 'Internal', 'Confidential', 'Legal'];

interface DocumentUploadDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  onUpload: (payload: UploadPayload) => void | Promise<void>;
  categories?: DocumentTypeVm[];
  replaceMode?: boolean;
}

// Upload a new document or a replacement version. Collects the file plus category/
// classification metadata; the drop area accepts drag-and-drop or click-to-select.
function DocumentUploadDialog({ open, setOpen, onUpload, categories = [], replaceMode = false }: DocumentUploadDialogProps) {
  const { t } = useTranslation();
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<UploadFormValues>({ classification: 'Internal' });
  const [file, setFile] = useState<File | null>(null);
  const [dragging, setDragging] = useState(false);
  const inputRef = useRef<HTMLInputElement | null>(null);

  const reset = () => { setValues({ classification: 'Internal' }); setErrors({}); setFile(null); };

  const pickFile = (selected: File | null | undefined) => { if (selected) setFile(selected); };

  const handleDrop = (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    setDragging(false);
    if (event.dataTransfer.files && event.dataTransfer.files.length > 0) {
      pickFile(event.dataTransfer.files[0]);
    }
  };

  const handleSave = async () => {
    const required = replaceMode ? [] : ['category'];
    if (!validate(required) || !file) {
      if (!file) setErrors({ ...errors, file: t('validation.required', { field: 'file' }) });
      return;
    }
    await onUpload({
      file,
      category: values.category,
      classification: values.classification,
      title: values.title,
      description: values.description,
      expiresAt: values.expiresAt ? new Date(values.expiresAt).toISOString() : null,
      reason: values.reason,
    });
    reset();
    setOpen(false);
  };

  const categoryOptions = (categories || []).map(c => ({ value: c.category, label: c.displayName || c.category }));

  return (
    <FormDialog
      title={replaceMode ? t('documentManagement.replace') : t('documentManagement.upload')}
      handleSave={handleSave}
      handleCancel={reset}
      open={open}
      setOpen={setOpen}
      maxWidth="md">
      <form>
        <ArgonBox
          onDragOver={(e) => { e.preventDefault(); setDragging(true); }}
          onDragLeave={() => setDragging(false)}
          onDrop={handleDrop}
          onClick={() => inputRef.current && inputRef.current.click()}
          sx={{
            border: '2px dashed',
            borderColor: dragging ? 'info.main' : 'grey.400',
            borderRadius: 2,
            p: 3,
            my: 1,
            textAlign: 'center',
            cursor: 'pointer',
          }}>
          <input
            ref={inputRef}
            type="file"
            hidden
            onChange={(e: ChangeEvent<HTMLInputElement>) => pickFile(e.target.files && e.target.files[0])}
          />
          <ArgonTypography variant="caption" color={file ? 'success' : 'secondary'} fontWeight="medium">
            {file ? file.name : t('documentManagement.dropHere')}
          </ArgonTypography>
          {errors.file && !file && (
            <ArgonBox mt={1}>
              <ArgonTypography variant="caption" color="error">{errors.file}</ArgonTypography>
            </ArgonBox>
          )}
        </ArgonBox>

        {!replaceMode && (categoryOptions.length > 0 ? (
          <CustomSelect
            name="category"
            id="category"
            label={t('documentManagement.category')}
            list={categoryOptions}
            numericValue={false}
            value={values.category || ''}
            handleChange={handleChange}
            required
          />
        ) : (
          <CustomTextField
            margin="normal"
            name="category"
            id="category"
            label={t('documentManagement.category')}
            type="text"
            fullWidth
            value={values.category || ''}
            onChange={handleChange}
            required
            errorMsg={errors.category}
          />
        ))}

        {!replaceMode && (
          <CustomSelect
            name="classification"
            id="classification"
            label={t('documentManagement.classification')}
            list={CLASSIFICATIONS.map(c => ({ value: c, label: t(`documentManagement.values.classification.${c.toLowerCase()}` as 'documentManagement.values.classification.public', { defaultValue: c }) }))}
            numericValue={false}
            value={values.classification || 'Internal'}
            handleChange={handleChange}
          />
        )}

        {!replaceMode && (
          <CustomTextField
            margin="normal"
            name="title"
            id="title"
            label={t('documentManagement.displayTitle')}
            type="text"
            fullWidth
            value={values.title || ''}
            onChange={handleChange}
          />
        )}

        {!replaceMode && (
          <CustomTextField
            margin="normal"
            name="expiresAt"
            id="expiresAt"
            label={t('documentManagement.expiresAt')}
            type="datetime-local"
            fullWidth
            InputLabelProps={{ shrink: true }}
            value={values.expiresAt || ''}
            onChange={handleChange}
          />
        )}

        {replaceMode && (
          <CustomTextField
            margin="normal"
            name="reason"
            id="reason"
            label={t('documentManagement.reason')}
            type="text"
            fullWidth
            value={values.reason || ''}
            onChange={handleChange}
          />
        )}
      </form>
    </FormDialog>
  );
}

export default DocumentUploadDialog;
