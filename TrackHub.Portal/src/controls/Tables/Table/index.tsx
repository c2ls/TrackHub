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

import { useState, useMemo, useRef, useEffect } from "react";
import type { ChangeEvent, MouseEvent } from "react";
import { Table as MuiTable, TableContainer } from "@mui/material";
import TableHeader from "./TableHeader";
import TableBody from "./TableBody";
import TablePagination from "./TablePagination";
import { useTranslation } from "react-i18next";
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

/** A single column descriptor for {@link Table}. */
export interface TableColumn {
  name: string;
  title?: string;
  align?: "left" | "right" | "center";
  width?: string | number;
  /** Keep the title for assistive technology only, for narrow icon columns. */
  hideTitle?: boolean;
  /**
   * The value this column sorts on. Cells hold rendered elements, so without it a column sorts by
   * the text inside them: speeds order 100 < 12 < 9 and dates by their formatted MM/DD/YYYY string.
   */
  sortValue?: (row: TableRowData) => number | string | Date | null | undefined;
}

/** A table row: known control fields plus arbitrary keyed cell values. */
export interface TableRowData {
  id?: string | number;
  hasBorder?: boolean;
  [key: string]: unknown;
}

const extractValue = (obj: unknown): string => {
  // A plain string or number cell (the row id) is its own value.
  if (typeof obj === "string" || typeof obj === "number") return String(obj);
  const el = obj as
    | { props?: { children?: unknown; name?: unknown; description?: unknown } }
    | null
    | undefined;
  return (el?.props?.children || el?.props?.name || el?.props?.description || "") as string;
};

/**
 * Orders two cell values: numbers and dates compare as themselves, text compares with the user's
 * collation, and a value that reads as a number is compared as one so "9" does not follow "100".
 */
const compareCellValues = (a: unknown, b: unknown): number => {
  if (a == null || a === "") return b == null || b === "" ? 0 : 1;
  if (b == null || b === "") return -1;

  if (a instanceof Date || b instanceof Date) {
    return new Date(a as Date).getTime() - new Date(b as Date).getTime();
  }

  const aNumber = typeof a === "number" ? a : Number(a);
  const bNumber = typeof b === "number" ? b : Number(b);
  if (!Number.isNaN(aNumber) && !Number.isNaN(bNumber)) {
    return aNumber - bNumber;
  }

  return String(a).localeCompare(String(b), undefined, { numeric: true, sensitivity: "base" });
};

export interface TableProps {
  columns?: TableColumn[];
  rows?: TableRowData[];
  selected?: string | null;
  selectedField?: string;
  handleSelected?: (value: string | null) => void;
  searchQuery?: string;
  compact?: boolean;
  scrollable?: boolean;
  /** Size columns to their content and scroll horizontally when they overflow the container
   *  (for wide datasets, e.g. report previews). Default keeps the fixed 100%-width layout. */
  horizontalScroll?: boolean;
  maxHeight?: string;
  /** Rows rendered in scrollable mode before the list is truncated. */
  maxScrollableRows?: number;
  defaultRowsPerPage?: number;
  /**
   * The rows are one SERVER page. Turns off the client-side pager, the
   * `searchQuery` filter and the column-sort affordance: all three would
   * silently operate within the page only, which reads as if they applied to
   * the whole result set. Pair with `controls/Tables/ServerPagination` and push
   * search/sort to the server query.
   */
  serverPaged?: boolean;
}

