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
import PoiFormDialog from 'layouts/manageadmin/components/pois/PoiDialog';
import useForm from 'controls/Dialogs/useForm';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import usePoiTableData from "layouts/manageadmin/data/poisTableData";
import type { PoiFormValues, PoiColumn, PoiRow } from "layouts/manageadmin/data/poisTableData";

// Change event shape emitted by the vendored dialog controls.
type FormChangeHandler = (
  event: { target: { name: string; value: string; type?: string; checked?: boolean } }
) => void;


function ManagePois() {
  const { t } = useTranslation();
  const handleAddClick = () => {
    setValues({ active: true });
    setErrors({});
  };

  const handleEditClick = (rowData: PoiFormValues) => {
    setValues(rowData);
    setErrors({});
  };

  const handleDeleteClick = (pointOfInterestId: string) => {
    setToDelete(pointOfInterestId);
  };

  const [expanded, setExpanded] = useState(false);
  const {
    data,
    groupOptions,
    open,
    confirmOpen,
    onSave,
    onDelete,
    setOpen,
    setConfirmOpen} = usePoiTableData(expanded, handleEditClick, handleDeleteClick);

  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<PoiFormValues>({});
  const [toDelete, setToDelete] = useState<string | null>(null);
  const { columns, rows } = data;

  const handleSubmit = async () => {
    const requiredFields = ['name', 'type', 'latitude', 'longitude'];

    if (!validate(requiredFields)) {
      return;
    }

    const latitude = parseFloat(values.latitude as string);
    const longitude = parseFloat(values.longitude as string);
    const newErrors: Record<string, string> = {};
    if (Number.isNaN(latitude) || latitude < -90 || latitude > 90) {
      newErrors.latitude = t('poi.invalidLatitude');
    }
    if (Number.isNaN(longitude) || longitude < -180 || longitude > 180) {
      newErrors.longitude = t('poi.invalidLongitude');
    }
    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    onSave({ ...values, latitude, longitude });
  };

  return (
    <>
      <TableAccordion
        title={t('poi.title')}
        showAddIcon={true}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <Table columns={columns} rows={rows} selectedField='name' />
      </TableAccordion>

      <PoiFormDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        errors={errors}
        groupOptions={groupOptions}
      />

      <ConfirmDialog
        title={t('poi.deleteTitle')}
        message={t('poi.deleteMessage')}
        open={confirmOpen}
        setOpen={setConfirmOpen}
        onConfirm={async() => await onDelete(toDelete!)} />

    </>
  );
}

export default ManagePois;
