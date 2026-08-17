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

import type { ChangeEvent, Dispatch, SetStateAction } from "react";
import CustomPasswordField from "controls/Dialogs/CustomPasswordField";
import FormDialog from "controls/Dialogs/FormDialog";
import { useTranslation } from "react-i18next";

// Shared shape of the password-change form values (also consumed by ProfileInfoCard).
export interface PasswordFormValues {
  password?: string;
  confirmPassword?: string;
  userId?: string;
}

type FormChangeHandler = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => void;

interface PasswordChangeFormProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void;
  values: PasswordFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

function PasswordChangeForm({
  open,
  setOpen,
  handleSubmit,
  values,
  handleChange,
  errors,
}: PasswordChangeFormProps) {
  const { t } = useTranslation();

  return (
    <FormDialog
        title={t('user.updatePassword')}
        handleSave={handleSubmit}
        open={open}
        setOpen={setOpen}
        maxWidth="md">
        <form>
            <CustomPasswordField
                name="password"
                id="password"
                label={t("user.password")}
                fullWidth
                value={values.password || ''}
                onChange={handleChange}
                errorMsg={errors.password}/>
            <CustomPasswordField
                name="confirmPassword"
                id="confirmPassword"
                label={t("user.confirmPassword")}
                fullWidth
                value={values.confirmPassword || ''}
                onChange={handleChange}
                errorMsg={errors.confirmPassword}/>
        </form>
    </FormDialog>
  );
}

export default PasswordChangeForm;
