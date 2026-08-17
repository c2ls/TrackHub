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
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import useDeviceTableData from "layouts/manageadmin/data/devicesTableData";

const PAGE_SIZE = 10;

function ManageDevices() {
  const { t } = useTranslation();

  const handleDeleteClick = (deviceId: string) => {
    setToDelete(deviceId);
  };

  const [expanded, setExpanded] = useState(false);
  const { page, setPage, searchDraft, setSearchDraft, params } = useServerList(PAGE_SIZE);
  const {
    data,
    totalCount,
    confirmOpen,
    onDelete,
    setConfirmOpen } = useDeviceTableData(expanded, handleDeleteClick, params);
  useClampPage(page, PAGE_SIZE, totalCount, setPage);
  const [toDelete, setToDelete] = useState<string | null>(null);
  const { columns, rows } = data;

  return (
    <>
      <TableAccordion
        title={t('device.title')}
        expanded={expanded}
        setExpanded={setExpanded}>
        <ServerSearch value={searchDraft} onChange={setSearchDraft} />
        <Table columns={columns} rows={rows} selectedField='name' serverPaged />
        <ServerPagination
          page={page}
          pageSize={PAGE_SIZE}
          totalCount={totalCount}
          pageLength={rows.length}
          onPageChange={setPage}
        />
      </TableAccordion>

      <ConfirmDialog
        title={t('device.deleteTitle')}
        message={t('device.deleteMessage')}
        open={confirmOpen}
        setOpen={setConfirmOpen}
        onConfirm={async() => await onDelete(toDelete!)} />

    </>
  );
}

export default ManageDevices;
