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

import Table from "controls/Tables/Table";
import useTransportersTableData from "layouts/dashboard/data/transportersData";
import type { Position } from "api/router/router";

interface TransportersTableProps {
  transporters: Position[];
  selected: string | null;
  handleSelected: (value: string | null) => void;
  searchQuery?: string;
  maxHeight?: string;
}

function TransportersTable({
    transporters,
    selected,
    handleSelected,
    searchQuery = '',
    maxHeight = 'calc(100vh - 400px)'}: TransportersTableProps) {
  const { data } = useTransportersTableData(transporters);
  const { columns, rows } = data;

  return (
      <Table
        columns={columns}
        rows={rows}
        selected={selected}
        selectedField="name"
        handleSelected={handleSelected}
        searchQuery={searchQuery}
        compact={true}
        scrollable={true}
        maxHeight={maxHeight}/>
  );
}

export default TransportersTable;
