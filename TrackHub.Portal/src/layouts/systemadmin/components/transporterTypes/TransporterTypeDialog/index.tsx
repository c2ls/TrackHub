/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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
import type { TransporterTypeFormValues } from 'layouts/systemadmin/data/transporterTypesTableData';

interface TransporterTypeFormDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: TransporterTypeFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

function TransporterTypeFormDialog({ open, setOpen, handleSubmit, values, handleChange, errors }: TransporterTypeFormDialogProps) {
  const { t } = useTranslation();
  return (
    <FormDialog
          title={t('transporterType.details')}
          handleSave={handleSubmit}
          open={open}
          setOpen={setOpen}
          maxWidth="md">
        <form>
          <CustomTextField
            autoFocus
            margin="dense"
            name="stoppedGap"
            id="stoppedGap"
            label={t('transporterType.stoppedGap')}
            type="number"
            fullWidth
            value={values.stoppedGap || 10}
            onChange={handleChange}
            errorMsg={errors.stoppedGap}
            required
          />

          <CustomTextField
            margin="dense"
            name="maxTimeGap"
            id="maxTimeGap"
            label={t('transporterType.maxTimeGap')}
            type="number"
            fullWidth
            value={values.maxTimeGap || 120}
            onChange={handleChange}
            errorMsg={errors.maxTimeGap}
            required
          />

        <CustomTextField
            margin="dense"
            name="maxDistance"
            id="maxDistance"
            label={t('transporterType.maxDistance')}
            type="number"
            fullWidth
            value={values.maxDistance || 10}
            onChange={handleChange}
            errorMsg={errors.maxDistance}
            required
          />

          <CustomCheckbox
            name="accBased"
            id="accBased"
            value={values.accBased}
            handleChange={handleChange}
            label={t('transporterType.accBased')} />
        </form>
      </FormDialog>
  );
}

export default TransporterTypeFormDialog;
