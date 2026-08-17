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
import type { FormChangeHandler } from 'controls/Dialogs/useForm';

/** Dialog/form state for a toll vehicle class. */
export interface TollVehicleClassFormValues {
  tollVehicleClassId?: string;
  code?: string;
  name?: string;
  description?: string | null;
  sortOrder?: number | string;
}

interface TollVehicleClassDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: TollVehicleClassFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

function TollVehicleClassDialog({
  open,
  setOpen,
  handleSubmit,
  values,
  handleChange,
  errors,
}: TollVehicleClassDialogProps) {
  const { t } = useTranslation();
  return (
    <FormDialog
      title={t('tolls.catalog.classDetails')}
      handleSave={handleSubmit}
      open={open}
      setOpen={setOpen}
      maxWidth="sm"
    >
      <form>
        <CustomTextField
          autoFocus
          margin="dense"
          name="code"
          id="code"
          label={t('tolls.catalog.code')}
          type="text"
          value={values.code || ''}
          onChange={handleChange}
          errorMsg={errors.code}
          required
        />
        <CustomTextField
          margin="normal"
          name="name"
          id="name"
          label={t('tolls.catalog.name')}
          type="text"
          value={values.name || ''}
          onChange={handleChange}
          errorMsg={errors.name}
          required
        />
        <CustomTextField
          margin="normal"
          name="description"
          id="description"
          label={t('tolls.catalog.description')}
          type="text"
          multiline
          rows={2}
          value={values.description || ''}
          onChange={handleChange}
          errorMsg={errors.description}
        />
        <CustomTextField
          margin="normal"
          name="sortOrder"
          id="sortOrder"
          label={t('tolls.catalog.sortOrder')}
          type="number"
          value={values.sortOrder ?? 0}
          onChange={handleChange}
          errorMsg={errors.sortOrder}
        />
      </form>
    </FormDialog>
  );
}

export default TollVehicleClassDialog;
