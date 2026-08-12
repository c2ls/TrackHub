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
import CustomCheckbox from 'controls/Dialogs/CustomCheckbox';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import FormDialog from "controls/Dialogs/FormDialog";
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import { geofenceTypes } from 'data/geofenceTypes';
import { colors } from 'data/colors';
import { toCamelCase } from 'utils/stringUtils';
import type { GeofenceFormValues } from 'layouts/geofencemanager/data/geofencesData';

/** A translated option for the type/color selects. */
interface SelectOption { value: number; label: string; }

interface GeofenceFormDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void;
  handleCancel: () => void;
  values: GeofenceFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

function GeofenceFormDialog({ open, setOpen, handleSubmit, handleCancel, values, handleChange, errors }: GeofenceFormDialogProps) {
  const { t } = useTranslation();
  const translatedColors: SelectOption[] = colors.map(color => ({
    value: color.value,
    label: t(`colors.${color.label.toLowerCase()}` as 'colors.red')
  }));
  const translatedTypes: SelectOption[] = geofenceTypes.map(type => ({
    value: type.value,
    label: t(`geofenceTypes.${toCamelCase(type.label)}` as 'geofenceTypes.office')
  }));
  return (
    <FormDialog 
      title={t('user.details')}
      handleSave={handleSubmit}
      handleCancel={handleCancel}
      open={open}
      setOpen={setOpen}
      maxWidth="md">
        <form>
            <CustomTextField
              autoFocus
              margin="dense"
              name="name"
              id="name"
              label={t('geofence.name')}
              type="text"
              fullWidth
              value={values.name || ''}
              onChange={handleChange}
              required
              errorMsg={errors.name}
            />

            <CustomTextField
              margin="normal"
              name="description"
              id="description"
              label={t('geofence.description')}
              type="text"
              fullWidth
              value={values.description || ''}
              onChange={handleChange}
            />

            <CustomSelect
              list={translatedTypes}
              handleChange={handleChange}
              name="type"
              id="type"
              label={t('geofence.type')}
              value={values.type}
              required
            />

            <CustomSelect
              list={translatedColors}
              handleChange={handleChange}
              name="color"
              id="color"
              label={t('geofence.color')}
              value={values.color}
              required
            />

            {values.shape === 'circle' && (
              <>
                <CustomTextField
                  margin="normal"
                  name="circleRadiusMeters"
                  id="circleRadiusMeters"
                  label={t('geofence.radius')}
                  type="number"
                  fullWidth
                  value={values.circleRadiusMeters ?? ''}
                  onChange={handleChange}
                  helperText={errors.circleRadiusMeters || t('geofence.radiusHelp')}
                  error={!!errors.circleRadiusMeters}
                  inputProps={{ min: 10, max: 100000 }}
                />
                <CustomTextField
                  margin="normal"
                  name="circleCenter"
                  id="circleCenter"
                  label={t('geofence.center')}
                  type="text"
                  fullWidth
                  value={
                    values.circleCenter
                      ? `${values.circleCenter.latitude.toFixed(6)}, ${values.circleCenter.longitude.toFixed(6)}`
                      : ''
                  }
                  slotProps={{ input: { readOnly: true } }}
                />
              </>
            )}

            <CustomTextField
              margin="normal"
              name="dwellThresholdMinutes"
              id="dwellThresholdMinutes"
              label={t('geofence.dwellThreshold')}
              type="number"
              fullWidth
              value={values.dwellThresholdMinutes ?? ''}
              onChange={handleChange}
              helperText={errors.dwellThresholdMinutes || t('geofence.dwellThresholdHelp')}
              error={!!errors.dwellThresholdMinutes}
              inputProps={{ min: 1, max: 10080 }}
            />

            <CustomCheckbox
            name="active"
            id="active"
            value={values.active}
            handleChange={handleChange}
            label={t('geofence.active')} />

            <CustomCheckbox
            name="alertOnEntry"
            id="alertOnEntry"
            value={values.alertOnEntry}
            handleChange={handleChange}
            label={t('geofence.alertOnEntry')} />

            <CustomCheckbox
            name="alertOnExit"
            id="alertOnExit"
            value={values.alertOnExit}
            handleChange={handleChange}
            label={t('geofence.alertOnExit')} />
        </form>
      </FormDialog>
  );
}

export default GeofenceFormDialog;