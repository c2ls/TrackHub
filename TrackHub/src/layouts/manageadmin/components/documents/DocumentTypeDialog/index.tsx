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
import FormDialog from "controls/Dialogs/FormDialog";
import CustomTextField from 'controls/Dialogs/CustomTextField';
import CustomCheckbox from 'controls/Dialogs/CustomCheckbox';
import ArgonBox from "components/ArgonBox";
import type { FormChangeHandler } from 'controls/Dialogs/useForm';

/** Dialog/form state for configuring an account document type / category. */
export interface DocumentTypeFormValues {
  category?: string;
  displayName?: string;
  required?: boolean;
  expiring?: boolean;
  defaultValidityDays?: string | number;
}

interface DocumentTypeDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: DocumentTypeFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

// Configure an account document type / Category with required + expiring flags.
function DocumentTypeDialog({ open, setOpen, handleSubmit, values, handleChange, errors }: DocumentTypeDialogProps) {
  const { t } = useTranslation();

  return (
    <FormDialog title={t('documentManagement.types')} handleSave={handleSubmit} open={open} setOpen={setOpen} maxWidth="sm">
      <form>
        <CustomTextField
          autoFocus margin="dense" name="category" id="category" label={t('documentManagement.category')} type="text" fullWidth
          value={values.category || ''} onChange={handleChange} required errorMsg={errors.category}
        />
        <CustomTextField
          margin="normal" name="displayName" id="displayName" label={t('documentManagement.displayTitle')} type="text" fullWidth
          value={values.displayName || ''} onChange={handleChange}
        />
        <CustomTextField
          margin="normal" name="defaultValidityDays" id="defaultValidityDays" label={t('documentManagement.validityDays')} type="number" fullWidth
          value={values.defaultValidityDays || ''} onChange={handleChange}
        />
        <ArgonBox mt={1} display="flex" flexDirection="column">
          <CustomCheckbox name="required" id="required" value={!!values.required} label={t('documentManagement.required')} handleChange={handleChange} />
          <CustomCheckbox name="expiring" id="expiring" value={!!values.expiring} label={t('documentManagement.expiringFlag')} handleChange={handleChange} />
        </ArgonBox>
      </form>
    </FormDialog>
  );
}

export default DocumentTypeDialog;
