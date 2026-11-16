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

import { useEffect } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import type { ParseKeys } from 'i18next';
import FormDialog from "controls/Dialogs/FormDialog";
import CustomTextField from 'controls/Dialogs/CustomTextField';
import CustomPasswordField from 'controls/Dialogs/CustomPasswordField';
import { useProviderCapabilities } from 'queries/router';
import type { CredentialFormValues } from 'layouts/gpsintegration/data/operatorsTableData';

// Change event shape emitted by the vendored dialog controls.
type FormChangeHandler = (
  event: { target: { name: string; value: string; type?: string; checked?: boolean } }
) => void;

type CredentialField = 'username' | 'password' | 'key' | 'key2';

const DEFAULT_FIELD_NAMES: Record<CredentialField, ParseKeys> = {
  username: 'credential.username',
  password: 'credential.password',
  key: 'credential.key',
  key2: 'credential.key2',
};

// What each provider calls the generic credential fields, keyed by the Router's protocol name.
const PROVIDER_FIELD_NAMES: Record<string, Partial<Record<CredentialField | 'uriHelp', ParseKeys>>> = {
  CommandTrack: { key: 'credential.providers.CommandTrack.key' },
  GeoTab: { key: 'credential.providers.GeoTab.key' },
  GpsGate: {
    password: 'credential.providers.GpsGate.password',
    key: 'credential.providers.GpsGate.key',
    key2: 'credential.providers.GpsGate.key2',
  },
  Tracksolid: {
    username: 'credential.providers.Tracksolid.username',
    key: 'credential.providers.Tracksolid.key',
    key2: 'credential.providers.Tracksolid.key2',
    uriHelp: 'credential.providers.Tracksolid.uriHelp',
  },
  Mettax: {
    key: 'credential.providers.Mettax.key',
    key2: 'credential.providers.Mettax.key2',
    uriHelp: 'credential.providers.Mettax.uriHelp',
  },
  TrustTrack: { key: 'credential.providers.TrustTrack.key' },
  Mapon: { key: 'credential.providers.Mapon.key' },
  GpsTrackit: { key: 'credential.providers.GpsTrackit.key' },
  Motive: { key: 'credential.providers.Motive.key' },
  Cartrack: {
    password: 'credential.providers.Cartrack.password',
    uriHelp: 'credential.providers.Cartrack.uriHelp',
  },
  Pegasus: { uriHelp: 'credential.providers.Pegasus.uriHelp' },
  Webfleet: { key: 'credential.providers.Webfleet.key', key2: 'credential.providers.Webfleet.key2' },
  RedGps: { key: 'credential.providers.RedGps.key' },
  Plaspy: { key: 'credential.providers.Plaspy.key' },
};

interface CredentialFormDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: CredentialFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

function CredentialFormDialog({ open, setOpen, handleSubmit, values, handleChange, errors }: CredentialFormDialogProps) {
  const { t } = useTranslation();
  const { data: providers } = useProviderCapabilities({ enabled: open });
  const provider = providers?.find((p) => p.protocolTypeId === values.protocolTypeId);
  const names = PROVIDER_FIELD_NAMES[provider?.protocol ?? ''] ?? {};
  const label = (field: CredentialField) => t(names[field] ?? DEFAULT_FIELD_NAMES[field]);
  const uriHelp = names.uriHelp ? t(names.uriHelp) : undefined;
  const defaultUri = provider?.defaultUri;
  const asks = (field: Uppercase<CredentialField>) => !provider || provider.credentialFields.includes(field);

  useEffect(() => {
    if (open && defaultUri && !values.uri) {
      handleChange({ target: { name: 'uri', value: defaultUri } });
    }
    // Only when the dialog opens for a provider: a URL the user clears must stay cleared.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, defaultUri]);

  return (
    <FormDialog
          title={t('credential.title')}
          handleSave={handleSubmit}
          open={open}
          setOpen={setOpen}
          maxWidth="md">
        <form>
          <CustomTextField
            autoFocus
            margin="dense"
            name="uri"
            id="uri"
            label="URL"
            type="text"
            fullWidth
            value={values.uri || ''}
            onChange={handleChange}
            required
            errorMsg={errors.uri}
            helperText={uriHelp}
          />
          {asks('USERNAME') && (
            <CustomTextField
              margin="normal"
              name="username"
              id="username"
              label={label('username')}
              type="text"
              fullWidth
              value={values.username || ''}
              onChange={handleChange}
            />
          )}

          {asks('PASSWORD') && (
            <CustomPasswordField
              margin="normal"
              name="password"
              id="password"
              label={label('password')}
              fullWidth
              value={values.password || ''}
              onChange={handleChange}
            />
          )}

          {asks('KEY') && (
            <CustomPasswordField
              margin="normal"
              name="key"
              id="key"
              label={label('key')}
              fullWidth
              value={values.key || ''}
              onChange={handleChange}
            />
          )}

          {asks('KEY2') && (
            <CustomPasswordField
              margin="normal"
              name="key2"
              id="key2"
              label={label('key2')}
              fullWidth
              value={values.key2 || ''}
              onChange={handleChange}
            />
          )}
        </form>
      </FormDialog>
  );
}

export default CredentialFormDialog;
