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
import CustomTextField from 'controls/Dialogs/CustomTextField';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import CustomCheckbox from 'controls/Dialogs/CustomCheckbox';
import ArgonTypography from 'components/ArgonTypography';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import deviceTypes from 'data/deviceTypes';
import type { GpsOperator } from 'api/manager/operators';

export interface ManualDeviceFormValues {
  operatorId?: string;
  name?: string;
  serial?: string;
  deviceTypeId?: number;
  identifier?: number;
  description?: string;
  autoAssign?: boolean;
}

interface DeviceFormDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: ManualDeviceFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
  operators: GpsOperator[];
}

/**
 * Manual device registration for providers without a device-catalog API
 * (e.g. Prosegur): sync can never discover their devices, so the operator
 * enters them here. The name must be the provider's lookup key — for the
 * SOAP providers queried by plate that is the license plate.
 */
function DeviceFormDialog({ open, setOpen, handleSubmit, values, handleChange, errors, operators }: DeviceFormDialogProps) {
  const { t } = useTranslation();
  const operatorOptions = operators.map((o) => ({ value: o.operatorId, label: o.name }));
  const typeOptions = deviceTypes.map((dt) => ({
    value: dt.value,
    label: t(`deviceTypes.${dt.label}` as 'deviceTypes.cellular', { defaultValue: dt.label }),
  }));
  return (
    <FormDialog
          title={t('gpsIntegration.deviceForm.title')}
          handleSave={handleSubmit}
          open={open}
          setOpen={setOpen}
          maxWidth="md">
        <form>
          <CustomSelect
            list={operatorOptions}
            handleChange={handleChange}
            name="operatorId"
            id="operatorId"
            label={t('operator.title')}
            value={values.operatorId ?? ''}
            numericValue={false}
            required
            errorMsg={errors.operatorId}
          />
          <CustomTextField
            autoFocus
            margin="dense"
            name="name"
            id="name"
            label={t('device.name')}
            type="text"
            fullWidth
            value={values.name || ''}
            onChange={handleChange}
            required
            errorMsg={errors.name}
          />
          <ArgonTypography variant="caption" color="secondary">
            {t('gpsIntegration.deviceForm.nameHelp')}
          </ArgonTypography>
          <CustomTextField
            margin="normal"
            name="serial"
            id="serial"
            label={t('device.serial')}
            type="text"
            fullWidth
            value={values.serial || ''}
            onChange={handleChange}
            required
            errorMsg={errors.serial}
          />
          <CustomSelect
            list={typeOptions}
            handleChange={handleChange}
            name="deviceTypeId"
            id="deviceTypeId"
            label={t('device.type')}
            value={values.deviceTypeId ?? 0}
            required
            errorMsg={errors.deviceTypeId}
          />
          <CustomTextField
            margin="normal"
            name="identifier"
            id="identifier"
            label={t('device.identifier')}
            type="number"
            fullWidth
            value={values.identifier ?? 0}
            onChange={handleChange}
            helperText={t('gpsIntegration.deviceForm.identifierHelp')}
          />
          <CustomTextField
            margin="normal"
            name="description"
            id="description"
            label={t('device.description')}
            type="text"
            fullWidth
            value={values.description || ''}
            onChange={handleChange}
          />
          <CustomCheckbox
            handleChange={handleChange}
            name="autoAssign"
            id="autoAssign"
            value={values.autoAssign !== false}
            label={t('gpsIntegration.deviceForm.autoAssign')}
          />
          <ArgonTypography variant="caption" color="secondary" display="block">
            {t('gpsIntegration.deviceForm.autoAssignHelp')}
          </ArgonTypography>
        </form>
      </FormDialog>
  );
}

export default DeviceFormDialog;
