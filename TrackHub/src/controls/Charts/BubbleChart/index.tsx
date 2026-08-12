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
import type { BubbleDataPoint } from "chart.js";

// react-chartjs-2 components
import { Bubble } from "react-chartjs-2";

// @mui material components
import Card from "@mui/material/Card";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

// BubbleChart configurations
import configs from "controls/Charts/BubbleChart/configs";

// Argon Dashboard 2 MUI base styles
import colors from "assets/theme/base/colors";

export interface BubbleChartDataset {
  label?: string;
  color?: string;
  data: BubbleDataPoint[];
}

export interface BubbleChartData {
  labels?: string[];
  datasets?: BubbleChartDataset[];
}

export interface BubbleChartProps {
  title?: string;
  description?: ReactNode;
  height?: string | number;
  chart: BubbleChartData;
}

function BubbleChart({
  title = "",
  description = "",
  height = "100%",
  chart,
}: BubbleChartProps) {
  const palette = colors as unknown as Record<string, { main: string } | undefined>;

  const chartDatasets = chart.datasets
    ? chart.datasets.map((dataset) => ({
        ...dataset,
        tension: 0.4,
        borderWidth: 3,
        pointRadius: 2,
        backgroundColor: palette[dataset.color as string]
          ? palette[dataset.color || "dark"]!.main
          : colors.dark.main,
        borderColor: palette[dataset.color as string]
          ? palette[dataset.color || "dark"]!.main
          : colors.dark.main,
        maxBarThickness: 6,
      }))
    : [];

  const { data, options } = configs(chart.labels || [], chartDatasets);

  const renderChart = (
    <ArgonBox p={2}>
      {title || description ? (
        <ArgonBox px={description ? 1 : 0} pt={description ? 1 : 0}>
          {title && (
            <ArgonBox mb={1}>
              <ArgonTypography variant="h6">{title}</ArgonTypography>
            </ArgonBox>
          )}
          <ArgonBox mb={2}>
            <ArgonTypography component="div" variant="button" fontWeight="regular" color="text">
              {description}
            </ArgonTypography>
          </ArgonBox>
        </ArgonBox>
      ) : null}
      {useMemo(
        () => (
          <ArgonBox height={height}>
            <Bubble data={data} options={options} />
          </ArgonBox>
        ),
        [chart, height]
      )}
    </ArgonBox>
  );

  return title || description ? <Card>{renderChart}</Card> : renderChart;
}

export default BubbleChart;
