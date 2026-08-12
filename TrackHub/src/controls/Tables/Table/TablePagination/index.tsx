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

import type { ChangeEventHandler, MouseEvent } from "react";
import { Box } from "@mui/material";
import TablePaginationStyle from "controls/Tables/styles/TablePagination";

export interface TablePaginationProps {
  count: number;
  page: number;
  rowsPerPage: number;
  handleChangePage: (event: MouseEvent<HTMLButtonElement> | null, page: number) => void;
  handleChangeRowsPerPage: ChangeEventHandler<HTMLInputElement | HTMLTextAreaElement>;
  compact?: boolean;
}

const TablePagination = ({
  count,
  page,
  rowsPerPage,
  handleChangePage,
  handleChangeRowsPerPage,
  compact = false,
}: TablePaginationProps) => {
  // `component` is forwarded at runtime but dropped from the styled() wrapper's
  // prop types, so pass it through a spread to keep the div root element.
  const rootProps = { component: "div" };
  return (
    <Box sx={{ width: "fit-content", margin: "auto", py: compact ? 0.5 : 1 }}>
      <TablePaginationStyle
        {...rootProps}
        count={count}
        page={page}
        onPageChange={handleChangePage}
        rowsPerPage={rowsPerPage}
        onRowsPerPageChange={handleChangeRowsPerPage}
        sx={{
          "& .MuiTablePagination-toolbar": {
            minHeight: compact ? "40px" : "52px",
            paddingLeft: compact ? "8px" : "16px",
            paddingRight: compact ? "8px" : "16px",
          },
          "& .MuiTablePagination-selectLabel, & .MuiTablePagination-displayedRows": {
            fontSize: compact ? "0.75rem" : "0.875rem",
          },
          "& .MuiTablePagination-select": {
            fontSize: compact ? "0.75rem" : "0.875rem",
            paddingTop: compact ? "4px" : "8px",
            paddingBottom: compact ? "4px" : "8px",
          },
          "& .MuiTablePagination-input": {
            marginRight: compact ? "16px" : "24px",
            marginLeft: compact ? "4px" : "8px",
          },
          "& .MuiIconButton-root": {
            padding: compact ? "4px" : "8px",
          },
        }}
      />
    </Box>
  );
};

export default TablePagination;
