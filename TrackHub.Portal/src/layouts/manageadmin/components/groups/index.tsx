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
import ServerPagination from "controls/Tables/ServerPagination";
import ServerSearch from "controls/Tables/ServerSearch";
import useServerList, { useClampPage } from "controls/Tables/useServerList";
import TableAccordion from "controls/Accordions/TableAccordion";
import GroupFormDialog from 'layouts/manageadmin/components/groups/GroupDialog';
import TransporterAllocatorDialog from 'layouts/manageadmin/components/groups/TransporterAllocatorDialog';
import UserAllocatorDialog from 'layouts/manageadmin/components/groups/UserAllocatorDialog';
import useForm from 'controls/Dialogs/useForm';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import useGroupTableData from "layouts/manageadmin/data/groupsTableData";
import type { GroupFormValues } from "layouts/manageadmin/data/groupsTableData";

const PAGE_SIZE = 10;

function ManageGroups() {
  const { t } = useTranslation();
  const handleAddClick = () => {
    setValues({ active: true });
    setErrors({});
  };

  const handleEditClick = (rowData: GroupFormValues) => {
    setValues(rowData);
    setErrors({});
  };

  const handleDeleteClick = (groupId: number) => {
    setToDelete(groupId);
  };

  const handleUserClick = (groupId: number) => {
    setGroupId(groupId);
    setOpenUserAllocator(true);
  };

  const handleTransporterClick = (groupId: number) => {
    setGroupId(groupId);
    setOpenTransporterAllocator(true);
  };

  const [expanded, setExpanded] = useState(false);
  const { page, setPage, searchDraft, setSearchDraft, params } = useServerList(PAGE_SIZE);
  const {
    data,
    totalCount,
    open,
    confirmOpen,
    onSave,
    onDelete,
    setOpen,
    setConfirmOpen} = useGroupTableData(expanded, handleEditClick, handleDeleteClick, handleUserClick, handleTransporterClick, params);
  useClampPage(page, PAGE_SIZE, totalCount, setPage);

  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<GroupFormValues>({});
  const [toDelete, setToDelete] = useState<number | null>(null);
  const { columns, rows } = data;
  const [groupId, setGroupId] = useState(0);
  const [openUserAllocator, setOpenUserAllocator] = useState(false);
  const [openTransporterAllocator, setOpenTransporterAllocator] = useState(false);

  const handleSubmit = async () => {
    const requiredFields = ['name', 'description'];

    if (validate(requiredFields)) {
      onSave(values);
    }
  };

  return (
    <>
      <TableAccordion
        title={t('group.title')}
        showAddIcon={true}
        expanded={expanded}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}>
        <ServerSearch value={searchDraft} onChange={setSearchDraft} />
        <Table columns={columns} rows={rows} selectedField='group' serverPaged />
        <ServerPagination
          page={page}
          pageSize={PAGE_SIZE}
          totalCount={totalCount}
          pageLength={rows.length}
          onPageChange={setPage}
        />
      </TableAccordion>

      <GroupFormDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={handleSubmit}
        values={values}
        handleChange={handleChange}
        errors={errors}
      />

      <ConfirmDialog
        title={t('group.deleteTitle')}
        message={t('group.deleteMessage')}
        open={confirmOpen}
        setOpen={setConfirmOpen}
        onConfirm={async() => await onDelete(toDelete!)} />

      <TransporterAllocatorDialog
        open={openTransporterAllocator}
        setOpen={setOpenTransporterAllocator}
        groupId={groupId}
      />

      <UserAllocatorDialog
        open={openUserAllocator}
        setOpen={setOpenUserAllocator}
        groupId={groupId}
      />

    </>
  );
}

export default ManageGroups;
