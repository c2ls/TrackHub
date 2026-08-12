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
import TransporterFormDialog from 'layouts/manageadmin/components/transporters/TransporterDialog';
import useForm from 'controls/Dialogs/useForm';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import useTransporterTableData from "layouts/manageadmin/data/transportersTableData";
import type {
  TransporterFormValues,
  TransporterTableColumn,
  TransporterTableRow,
} from "layouts/manageadmin/data/transportersTableData";

// Change event shape emitted by the vendored dialog controls.
type FormChangeHandler = (
  event: { target: { name: string; value: string; type?: string; checked?: boolean } }
) => void;


function ManageTransporters() {
  const { t } = useTranslation();

  const handleEditClick = (rowData: TransporterFormValues) => {
    setValues(rowData);
    setErrors({});
  };

  const handleDeleteClick = (transporterId: string) => {
    setToDelete(transporterId);
  };

  const [expanded, setExpanded] = useState(false);
  const {
    data,
    open,
    confirmOpen,
    onSave,
    onDelete,
    setOpen,
    setConfirmOpen } = useTransporterTableData(expanded, handleEditClick, handleDeleteClick);
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<TransporterFormValues>({});
  const [toDelete, setToDelete] = useState<string | null>(null);
  const { columns, rows } = data;

  const handleSubmit = async () => {
    if (validate(['name', 'transporterTypeId'])) {
      onSave(values);
    }
  };

  return (
    <>
      <TableAccordion
        title={t('transporter.title')}
        expanded={expanded}
        setOpen={setOpen}
        setExpanded={setExpanded}>
        <Table columns={columns} rows={rows} selectedField='name' />
      </TableAccordion>

      <TransporterFormDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        errors={errors}
      />

      <ConfirmDialog
        title={t('transporter.deleteTitle')}
        message={t('transporter.deleteMessage')}
        open={confirmOpen}
        setOpen={setConfirmOpen}
        onConfirm={async() => await onDelete(toDelete!)} />
    </>
  );
}

export default ManageTransporters;
