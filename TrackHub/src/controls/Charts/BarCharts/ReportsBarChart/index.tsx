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

/**
=========================================================
* Argon Dashboard 2 MUI - v3.0.1
=========================================================

* Product Page: https://www.creative-tim.com/product/argon-dashboard-material-ui
* Copyright 2023 Creative Tim (https://www.creative-tim.com)

Coded by www.creative-tim.com

 =========================================================

* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
*/

import { useMemo } from "react";
import type { ReactNode } from "react";

// react-chartjs-2 components
import { Bar } from "react-chartjs-2";

// @mui material components
import Card from "@mui/material/Card";
import Grid from "@mui/material/Grid";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

// Argon Dashboard 2 MUI example components
import BarReportsChartItem from "controls/Charts/BarCharts/ReportsBarChart/ReportsBarChartItem";
import type { ReportsBarChartItemColor } from "controls/Charts/BarCharts/ReportsBarChart/ReportsBarChartItem";

// ReportsBarChart configurations
import configs from "controls/Charts/BarCharts/ReportsBarChart/configs";
import type { ReportsBarChartDatasets } from "controls/Charts/BarCharts/ReportsBarChart/configs";

export interface ReportsBarChartItemData {
  icon: {
    color: ReportsBarChartItemColor;
    component: ReactNode;
  };
  label: string;
  progress: {
    content: string;
    percentage: number;
  };
}

export interface ReportsBarChartData {
  labels?: string[];
  datasets?: ReportsBarChartDatasets;
}

export interface ReportsBarChartProps {
  color?: ReportsBarChartItemColor;
  title: string;
  description?: ReactNode;
  chart: ReportsBarChartData;
  items?: ReportsBarChartItemData[];
}

function ReportsBarChart({
  color = "dark",
  title,
  description = "",
  chart,
  items = [],
}: ReportsBarChartProps) {
  const { data, options } = configs(chart.labels || [], chart.datasets || {});

  const renderItems = items.map(({ icon, label, progress }) => (
    <Grid size={{ xs: 6, sm: 3 }} key={label}>
      <BarReportsChartItem
        color={color}
        icon={{ color: icon.color, component: icon.component }}
        label={label}
        progress={{ content: progress.content, percentage: progress.percentage }}
      />
    </Grid>
  ));

  return (
    <Card sx={{ height: "100%" }}>
      <ArgonBox padding="1rem">
        {useMemo(
          () => (
            <ArgonBox
              variant="gradient"
              bgColor={color}
              borderRadius="lg"
              py={2}
              pr={0.5}
              mb={3}
              height="12.5rem"
            >
              <Bar data={data} options={options} />
            </ArgonBox>
          ),
          [chart, color]
        )}
        <ArgonBox px={1}>
          <ArgonBox mb={2}>
            <ArgonTypography variant="h6" fontWeight="medium" textTransform="capitalize">
              {title}
            </ArgonTypography>
            <ArgonTypography component="div" variant="button" color="text" fontWeight="regular">
              {description}
            </ArgonTypography>
          </ArgonBox>
          <ArgonBox py={1} px={0.5}>
            <Grid container spacing={2}>
              {renderItems}
            </Grid>
          </ArgonBox>
        </ArgonBox>
      </ArgonBox>
    </Card>
  );
}

export default ReportsBarChart;
