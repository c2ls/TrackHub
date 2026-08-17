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
import Grid from '@mui/material/Grid';
import FormDialog from 'controls/Dialogs/FormDialog';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import ArgonTypography from 'components/ArgonTypography';
import ArgonBox from 'components/ArgonBox';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import type { TollVehicleClass } from 'api/tripManagement/trips';

/** Dialog/form state for an effective-dated tariff row. */
export interface TollTariffFormValues {
  tollTariffId?: string;
  tollStationId?: string;
  tollVehicleClassCode?: string;
  amount?: number | string;
  currency?: string;
  effectiveFrom?: string;
  effectiveTo?: string | null;
}

interface TollTariffDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: TollTariffFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
  vehicleClasses: TollVehicleClass[];
}

function TollTariffDialog({
  open,
  setOpen,
  handleSubmit,
  values,
  handleChange,
  errors,
  vehicleClasses,
}: TollTariffDialogProps) {
  const { t } = useTranslation();
  const classOptions = vehicleClasses.map((vehicleClass) => ({
    value: vehicleClass.code,
    label: `${vehicleClass.code} — ${vehicleClass.name}`,
  }));

  return (
    <FormDialog
      title={t('tolls.catalog.tariffDetails')}
      handleSave={handleSubmit}
      open={open}
      setOpen={setOpen}
      maxWidth="sm"
    >
      <form>
        <CustomSelect
          list={classOptions}
          handleChange={handleChange}
          name="tollVehicleClassCode"
          id="tollVehicleClassCode"
          label={t('tolls.vehicleClass')}
          value={values.tollVehicleClassCode}
          numericValue={false}
          placeholder={t('tolls.selectVehicleClass')}
          required
          errorMsg={errors.tollVehicleClassCode}
        />
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 8 }}>
            <CustomTextField
              margin="dense"
              name="amount"
              id="amount"
              label={t('tolls.amount')}
              type="number"
              value={values.amount ?? ''}
              onChange={handleChange}
              errorMsg={errors.amount}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <CustomTextField
              margin="dense"
              name="currency"
              id="currency"
              label={t('tolls.currency')}
              type="text"
              value={values.currency || ''}
              onChange={handleChange}
              errorMsg={errors.currency}
              required
              slotProps={{ htmlInput: { maxLength: 3 } }}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="effectiveFrom"
              id="effectiveFrom"
              label={t('tolls.catalog.effectiveFrom')}
              type="date"
              value={values.effectiveFrom || ''}
              onChange={handleChange}
              errorMsg={errors.effectiveFrom}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="effectiveTo"
              id="effectiveTo"
              label={t('tolls.catalog.effectiveTo')}
              type="date"
              value={values.effectiveTo || ''}
              onChange={handleChange}
              errorMsg={errors.effectiveTo}
            />
          </Grid>
        </Grid>
        <ArgonBox mt={1}>
          <ArgonTypography variant="caption" color="secondary">
            {t('tolls.catalog.historyHint')}
          </ArgonTypography>
        </ArgonBox>
      </form>
    </FormDialog>
  );
}

export default TollTariffDialog;
