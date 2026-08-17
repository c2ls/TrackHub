/**
* Copyright (c) 2026 Sergio Hernandez. All rights reserved.
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

import { useMemo } from "react";
import Card from "@mui/material/Card";
import { useTranslation } from 'react-i18next';
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import Table from "controls/Tables/Table";
import type { ReportPreview } from "api/reporting/reports";
import { mapPreviewToTable, formatPreviewCell } from "utils/reportUtils";

interface ReportPreviewPanelProps {
  preview: ReportPreview;
}

/** Renders a report preview dataset as a client-paginated table. */
function ReportPreviewPanel({ preview }: ReportPreviewPanelProps) {
  const { t } = useTranslation();

  const { columns, rows } = useMemo(() => {
    const mapped = mapPreviewToTable(preview);
    return {
      columns: mapped.columns,
      rows: mapped.rows.map((row) => {
        const display: Record<string, unknown> = { id: row.id };
        for (const column of mapped.columns) {
          display[column.name] = (
            <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
              {formatPreviewCell(row[column.name] as never)}
            </ArgonTypography>
          );
        }
        return display;
      }),
    };
  }, [preview]);

  return (
    <Card>
      <ArgonBox p={2}>
        <ArgonBox display="flex" justifyContent="space-between" alignItems="center" mb={1} flexWrap="wrap" gap={1}>
          <ArgonTypography variant="button" fontWeight="medium">
            {t('reports.totalRows', { total: preview.totalRows })}
          </ArgonTypography>
          {preview.truncated && (
            <ArgonTypography variant="caption" color="warning" fontWeight="medium">
              {t('reports.previewTruncated', { count: preview.rows.length })}
            </ArgonTypography>
          )}
        </ArgonBox>
        {preview.rows.length === 0 ? (
          <ArgonTypography variant="caption" color="secondary">
            {t('reports.noPreviewRows')}
          </ArgonTypography>
        ) : (
          <Table columns={columns} rows={rows} horizontalScroll />
        )}
      </ArgonBox>
    </Card>
  );
}

export default ReportPreviewPanel;
