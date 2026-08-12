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
import CustomCheckbox from 'controls/Dialogs/CustomCheckbox';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';

/**
 * Dialog/form state for a driver. Merges an API {@link Driver} (when editing)
 * with the fresh-add shape (`{ accountId, active }`); all fields are optional
 * and the `name` requirement (plus a resolved accountId) is enforced by the
 * screen's validate() gate before save.
 */
export interface DriverFormValues {
  driverId?: string;
  accountId?: string;
  name?: string;
  phone?: string | null;
  documentType?: string | null;
  documentNumber?: string | null;
  employeeCode?: string | null;
  licenseNumber?: string | null;
  licenseExpiresAt?: string | null;
  defaultTransporterId?: string | null;
  active?: boolean;
}

interface DriverDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: DriverFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

function DriverDialog({ open, setOpen, handleSubmit, values, handleChange, errors }: DriverDialogProps) {
  const { t } = useTranslation();

  return (
    <FormDialog
      title={values.driverId ? t('driver.update') : t('driver.create')}
      handleSave={handleSubmit}
      open={open}
      setOpen={setOpen}
      maxWidth="md">
      <form>
        <CustomTextField
          autoFocus
          margin="dense"
          name="name"
          id="name"
          label={t('driver.name')}
          type="text"
          fullWidth
          value={values.name || ''}
          onChange={handleChange}
          required
          errorMsg={errors.name}
        />
        <CustomTextField
          margin="normal"
          name="phone"
          id="phone"
          label={t('driver.phone')}
          type="text"
          fullWidth
          value={values.phone || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="documentType"
          id="documentType"
          label={t('driver.documentType')}
          type="text"
          fullWidth
          value={values.documentType || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="documentNumber"
          id="documentNumber"
          label={t('driver.documentNumber')}
          type="text"
          fullWidth
          value={values.documentNumber || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="employeeCode"
          id="employeeCode"
          label={t('driver.employeeCode')}
          type="text"
          fullWidth
          value={values.employeeCode || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="licenseNumber"
          id="licenseNumber"
          label={t('driver.licenseNumber')}
          type="text"
          fullWidth
          value={values.licenseNumber || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="licenseExpiresAt"
          id="licenseExpiresAt"
          label={t('driver.licenseExpiresAt')}
          type="date"
          fullWidth
          value={values.licenseExpiresAt ? values.licenseExpiresAt.substring(0, 10) : ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="defaultTransporterId"
          id="defaultTransporterId"
          label={t('driver.defaultTransporter')}
          type="text"
          fullWidth
          value={values.defaultTransporterId || ''}
          onChange={handleChange}
        />
        <CustomCheckbox
          name="active"
          id="active"
          value={values.active}
          handleChange={handleChange}
          label={t('generic.active')} />
      </form>
    </FormDialog>
  );
}

export default DriverDialog;
