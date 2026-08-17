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
import CustomCheckbox from 'controls/Dialogs/CustomCheckbox';
import CustomPasswordField from 'controls/Dialogs/CustomPasswordField';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import ArgonTypography from 'components/ArgonTypography';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';

/** Which credential lifecycle step the dialog is collecting input for. */
export type CredentialDialogMode = 'create' | 'activate' | 'lock' | 'reset';

/** Dialog/form state for the driver credential lifecycle operations. */
export interface CredentialFormValues {
  login?: string;
  password?: string;
  active?: boolean;
  resetRequired?: boolean;
  lockedUntil?: string;
}

interface DriverCredentialDialogProps {
  mode: CredentialDialogMode;
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: CredentialFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

function DriverCredentialDialog({
  mode,
  open,
  setOpen,
  handleSubmit,
  values,
  handleChange,
  errors,
}: DriverCredentialDialogProps) {
  const { t } = useTranslation();

  const titles: Record<CredentialDialogMode, string> = {
    create: t('workforce.credentials.create'),
    // Wires the long-dangling driver.activation key to its first consumer.
    activate: t('driver.activation'),
    lock: t('workforce.credentials.lockTitle'),
    reset: t('workforce.credentials.resetTitle'),
  };

  return (
    <FormDialog title={titles[mode]} handleSave={handleSubmit} open={open} setOpen={setOpen}>
      <form>
        {mode === 'create' && (
          <CustomTextField
            autoFocus
            margin="dense"
            name="login"
            id="credentialLogin"
            label={t('workforce.credentials.login')}
            type="text"
            fullWidth
            value={values.login || ''}
            onChange={handleChange}
            required
            errorMsg={errors.login}
          />
        )}

        {mode !== 'lock' && (
          <CustomPasswordField
            margin="normal"
            name="password"
            id="credentialPassword"
            label={
              mode === 'create'
                ? t('workforce.credentials.password')
                : t('workforce.credentials.newPassword')
            }
            value={values.password || ''}
            onChange={handleChange}
            required
            errorMsg={errors.password}
          />
        )}

        {mode === 'lock' && (
          <>
            <CustomTextField
              autoFocus
              margin="dense"
              name="lockedUntil"
              id="credentialLockedUntil"
              label={t('workforce.credentials.lockedUntil')}
              type="datetime-local"
              fullWidth
              value={values.lockedUntil || ''}
              onChange={handleChange}
              required
              errorMsg={errors.lockedUntil}
            />
            <ArgonTypography variant="caption" color="secondary">
              {t('workforce.credentials.lockedUntilHelp')}
            </ArgonTypography>
          </>
        )}

        {mode === 'create' && (
          <CustomCheckbox
            name="active"
            id="credentialActive"
            value={values.active}
            handleChange={handleChange}
            label={t('generic.active')}
          />
        )}

        {(mode === 'create' || mode === 'reset') && (
          <CustomCheckbox
            name="resetRequired"
            id="credentialResetRequired"
            value={values.resetRequired}
            handleChange={handleChange}
            label={t('workforce.credentials.resetRequired')}
          />
        )}
      </form>
    </FormDialog>
  );
}

export default DriverCredentialDialog;
