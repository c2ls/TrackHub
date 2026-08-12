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
import CustomCheckbox from 'controls/Dialogs/CustomCheckbox';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import CustomPasswordField from 'controls/Dialogs/CustomPasswordField';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import type { UserFormValues } from 'layouts/manageadmin/data/usersTableData';

interface UserFormDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: UserFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

function UserFormDialog({ open, setOpen, handleSubmit, values, handleChange, errors }: UserFormDialogProps) {
  const { t } = useTranslation();
  return (
    <FormDialog
          title={t('user.details')}
          handleSave={handleSubmit}
          open={open}
          setOpen={setOpen}
          maxWidth="md">
        <form>
          <CustomTextField
            autoFocus
            margin="dense"
            name="emailAddress"
            id="emailAddress"
            label={t('user.emailAddress')}
            type="email"
            fullWidth
            value={values.emailAddress || ''}
            onChange={handleChange}
            required
            errorMsg={errors.emailAddress}
          />

          {!values.userId && (
            <CustomPasswordField
                margin="normal"
                name="password"
                id="password"
                label={t('user.password')}
                fullWidth
                value={values.password || ''}
                onChange={handleChange}
                errorMsg={errors.password}
          />)}

          <CustomTextField
            margin="normal"
            name="username"
            id="username"
            label={t('user.username')}
            type="text"
            fullWidth
            value={values.username || ''}
            onChange={handleChange}
          />

          <CustomTextField
            margin="normal"
            name="firstName"
            id="firstName"
            label={t('user.firstName')}
            type="text"
            fullWidth
            value={values.firstName || ''}
            onChange={handleChange}
            errorMsg={errors.firstName}
            required
          />

          <CustomTextField
            margin="normal"
            name="secondName"
            id="secondName"
            label={t('user.secondName')}
            type="text"
            fullWidth
            value={values.secondName || ''}
            onChange={handleChange}
          />

          <CustomTextField
            margin="normal"
            name="lastName"
            id="lastName"
            label={t('user.lastName')}
            type="text"
            fullWidth
            value={values.lastName || ''}
            onChange={handleChange}
            errorMsg={errors.lastName}
            required
          />

          <CustomTextField
            margin="normal"
            name="secondSurname"
            id="secondSurname"
            label={t('user.secondSurname')}
            type="text"
            fullWidth
            value={values.secondSurname || ''}
            onChange={handleChange}
          />

          <CustomTextField
            margin="normal"
            name="dob"
            id="dob"
            label={t('user.dob')}
            type="date"
            fullWidth
            value={values.dob || ''}
            onChange={handleChange}
          />

          <CustomCheckbox
            name="active"
            id="active"
            value={values.active}
            handleChange={handleChange}
            label={t('user.active')} />

          <CustomCheckbox
            name="integrationUser"
            id="integrationUser"
            value={values.integrationUser}
            handleChange={handleChange}
            label={t('user.integrationUser')} />

        </form>
      </FormDialog>
  );
}

export default UserFormDialog;
