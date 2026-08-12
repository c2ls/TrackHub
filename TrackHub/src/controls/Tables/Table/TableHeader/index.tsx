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

import ArgonBox from "components/ArgonBox";
import typography from "assets/theme/base/typography";
import borders from "assets/theme/base/borders";
import { TableRow } from "@mui/material";
import type { TableColumn } from "controls/Tables/Table";

export interface TableHeaderProps {
  columns: TableColumn[];
  orderBy: string;
  order: string;
  handleSort: (name: string) => void;
  compact?: boolean;
}

const TableHeader = ({ columns, handleSort, compact = false }: TableHeaderProps) => {
  const { size, fontWeightBold } = typography;
  const { borderWidth } = borders;

  return (
    <ArgonBox component="thead">
      <TableRow>
        {columns
          .filter(({ name }) => name !== "id") // Hide the id column
          .map(({ name, title, align, width }, key) => {
            let pl;
            let pr;

            if (key === 0) {
              pl = 3;
              pr = 3;
            } else if (key === columns.length - 1) {
              pl = 3;
              pr = 3;
            } else {
              pl = 1;
              pr = 1;
            }

            return (
              <ArgonBox
                key={name}
                component="th"
                width={width || "auto"}
                pt={compact ? 0.75 : 1.5}
                pb={compact ? 0.5 : 1.25}
                pl={compact ? (align === "left" ? 1.5 : 1.5) : align === "left" ? pl : 3}
                pr={compact ? (align === "right" ? 1.5 : 1.5) : align === "right" ? pr : 3}
                textAlign={align}
                fontSize={size.xxs}
                fontWeight={fontWeightBold}
                color="secondary"
                opacity={0.7}
                sx={({ palette: { light, background } }) => ({
                  borderBottom: `${borderWidth[1]} solid ${light.main}`,
                  position: "sticky",
                  top: 0,
                  backgroundColor:
                    (background as typeof background & { card?: string }).card ||
                    background.default ||
                    "#fff",
                  zIndex: 10,
                })}
                onClick={() => handleSort(name)}
                style={{ cursor: "pointer" }}
              >
                {(title || name).toUpperCase()}
              </ArgonBox>
            );
          })}
      </TableRow>
    </ArgonBox>
  );
};

export default TableHeader;
