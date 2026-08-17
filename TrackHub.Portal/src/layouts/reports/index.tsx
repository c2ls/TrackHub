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

import { useState, useEffect, useContext } from 'react';
import Grid from "@mui/material/Grid";
import ArgonBox from "components/ArgonBox";
import DashboardLayout from "controls/LayoutContainers/DashboardLayout";
import DashboardNavbar from "controls/Navbars/DashboardNavbar";
import ReportFilters from "layouts/reports/components/Filters";
import type { ReportAction } from "layouts/reports/components/Filters";
import ReportCatalog from "layouts/reports/components/Catalog";
import ReportPreviewPanel from "layouts/reports/components/Preview";
import { useReportCatalog, useReportPreview, useDownloadReport } from "queries/reports";
import type { Report } from "api/manager/reports";
import type { ReportFilterValues, ReportPreview } from "api/reporting/reports";
import { useTranslation } from 'react-i18next';
import { LoadingContext } from 'LoadingContext';
import { useAuth } from "AuthContext";
import { toCamelCase } from 'utils/stringUtils';

function Reports() {
  const { t } = useTranslation();
  const { isAuthenticated } = useAuth();
  const { setLoading } = useContext(LoadingContext);

  const [selected, setSelected] = useState<Report | undefined>(undefined);
  const [preview, setPreview] = useState<ReportPreview | undefined>(undefined);

  const catalogQuery = useReportCatalog({ enabled: isAuthenticated });
  const previewMutation = useReportPreview();
  const downloadMutation = useDownloadReport();

  const reports = catalogQuery.data ?? [];
  const running = previewMutation.isPending || downloadMutation.isPending;

  useEffect(() => {
    setLoading(catalogQuery.isFetching || running);
  }, [catalogQuery.isFetching, running, setLoading]);

  const handleSelect = (report: Report) => {
    setSelected(report);
    setPreview(undefined);
  };

  const handleRun = async (values: ReportFilterValues, action: ReportAction) => {
    if (!selected) return;
    const reportName = t(`reportList.${toCamelCase(selected.code)}` as 'reportList.liveReport');
    const args = { reportCode: selected.code, reportName, filters: values };
    if (action === 'preview') {
      previewMutation.mutate(args, { onSuccess: setPreview });
    } else {
      downloadMutation.mutate({ ...args, format: action });
    }
  };

  return (
    <DashboardLayout>
      <DashboardNavbar />
      <ArgonBox py={3}>
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, lg: 5 }}>
            <ReportCatalog
              reports={reports}
              selectedReport={selected?.code}
              onSelect={handleSelect}
            />
          </Grid>
          <Grid size={{ xs: 12, lg: 7 }}>
            {selected && (
              <ArgonBox mb={3}>
                <ReportFilters
                  selectedReport={selected.code}
                  supportsPdf={selected.supportsPdf}
                  running={running}
                  onRun={handleRun}
                />
              </ArgonBox>
            )}
            {preview && <ReportPreviewPanel preview={preview} />}
          </Grid>
        </Grid>
      </ArgonBox>
    </DashboardLayout>
  );
};

export default Reports;