function Table({
  columns = [],
  rows = [{}],
  selected = null,
  selectedField = "name",
  handleSelected = () => {},
  searchQuery = "",
  compact = false,
  scrollable = false,
  horizontalScroll = false,
  maxHeight = "600px",
  maxScrollableRows = 100,
  defaultRowsPerPage = 10,
  serverPaged = false,
}: TableProps) {
  const { t } = useTranslation();
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(defaultRowsPerPage);
  const [order, setOrder] = useState<"asc" | "desc">("asc");
  const [orderBy, setOrderBy] = useState("");
  const rowRefs = useRef<Record<string, HTMLElement | null>>({});

  const handleChangePage = (event: MouseEvent<HTMLButtonElement> | null, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleRowSelection = (rowKey: string | number) => {
    const selectedRow = rows.find((row) => row.id === rowKey);
    const selectedValue = extractValue(selectedRow![selectedField]);
    if (selected === selectedValue) {
      handleSelected(null);
    } else {
      handleSelected(selectedValue);
    }
  };

  const handleSort = (columnName: string) => {
    const isAsc = orderBy === columnName && order === "asc";
    setOrder(isAsc ? "desc" : "asc");
    setOrderBy(columnName);
  };

  const filteredRows = useMemo(() => {
    if (searchQuery && !serverPaged) {
      return rows.filter((row) =>
        columns.some((column) =>
          extractValue(row[column.name]).toString().toLowerCase().includes(searchQuery.toLowerCase())
        )
      );
    }
    return rows;
  }, [rows, columns, searchQuery, serverPaged]);

  const sortedRows = useMemo(() => {
    if (orderBy && !serverPaged) {
      const column = columns.find((candidate) => candidate.name === orderBy);
      const valueOf = (row: TableRowData) =>
        column?.sortValue ? column.sortValue(row) : extractValue(row[orderBy]);

      return [...filteredRows].sort((a, b) => {
        const result = compareCellValues(valueOf(a), valueOf(b));
        return order === "asc" ? result : -result;
      });
    }
    return filteredRows;
  }, [filteredRows, columns, order, orderBy, serverPaged]);

  // How many rows the scroller renders. The cap keeps a thousand-unit fleet from mounting five
  // thousand cells on every refresh, but the SELECTED row always stays inside the window: picking a
  // unit on the map has to highlight and scroll to its row even when that row sits past the cap, and
  // a server-paged table never has more rows in hand than its page anyway.
  const selectedRowIndex = selected == null
    ? -1
    : sortedRows.findIndex((row) => extractValue(row[selectedField]) === selected);
  const scrollableRowCount = Math.max(
    Math.min(sortedRows.length, maxScrollableRows),
    selectedRowIndex + 1
  );

  // Auto-scroll to selected row in scrollable mode
  useEffect(() => {
    if (scrollable && selected) {
      const selectedRowRef = rowRefs.current[selected];
      if (selectedRowRef) {
        selectedRowRef.scrollIntoView({ behavior: "smooth", block: "nearest" });
      }
    }
  }, [selected, scrollable]);

  return (
    <TableContainer
      sx={{
        ...(scrollable ? { maxHeight: maxHeight, overflow: "auto", overflowX: "hidden" } : {}),
        ...(horizontalScroll ? { overflowX: "auto" } : {}),
      }}
    >
      <MuiTable
        sx={
          horizontalScroll
            ? {
                tableLayout: "auto",
                width: "max-content",
                minWidth: "100%",
                "& th": { whiteSpace: "nowrap" },
              }
            : { tableLayout: "fixed", width: "100%" }
        }
      >
        <TableHeader
          columns={columns}
          orderBy={orderBy}
          order={order}
          handleSort={handleSort}
          sortable={!serverPaged}
          compact={compact}
        />
        <TableBody
          columns={columns}
          rows={rows}
          sortedRows={sortedRows}
          selected={selected}
          selectedField={selectedField}
          handleRowSelection={handleRowSelection}
          page={scrollable || serverPaged ? 0 : page}
          // A scrollable table has no pager, so it used to mount every row: a thousand-unit fleet
          // rebuilt five thousand cells on each map refresh. The cap keeps the scroller honest.
          rowsPerPage={
            scrollable
              ? scrollableRowCount
              : serverPaged
                ? sortedRows.length
                : rowsPerPage
          }
          compact={compact}
          rowRefs={rowRefs}
        />
      </MuiTable>
      {scrollable && sortedRows.length > scrollableRowCount && (
        <ArgonBox p={1} textAlign="center">
          <ArgonTypography variant="caption" color="secondary">
            {t("table.moreRows", { count: sortedRows.length - scrollableRowCount })}
          </ArgonTypography>
        </ArgonBox>
      )}
      {!scrollable && !serverPaged && filteredRows.length > 10 && (
        <TablePagination
          count={filteredRows.length}
          page={page}
          rowsPerPage={rowsPerPage}
          handleChangePage={handleChangePage}
          handleChangeRowsPerPage={handleChangeRowsPerPage}
          compact={compact}
        />
      )}
    </TableContainer>
  );
}

export default Table;
