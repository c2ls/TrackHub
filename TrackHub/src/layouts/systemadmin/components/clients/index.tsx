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
import ClientsFormDialog from 'layouts/systemadmin/components/clients/ClientsDialog';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import useForm from 'controls/Dialogs/useForm';
import useClientsTableData from 'layouts/systemadmin/data/clientsTableData';
import type { ClientFormValues } from 'layouts/systemadmin/data/clientsTableData';

function ManageClients() {
  const { t } = useTranslation();
  const handleAddClick = () => {
    setValues({});
    setErrors({});
  };

  const handleEditClick = (rowData: ClientFormValues) => {
    setValues(rowData);
    setErrors({});
  };

  const handleDeleteClick = (clientId: string) => {
    setToDelete(clientId);
  };

  const [expanded, setExpanded] = useState(false);
  const {
    data,
    users,
    open,
    confirmOpen,
    onSave,
    onDelete,
    setOpen,
    setConfirmOpen} = useClientsTableData(expanded, handleEditClick, handleDeleteClick);

  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<ClientFormValues>({});
  const [toDelete, setToDelete] = useState<string | null>(null);
  const { columns, rows } = data;

  const handleSubmit = async () => {
    if (validate(values.clientId ? ['name'] : ['name', 'secret'])) {
      onSave(values);
    }
  };

  return (
    <>
      <TableAccordion
        title={t('clients.title')}
        showAddIcon={true}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <Table columns={columns} rows={rows} selectedField='name'/>
      </TableAccordion>

      <ClientsFormDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        errors={errors}
        users={users} />

      <ConfirmDialog
        title={t('clients.deleteTitle')}
        message={t('clients.deleteMessage')}
        open={confirmOpen}
        setOpen={setConfirmOpen}
        onConfirm={async() => await onDelete(toDelete!)} />
    </>
  );
}

export default ManageClients;
