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

import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Table from "controls/Tables/Table";
import TableAccordion from "controls/Accordions/TableAccordion";
import GeocodingProviderFormDialog from 'layouts/systemadmin/components/geocodingProviders/GeocodingProviderDialog';
import useForm from 'controls/Dialogs/useForm';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import useGeocodingProvidersTableData from 'layouts/systemadmin/data/geocodingProvidersTableData';
import type { GeocodingProviderFormValues } from 'layouts/systemadmin/data/geocodingProvidersTableData';

function ManageGeocodingProviders() {
  const { t } = useTranslation();
  const handleAddClick = () => {
    setValues({ requestsPerSecond: 1, timeoutSeconds: 5, active: false });
    setErrors({});
  };

  const handleEditClick = (rowData: GeocodingProviderFormValues) => {
    setValues(rowData);
    setErrors({});
  };

  const handleDeleteClick = (geocodingProviderId: string) => {
    setToDelete(geocodingProviderId);
  };

  const [expanded, setExpanded] = useState(false);
  const {
    data,
    open,
    confirmOpen,
    onSave,
    onDelete,
    setOpen,
    setConfirmOpen} = useGeocodingProvidersTableData(expanded, handleEditClick, handleDeleteClick);

  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<GeocodingProviderFormValues>({});
  const [toDelete, setToDelete] = useState<string | null>(null);
  const { columns, rows } = data;

  const handleSubmit = async () => {
    const requiredFields = ['name', 'type', 'endpointUri'];

    if (validate(requiredFields)) {
      onSave({
        ...values,
        requestsPerSecond: parseInt(String(values.requestsPerSecond), 10) || 1,
        timeoutSeconds: parseInt(String(values.timeoutSeconds), 10) || 5
      });
    }
  };

  return (
    <>
      <TableAccordion
        title={t('geocodingProviders.title')}
        showAddIcon={true}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <Table columns={columns} rows={rows} selectedField='name' />
      </TableAccordion>

      <GeocodingProviderFormDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        errors={errors}
      />

      <ConfirmDialog
        title={t('geocodingProviders.deleteTitle')}
        message={t('geocodingProviders.deleteMessage')}
        open={confirmOpen}
        setOpen={setConfirmOpen}
        onConfirm={async() => await onDelete(toDelete!)} />

    </>
  );
}

export default ManageGeocodingProviders;
