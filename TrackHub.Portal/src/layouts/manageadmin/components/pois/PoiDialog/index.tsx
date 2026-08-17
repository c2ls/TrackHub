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
import CustomSelect from 'controls/Dialogs/CustomSelect';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import { poiTypes } from 'data/poiTypes';
import { colors } from 'data/colors';
import { toCamelCase } from 'utils/stringUtils';
import type { PoiFormValues, PoiGroupOption } from 'layouts/manageadmin/data/poisTableData';

interface PoiFormDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: PoiFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
  groupOptions: PoiGroupOption[];
}

function PoiFormDialog({ open, setOpen, handleSubmit, values, handleChange, errors, groupOptions }: PoiFormDialogProps) {
  const { t } = useTranslation();
  const translatedTypes = poiTypes.map(type => ({
    ...type,
    label: t(`poi.types.${toCamelCase(type.label)}` as 'poi.types.clientSite')
  }));
  const translatedColors = colors.map(color => ({
    ...color,
    label: t(`colors.${color.label.toLowerCase()}` as 'colors.red')
  }));
  return (
    <FormDialog
          title={t('poi.details')}
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
            label={t('poi.name')}
            type="text"
            fullWidth
            value={values.name || ''}
            onChange={handleChange}
            errorMsg={errors.name}
            required
          />

          <CustomTextField
            margin="normal"
            name="description"
            id="description"
            label={t('poi.description')}
            type="text"
            fullWidth
            value={values.description || ''}
            onChange={handleChange}
            errorMsg={errors.description}
          />

          <CustomSelect
            list={translatedTypes}
            handleChange={handleChange}
            name="type"
            id="type"
            label={t('poi.type')}
            value={values.type}
            required
          />

          <CustomTextField
            margin="normal"
            name="latitude"
            id="latitude"
            label={t('poi.latitude')}
            type="number"
            fullWidth
            value={values.latitude ?? ''}
            onChange={handleChange}
            errorMsg={errors.latitude}
            required
          />

          <CustomTextField
            margin="normal"
            name="longitude"
            id="longitude"
            label={t('poi.longitude')}
            type="number"
            fullWidth
            value={values.longitude ?? ''}
            onChange={handleChange}
            errorMsg={errors.longitude}
            required
          />

          <CustomTextField
            margin="normal"
            name="address"
            id="address"
            label={t('poi.address')}
            type="text"
            fullWidth
            value={values.address || ''}
            onChange={handleChange}
            errorMsg={errors.address}
          />

          <CustomSelect
            list={translatedColors}
            handleChange={handleChange}
            name="color"
            id="color"
            label={t('poi.color')}
            value={values.color ?? undefined}
          />

          <CustomSelect
            list={groupOptions}
            handleChange={handleChange}
            name="groupId"
            id="groupId"
            label={t('poi.group')}
            value={values.groupId ?? undefined}
          />

          <CustomCheckbox
            name="active"
            id="active"
            value={values.active}
            handleChange={handleChange}
            label={t('poi.active')} />

        </form>
      </FormDialog>
  );
}

export default PoiFormDialog;
