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

/** A single column descriptor for {@link Table}. */
export interface TableColumn {
  name: string;
  title?: string;
  align?: "left" | "right" | "center";
  width?: string | number;
}

/** A table row: known control fields plus arbitrary keyed cell values. */
export interface TableRowData {
  id?: string | number;
  hasBorder?: boolean;
  [key: string]: unknown;
}

const extractValue = (obj: unknown): string => {
  const el = obj as
    | { props?: { children?: unknown; name?: unknown; description?: unknown } }
    | null
    | undefined;
  return (el?.props?.children || el?.props?.name || el?.props?.description || "") as string;
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
  defaultRowsPerPage?: number;
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
  defaultRowsPerPage = 10,
}: TableProps) {
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
    if (searchQuery) {
      return rows.filter((row) =>
        columns.some((column) =>
          extractValue(row[column.name]).toString().toLowerCase().includes(searchQuery.toLowerCase())
        )
      );
    }
    return rows;
  }, [rows, columns, searchQuery]);

  const sortedRows = useMemo(() => {
    if (orderBy) {
      return [...filteredRows].sort((a, b) => {
        const aValue = extractValue(a[orderBy]);
        const bValue = extractValue(b[orderBy]);
        if (aValue < bValue) {
          return order === "asc" ? -1 : 1;
        }
        if (aValue > bValue) {
          return order === "asc" ? 1 : -1;
        }
        return 0;
      });
    }
    return filteredRows;
  }, [filteredRows, order, orderBy]);

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
          compact={compact}
        />
        <TableBody
          columns={columns}
          rows={rows}
          sortedRows={scrollable ? sortedRows : sortedRows}
          selected={selected}
          selectedField={selectedField}
          handleRowSelection={handleRowSelection}
          page={scrollable ? 0 : page}
          rowsPerPage={scrollable ? sortedRows.length : rowsPerPage}
          compact={compact}
          rowRefs={rowRefs}
        />
      </MuiTable>
      {!scrollable && filteredRows.length > 10 && (
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
