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

import { useEffect, useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import FormDialog from "controls/Dialogs/FormDialog";
import CustomCheckbox from 'controls/Dialogs/CustomCheckbox';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import SearchSelect from 'edition/SearchSelect';
import { useAccountTransporterOptions } from 'edition/pickerOptions';
import { useTransporterNames } from 'queries/transporters';
import { DEFAULT_STAFF_TYPE, STAFF_TYPES, staffTypeLabelKey } from 'layouts/manageadmin/components/staff/staffTypes';

/**
 * Dialog/form state for a staff. Merges an API {@link Staff} (when editing)
 * with the fresh-add shape (`{ accountId, active }`); all fields are optional
 * and the `name` requirement (plus a resolved accountId) is enforced by the
 * screen's validate() gate before save.
 */
export interface StaffFormValues {
  version?: number;
  staffId?: string;
  accountId?: string;
  name?: string;
  phone?: string | null;
  documentType?: string | null;
  documentNumber?: string | null;
  employeeCode?: string | null;
  licenseNumber?: string | null;
  licenseExpiresAt?: string | null;
  defaultTransporterId?: string | null;
  staffType?: string | null;
  active?: boolean;
}

interface StaffDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: StaffFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

function StaffDialog({ open, setOpen, handleSubmit, values, handleChange, errors }: StaffDialogProps) {
  const { t } = useTranslation();
  const [pickedTransporterName, setPickedTransporterName] = useState<string | null>(null);
  const storedTransporterNames = useTransporterNames(
    open && values.defaultTransporterId && !pickedTransporterName ? [values.defaultTransporterId] : []
  );
  const transporterLabel =
    pickedTransporterName ?? storedTransporterNames.get(values.defaultTransporterId ?? '') ?? null;
  const staffTypeOptions = STAFF_TYPES.map((type) => ({ value: type, label: t(staffTypeLabelKey(type)) }));

  useEffect(() => {
    if (!open) setPickedTransporterName(null);
  }, [open]);

  return (
    <FormDialog
      title={values.staffId ? t('staff.update') : t('staff.create')}
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
          label={t('staff.name')}
          type="text"
          fullWidth
          value={values.name || ''}
          onChange={handleChange}
          required
          errorMsg={errors.name}
        />
        <CustomSelect
          list={staffTypeOptions}
          handleChange={handleChange}
          name="staffType"
          id="staffType"
          label={t('staff.staffType')}
          value={values.staffType || DEFAULT_STAFF_TYPE}
          numericValue={false}
          required
        />
        <CustomTextField
          margin="normal"
          name="phone"
          id="phone"
          label={t('staff.phone')}
          type="text"
          fullWidth
          value={values.phone || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="documentType"
          id="documentType"
          label={t('staff.documentType')}
          type="text"
          fullWidth
          value={values.documentType || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="documentNumber"
          id="documentNumber"
          label={t('staff.documentNumber')}
          type="text"
          fullWidth
          value={values.documentNumber || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="employeeCode"
          id="employeeCode"
          label={t('staff.employeeCode')}
          type="text"
          fullWidth
          value={values.employeeCode || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="licenseNumber"
          id="licenseNumber"
          label={t('staff.licenseNumber')}
          type="text"
          fullWidth
          value={values.licenseNumber || ''}
          onChange={handleChange}
        />
        <CustomTextField
          margin="normal"
          name="licenseExpiresAt"
          id="licenseExpiresAt"
          label={t('staff.licenseExpiresAt')}
          type="date"
          fullWidth
          value={values.licenseExpiresAt ? values.licenseExpiresAt.substring(0, 10) : ''}
          onChange={handleChange}
        />
        <SearchSelect
          id="defaultTransporterId"
          label={t('staff.defaultTransporter')}
          value={values.defaultTransporterId || null}
          valueLabel={transporterLabel}
          onChange={(option) => {
            setPickedTransporterName(option?.label ?? null);
            handleChange({ target: { name: 'defaultTransporterId', value: option?.value ?? '' } });
          }}
          useOptions={useAccountTransporterOptions}
          placeholder={t('workforce.assignments.selectTransporter')}
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

export default StaffDialog;
