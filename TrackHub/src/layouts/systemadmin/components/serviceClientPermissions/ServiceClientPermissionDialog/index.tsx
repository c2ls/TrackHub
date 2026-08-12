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
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import type { ServiceClientPermissionFormValues } from 'layouts/systemadmin/components/serviceClientPermissions';

interface ServiceClientPermissionDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: ServiceClientPermissionFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

function ServiceClientPermissionDialog({ open, setOpen, handleSubmit, values, handleChange, errors }: ServiceClientPermissionDialogProps) {
  const { t } = useTranslation();

  return (
    <FormDialog
      title={t('serviceClientPermissions.formTitle')}
      handleSave={handleSubmit}
      open={open}
      setOpen={setOpen}
      maxWidth="md">
      <form>
        <CustomTextField
          autoFocus
          margin="dense"
          name="clientId"
          id="clientId"
          label={t('serviceClientPermissions.client')}
          type="text"
          fullWidth
          value={values.clientId || ''}
          onChange={handleChange}
          required
          errorMsg={errors.clientId}
        />
        <CustomTextField
          margin="normal"
          name="resource"
          id="resource"
          label={t('serviceClientPermissions.resource')}
          type="text"
          fullWidth
          value={values.resource || ''}
          onChange={handleChange}
          required
          errorMsg={errors.resource}
        />
        <CustomTextField
          margin="normal"
          name="action"
          id="action"
          label={t('serviceClientPermissions.action')}
          type="text"
          fullWidth
          value={values.action || ''}
          onChange={handleChange}
          required
          errorMsg={errors.action}
        />
        <CustomTextField
          margin="normal"
          name="scope"
          id="scope"
          label={t('serviceClientPermissions.scope')}
          type="text"
          fullWidth
          value={values.scope || ''}
          onChange={handleChange}
          required
          errorMsg={errors.scope}
        />
        <CustomTextField
          margin="normal"
          name="audience"
          id="audience"
          label={t('serviceClientPermissions.audience')}
          type="text"
          fullWidth
          value={values.audience || ''}
          onChange={handleChange}
          required
          errorMsg={errors.audience}
        />
        <CustomCheckbox
          name="active"
          id="active"
          value={values.active !== false}
          handleChange={handleChange}
          label={t('serviceClientPermissions.active')} />
        <CustomTextField
          margin="normal"
          name="accountId"
          id="accountId"
          label={t('serviceClientPermissions.account')}
          type="text"
          fullWidth
          value={values.accountId || ''}
          onChange={handleChange}
          errorMsg={errors.accountId}
        />
        <CustomTextField
          margin="normal"
          name="effectiveFrom"
          id="effectiveFrom"
          label={t('serviceClientPermissions.effectiveFrom')}
          type="datetime-local"
          fullWidth
          InputLabelProps={{ shrink: true }}
          value={values.effectiveFrom || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="effectiveTo"
          id="effectiveTo"
          label={t('serviceClientPermissions.effectiveTo')}
          type="datetime-local"
          fullWidth
          InputLabelProps={{ shrink: true }}
          value={values.effectiveTo || ''}
          onChange={handleChange}
        />
      </form>
    </FormDialog>
  );
}

export default ServiceClientPermissionDialog;
