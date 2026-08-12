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
import RoleAllocatorDialog from 'layouts/manageadmin/components/roles/RoleAllocatorDialog';
import useRoleTableData from "layouts/manageadmin/data/rolesTableData";
import type { RoleTableColumn, RoleTableRow } from "layouts/manageadmin/data/rolesTableData";

function ManageRoles() {
  const { t } = useTranslation();

  const handleOpen = (roleId: number) => {
    setRoleId(roleId);
    setOpen(true);
  };

  const [expanded, setExpanded] = useState(false);
  const { data } = useRoleTableData(expanded, handleOpen);
  const { columns, rows } = data;
  const [roleId, setRoleId] = useState(0);
  const [open, setOpen] = useState(false);

  return (
    <>
      <TableAccordion
        title={t('role.title')}
        expanded={expanded}
        setOpen={setOpen}
        setExpanded={setExpanded}>
        <Table columns={columns} rows={rows} selectedField='name' />
      </TableAccordion>

      <RoleAllocatorDialog
        open={open}
        setOpen={setOpen}
        roleId={roleId}
      />
    </>
  );
}

export default ManageRoles;
