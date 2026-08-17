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
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import type { GroupFormValues } from 'layouts/manageadmin/data/groupsTableData';

interface GroupFormDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: GroupFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
}

function GroupFormDialog({ open, setOpen, handleSubmit, values, handleChange, errors }: GroupFormDialogProps) {
  const { t } = useTranslation();
  return (
    <FormDialog
          title={t('group.details')}
          handleSave={handleSubmit}
          open={open}
          setOpen={setOpen}
          maxWidth="md">
        <form>

          <CustomTextField
            margin="normal"
            name="name"
            id="name"
            label={t('group.name')}
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
            label={t('group.description')}
            type="text"
            fullWidth
            value={values.description || ''}
            onChange={handleChange}
            errorMsg={errors.description}
            required
          />

          <CustomCheckbox
            name="active"
            id="active"
            value={values.active}
            handleChange={handleChange}
            label={t('group.active')} />

        </form>
      </FormDialog>
  );
}

export default GroupFormDialog;
