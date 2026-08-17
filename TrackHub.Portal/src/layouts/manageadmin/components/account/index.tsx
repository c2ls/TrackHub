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
import TableAccordion from "controls/Accordions/TableAccordion";
import Table from "controls/Tables/Table";
import useForm from 'controls/Dialogs/useForm';
import useAccountTableData from "layouts/manageadmin/data/accountTableData";
import type {
  AccountFormValues,
  AccountTableColumn,
  AccountTableRow,
} from "layouts/manageadmin/data/accountTableData";
import AccountFormDialog from 'layouts/manageadmin/components/account/AccountDialog';
import { useTranslation } from 'react-i18next';

function ManageAccount() {
  const { t } = useTranslation();

  const handleEditClick = (rowData: AccountFormValues) => {
    setValues(rowData);
    setErrors({});
  };

  const [expanded, setExpanded] = useState(false);
  const { data, open, onSave, setOpen } = useAccountTableData(expanded, handleEditClick);
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<AccountFormValues>({});
  const { columns, rows } = data;

  const handleSubmit = async () => {
    if (validate(['name'])) {
      onSave(values);
    }
  };

  return (
    <>
      <TableAccordion
        title={t('account.title')}
        expanded={expanded}
        setExpanded={setExpanded}>
        <Table columns={columns} rows={rows} selectedField='name' />
      </TableAccordion>

      <AccountFormDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        errors={errors}
      />
    </>
  );
}

export default ManageAccount;
