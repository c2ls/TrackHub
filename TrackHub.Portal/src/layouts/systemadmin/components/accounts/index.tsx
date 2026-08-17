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
import AccountFormDialog from 'layouts/systemadmin/components/accounts/AccountsDialog';
import AccountStatusDialog from 'layouts/systemadmin/components/accounts/AccountStatusDialog';
import UserFormDialog from 'layouts/manageadmin/components/users/UserDialog';
import useForm from 'controls/Dialogs/useForm';
import useAccountsTableData from 'layouts/systemadmin/data/accountsTableData';
import type {
  AccountFormValues,
  AccountUserFormValues,
  AccountStatusFormValues,
} from 'layouts/systemadmin/data/accountsTableData';
import type { Account } from 'api/manager/accounts';
import { requiresReason } from 'data/accountStatuses';
import type { AccountStatusName } from 'data/accountStatuses';

function ManageAccounts() {
  const { t } = useTranslation();
  const handleAddClick = () => {
    setAccountValues({ active: true, typeId: 0 });
    setAccountErrors({});
  };

  const handleEditClick = (rowData: AccountFormValues) => {
    setAccountValues(rowData);
    setAccountErrors({});
  };

  const handleAddManagerClick = (accountId: string) => {
    setUserValues({ active: true, accountId });
    setUserErrors({});
  };

  const handleStatusClick = (account: Account) => {
    setStatusValues({ accountId: account.accountId, statusId: account.statusId, targetStatus: '', reason: '' });
    setStatusErrors({});
  };

  const [expanded, setExpanded] = useState(false);
  const {
    data,
    open,
    openUser,
    openStatus,
    onSave,
    onSaveUser,
    onChangeStatus,
    setOpen,
    setOpenUser,
    setOpenStatus} = useAccountsTableData(expanded, handleEditClick, handleAddManagerClick, handleStatusClick);

  const [accountValues, handleAccountChange, setAccountValues, setAccountErrors, validateAccount, accountErrors] = useForm<AccountFormValues>({});
  const [userValues, handleUserChange, setUserValues, setUserErrors, validateUser, userErrors] = useForm<AccountUserFormValues>({});
  const [statusValues, handleStatusChange, setStatusValues, setStatusErrors, validateStatus, statusErrors] = useForm<AccountStatusFormValues>({});
  const { columns, rows } = data;

  const handleSubmit = async () => {
    if (validateAccount(['name', 'typeId'])) {
      onSave(accountValues);
    }
  };

  const handleSubmitUser = async () => {
    if (validateUser(['emailAddress', 'firstName', 'lastName', 'password'])) {
      onSaveUser(userValues);
    }
  };

  const handleSubmitStatus = async () => {
    const required = ['targetStatus'];
    if (requiresReason(statusValues.targetStatus as AccountStatusName)) {
      required.push('reason');
    }
    if (validateStatus(required)) {
      onChangeStatus(statusValues);
    }
  };

  return (
    <>
      <TableAccordion
        title={t('account.title')}
        showAddIcon={true}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <Table columns={columns} rows={rows} selectedField='name'/>
      </TableAccordion>

      <AccountFormDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={accountValues}
        handleChange={handleAccountChange}
        errors={accountErrors}
      />

      <UserFormDialog
        open={openUser}
        setOpen={setOpenUser}
        handleSubmit={handleSubmitUser}
        values={userValues}
        handleChange={handleUserChange}
        errors={userErrors}
      />

      <AccountStatusDialog
        open={openStatus}
        setOpen={setOpenStatus}
        handleSubmit={handleSubmitStatus}
        values={statusValues}
        handleChange={handleStatusChange}
        errors={statusErrors}
      />
    </>
  );
}

export default ManageAccounts;
