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

import { useMemo, useState } from "react";
import Card from "@mui/material/Card";
import { useTranslation } from 'react-i18next';
import ArgonBox from "components/ArgonBox";
import ArgonBadge from "components/ArgonBadge";
import ArgonTypography from "components/ArgonTypography";
import TableAccordion from "controls/Accordions/TableAccordion";
import type { Report } from "api/manager/reports";
import { toCamelCase } from 'utils/stringUtils';

interface ReportCatalogProps {
  reports: Report[];
  selectedReport?: string;
  onSelect: (report: Report) => void;
}

/** Preserves the server's category order (reports arrive ordered by category). */
function groupByCategory(reports: Report[]): [string, Report[]][] {
  const groups = new Map<string, Report[]>();
  for (const report of reports) {
    const bucket = groups.get(report.category);
    if (bucket) {
      bucket.push(report);
    } else {
      groups.set(report.category, [report]);
    }
  }
  return [...groups.entries()];
}

function ReportRow({
  report,
  selected,
  onSelect,
}: {
  report: Report;
  selected: boolean;
  onSelect: (report: Report) => void;
}) {
  const { t } = useTranslation();
  const nameKey = `reportList.${toCamelCase(report.code)}` as 'reportList.liveReport';
  const descriptionKey = `reportDescriptions.${toCamelCase(report.code)}` as 'reportDescriptions.liveReport';

  return (
    <ArgonBox
      onClick={() => onSelect(report)}
      px={2}
      py={1.5}
      mb={1}
      sx={{
        cursor: 'pointer',
        borderRadius: 'md',
        border: ({ borders }) => `${borders.borderWidth[1]} solid ${borders.borderColor}`,
        backgroundColor: ({ palette }) => (selected ? palette.action.selected : 'transparent'),
        '&:hover': {
          backgroundColor: ({ palette }) => (selected ? palette.action.selected : palette.action.hover),
        },
      }}
    >
      <ArgonBox display="flex" alignItems="center" justifyContent="space-between" flexWrap="wrap" gap={1}>
        <ArgonBox>
          <ArgonTypography variant="button" fontWeight="medium">
            {t(nameKey)}
          </ArgonTypography>
          <ArgonTypography variant="caption" color="secondary" display="block">
            {t(descriptionKey)}
          </ArgonTypography>
        </ArgonBox>
        <ArgonBox display="flex" gap={0.5}>
          <ArgonBadge badgeContent="XLSX" color="success" size="xs" container />
          {report.supportsPdf && (
            <ArgonBadge badgeContent="PDF" color="error" size="xs" container />
          )}
        </ArgonBox>
      </ArgonBox>
    </ArgonBox>
  );
}

function ReportCatalog({ reports, selectedReport, onSelect }: ReportCatalogProps) {
  const { t } = useTranslation();
  const grouped = useMemo(() => groupByCategory(reports), [reports]);
  const [expanded, setExpanded] = useState<string | null>(grouped.length > 0 ? grouped[0][0] : null);

  if (reports.length === 0) {
    return (
      <Card>
        <ArgonBox p={3}>
          <ArgonTypography variant="button" color="secondary">
            {t('reports.noReports')}
          </ArgonTypography>
        </ArgonBox>
      </Card>
    );
  }

  return (
    <ArgonBox>
      {grouped.map(([category, categoryReports]) => {
        const categoryKey = `reportCategories.${category.toLowerCase()}` as 'reportCategories.operations';
        return (
          <TableAccordion
            key={category}
            title={t(categoryKey)}
            expanded={expanded === category}
            setExpanded={(isOpen) => setExpanded(isOpen ? category : null)}
          >
            {categoryReports.map((report) => (
              <ReportRow
                key={report.code}
                report={report}
                selected={selectedReport === report.code}
                onSelect={onSelect}
              />
            ))}
          </TableAccordion>
        );
      })}
    </ArgonBox>
  );
}

export default ReportCatalog;
