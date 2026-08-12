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

import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Table from "controls/Tables/Table";
import TableAccordion from "controls/Accordions/TableAccordion";
import UserFormDialog from 'layouts/manageadmin/components/users/UserDialog';
import PasswordFormDialog from 'layouts/manageadmin/components/users/PasswordDialog';
import useForm from 'controls/Dialogs/useForm';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import useUserTableData from "layouts/manageadmin/data/usersTableData";
import type {
  UserFormValues,
  PasswordFormValues,
  UserTableColumn,
  UserTableRow,
} from "layouts/manageadmin/data/usersTableData";

// Change event shape emitted by the vendored dialog controls.
type FormChangeHandler = (
  event: { target: { name: string; value: string; type?: string; checked?: boolean } }
) => void;


function ManageUsers() {
  const { t } = useTranslation();
  const handleAddClick = () => {
    setUserValues({ active: true });
    setUserErrors({});
  };

  const handleEditClick = (rowData: UserFormValues) => {
    setUserValues(rowData);
    setUserErrors({});
  };

  const handleUpdatePasswordClick = (rowData: PasswordFormValues) => {
    setPasswordValues(rowData);
    setPasswordErrors({});
  };

  const handleDeleteClick = (userId: string) => {
    setToDelete(userId);
  };

  const [expanded, setExpanded] = useState(false);
  const {
    data,
    open,
    openPassword,
    confirmOpen,
    onSave,
    onSavePassword,
    onDelete,
    setOpen,
    setOpenPassword,
    setConfirmOpen} = useUserTableData(expanded, handleEditClick, handleUpdatePasswordClick, handleDeleteClick);

  const [userValues, handleUserChange, setUserValues, setUserErrors, validateUser, userErrors] = useForm<UserFormValues>({});
  const [passwordValues, handlePasswordChange, setPasswordValues, setPasswordErrors, validatePassword, passwordErrors] = useForm<PasswordFormValues>({});
  const [toDelete, setToDelete] = useState<string | null>(null);
  const { columns, rows } = data;

  const handleSubmit = async () => {
    let requiredFields = userValues.hasOwnProperty('userId') ?
      ['emailAddress', 'username', 'firstName', 'lastName'] :
      ['emailAddress', 'username', 'firstName', 'lastName', 'password'];

    if (validateUser(requiredFields)) {
      onSave(userValues);
    }
  };

  const handleSubmitPassword = async () => {
    if (validatePassword(['password'])) {
      onSavePassword(passwordValues);
    }
  };

  return (
    <>
      <TableAccordion
        title={t('user.title')}
        showAddIcon={true}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <Table columns={columns} rows={rows} selectedField='user' />
      </TableAccordion>

      <UserFormDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={userValues}
        handleChange={handleUserChange}
        errors={userErrors}
      />

      <PasswordFormDialog
        open={openPassword}
        setOpen={setOpenPassword}
        handleSubmit={handleSubmitPassword}
        values={passwordValues}
        handleChange={handlePasswordChange}
        errors={passwordErrors}
      />

      <ConfirmDialog
        title={t('user.deleteTitle')}
        message={t('user.deleteMessage')}
        open={confirmOpen}
        setOpen={setConfirmOpen}
        onConfirm={async() => await onDelete(toDelete!)} />
    </>
  );
}

export default ManageUsers;
