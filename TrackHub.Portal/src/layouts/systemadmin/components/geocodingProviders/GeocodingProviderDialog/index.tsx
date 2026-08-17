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
import CustomPasswordField from 'controls/Dialogs/CustomPasswordField';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import { geocodingProviderTypes } from 'data/geocodingProviderTypes';
import { toCamelCase } from 'utils/stringUtils';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import type { GeocodingProviderFormValues } from 'layouts/systemadmin/data/geocodingProvidersTableData';

interface GeocodingProviderFormDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: GeocodingProviderFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

function GeocodingProviderFormDialog({ open, setOpen, handleSubmit, values, handleChange, errors }: GeocodingProviderFormDialogProps) {
  const { t } = useTranslation();
  const translatedTypes = geocodingProviderTypes.map(type => ({
    ...type,
    label: t(`geocodingProviders.types.${toCamelCase(type.label)}` as 'geocodingProviders.types.nominatim')
  }));
  return (
    <FormDialog
          title={t('geocodingProviders.details')}
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
            label={t('geocodingProviders.name')}
            type="text"
            fullWidth
            value={values.name || ''}
            onChange={handleChange}
            errorMsg={errors.name}
            required
          />

          <CustomSelect
            list={translatedTypes}
            handleChange={handleChange}
            name="type"
            id="type"
            label={t('geocodingProviders.type')}
            value={values.type}
            required
          />

          <CustomTextField
            margin="normal"
            name="endpointUri"
            id="endpointUri"
            label={t('geocodingProviders.endpointUri')}
            type="text"
            fullWidth
            value={values.endpointUri || ''}
            onChange={handleChange}
            errorMsg={errors.endpointUri}
            required
          />

          <CustomPasswordField
            margin="normal"
            name="apiKey"
            id="apiKey"
            label={t('geocodingProviders.apiKey')}
            fullWidth
            value={values.apiKey || ''}
            onChange={handleChange}
            errorMsg={errors.apiKey}
          />

          <CustomTextField
            margin="normal"
            name="requestsPerSecond"
            id="requestsPerSecond"
            label={t('geocodingProviders.requestsPerSecond')}
            type="number"
            fullWidth
            value={values.requestsPerSecond ?? 1}
            onChange={handleChange}
            errorMsg={errors.requestsPerSecond}
          />

          <CustomTextField
            margin="normal"
            name="timeoutSeconds"
            id="timeoutSeconds"
            label={t('geocodingProviders.timeoutSeconds')}
            type="number"
            fullWidth
            value={values.timeoutSeconds ?? 5}
            onChange={handleChange}
            errorMsg={errors.timeoutSeconds}
          />

          <CustomTextField
            margin="normal"
            name="configurationJson"
            id="configurationJson"
            label={t('geocodingProviders.configurationJson')}
            type="text"
            fullWidth
            multiline
            rows={4}
            value={values.configurationJson || ''}
            onChange={handleChange}
            errorMsg={errors.configurationJson}
          />

        </form>
      </FormDialog>
  );
}

export default GeocodingProviderFormDialog;
