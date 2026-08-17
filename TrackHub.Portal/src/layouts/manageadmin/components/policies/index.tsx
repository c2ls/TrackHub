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
import PolicyAllocatorDialog from 'layouts/manageadmin/components/policies/PolicyAllocatorDialog';
import usePolicyTableData from "layouts/manageadmin/data/policiesTableData";
import type { PolicyTableColumn, PolicyTableRow } from "layouts/manageadmin/data/policiesTableData";

function ManagePolicies() {
  const { t } = useTranslation();

  const handleOpen = (policyId: number) => {
    setPolicyId(policyId);
    setOpen(true);
  };

  const [expanded, setExpanded] = useState(false);
  const { data } = usePolicyTableData(expanded, handleOpen);
  const { columns, rows } = data;
  const [policyId, setPolicyId] = useState(0);
  const [open, setOpen] = useState(false);

  return (
    <>
      <TableAccordion
        title={t('policy.title')}
        expanded={expanded}
        setOpen={setOpen}
        setExpanded={setExpanded}>
        <Table columns={columns} rows={rows} selectedField='name' />
      </TableAccordion>

      <PolicyAllocatorDialog
        open={open}
        setOpen={setOpen}
        policyId={policyId}
      />
    </>
  );
}

export default ManagePolicies;
