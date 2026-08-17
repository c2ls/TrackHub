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

import { isValidElement } from "react";
import type { ReactNode } from "react";
import { v4 as uuidv4 } from "uuid";
import { TableRow, TableBody as MuiTableBody } from "@mui/material";
import ArgonBox from "components/ArgonBox";
import ArgonAvatar from "components/ArgonAvatar";
import ArgonTypography from "components/ArgonTypography";
import borders from "assets/theme/base/borders";
import type { TableColumn, TableRowData } from "controls/Tables/Table";

const extractValue = (obj: unknown): string => {
  const el = obj as
    | { props?: { children?: unknown; name?: unknown; description?: unknown } }
    | null
    | undefined;
  return (el?.props?.children || el?.props?.name || el?.props?.description || "") as string;
};

export interface TableBodyProps {
  columns: TableColumn[];
  rows: TableRowData[];
  sortedRows: TableRowData[];
  selected?: string | null;
  selectedField: string;
  handleRowSelection: (rowKey: string | number) => void;
  page: number;
  rowsPerPage: number;
  compact?: boolean;
  rowRefs?: { current: Record<string, HTMLElement | null> };
}

const TableBody = ({
  columns,
  sortedRows,
  selected,
  selectedField,
  handleRowSelection,
  page,
  rowsPerPage,
  compact = false,
  rowRefs,
}: TableBodyProps) => {
  const { borderWidth } = borders;
  const tdExtra = { verticalalign: "middle" };

  return (
    <MuiTableBody>
      {sortedRows
        .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
        .map((row) => {
          const rowKey = row.id || uuidv4();
          const selectedValue = extractValue(row[selectedField]);

          const tableRow: ReactNode[] = columns
            .filter(({ name }) => name !== "id")
            .map(({ name, align }) => {
              let template: ReactNode;
              const cell = row[name];

              if (Array.isArray(cell)) {
                const avatar = cell as [string, string];
                const avatarProps = { src: avatar[0], name: avatar[1] };
                template = (
                  <ArgonBox
                    key={uuidv4()}
                    component="td"
                    p={compact ? 0.5 : 1}
                    sx={({ palette: { light } }) => ({
                      borderBottom: row.hasBorder ? `${borderWidth[1]} solid ${light.main}` : null,
                    })}
                  >
                    <ArgonBox
                      display="flex"
                      alignItems="center"
                      py={compact ? 0.25 : 0.5}
                      px={compact ? 0.5 : 1}
                    >
                      <ArgonBox mr={2}>
                        <ArgonAvatar {...avatarProps} variant="rounded" size="sm" />
                      </ArgonBox>
                      <ArgonTypography
                        variant="button"
                        fontWeight="medium"
                        sx={{ width: "max-content" }}
                      >
                        {avatar[1]}
                      </ArgonTypography>
                    </ArgonBox>
                  </ArgonBox>
                );
              } else if (isValidElement(cell)) {
                template = (
                  <ArgonBox
                    {...tdExtra}
                    key={uuidv4()}
                    component="td"
                    p={compact ? 0.5 : 1}
                    textAlign={align}
                    lineHeight={0.65}
                    sx={({ palette: { light } }) => ({
                      borderBottom: row.hasBorder ? `${borderWidth[1]} solid ${light.main}` : null,
                    })}
                  >
                    {cell}
                  </ArgonBox>
                );
              } else {
                template = (
                  <ArgonBox
                    {...tdExtra}
                    key={uuidv4()}
                    component="td"
                    p={compact ? 0.5 : 1}
                    textAlign={align}
                    lineHeight={0.65}
                    sx={({ palette: { light } }) => ({
                      borderBottom: row.hasBorder ? `${borderWidth[1]} solid ${light.main}` : null,
                    })}
                  >
                    <ArgonTypography
                      variant="button"
                      fontWeight="regular"
                      color="secondary"
                      sx={{ display: "inline-block", width: "max-content" }}
                    >
                      {extractValue(cell)}
                    </ArgonTypography>
                  </ArgonBox>
                );
              }

              return template;
            });

          return (
            <TableRow
              key={rowKey}
              onClick={() => handleRowSelection(rowKey)}
              selected={selected === selectedValue}
              ref={(el) => {
                if (rowRefs && rowRefs.current) {
                  rowRefs.current[selectedValue] = el;
                }
              }}
            >
              {tableRow}
            </TableRow>
          );
        })}
    </MuiTableBody>
  );
};

export default TableBody;
