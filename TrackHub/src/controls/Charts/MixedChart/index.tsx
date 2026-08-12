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

import { useRef, useEffect, useState, useMemo } from "react";
import type { ReactNode } from "react";
import type { ChartData, ChartDataset, ChartOptions } from "chart.js";

// react-chartjs-2 components
import { Line } from "react-chartjs-2";

// @mui material components
import Card from "@mui/material/Card";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

// Argon Dashboard 2 MUI helper functions
import gradientChartLine from "assets/theme/functions/gradientChartLine";

// MixedChart configurations
import configs from "controls/Charts/MixedChart/configs";

// Argon Dashboard 2 MUI base styles
import colors from "assets/theme/base/colors";

export interface MixedChartDataset {
  label?: string;
  color?: string;
  chartType?: "default-line" | "gradient-line" | "thin-bar" | "bar";
  data: number[];
}

export interface MixedChartData {
  labels?: string[];
  datasets?: MixedChartDataset[];
}

export interface MixedChartProps {
  title?: string;
  description?: ReactNode;
  height?: string | number;
  chart: MixedChartData;
}

type LineChartConfig = { data: ChartData<"line">; options: ChartOptions<"line"> };

function MixedChart({
  title = "",
  description = "",
  height = "19.125rem",
  chart,
}: MixedChartProps) {
  const chartRef = useRef<HTMLDivElement>(null);
  const [chartData, setChartData] = useState<Partial<LineChartConfig>>({});
  const { data, options } = chartData;
  const palette = colors as unknown as Record<string, { main: string } | undefined>;

  useEffect(() => {
    const chartDatasets = chart.datasets
      ? chart.datasets.map((dataset) => {
          const defaultLine = {
            ...dataset,
            type: "line",
            tension: 0.4,
            borderWidth: 3,
            pointRadius: 2,
            pointBackgroundColor: palette[dataset.color as string]
              ? palette[dataset.color || "dark"]!.main
              : colors.dark.main,
            borderColor: palette[dataset.color as string]
              ? palette[dataset.color || "dark"]!.main
              : colors.dark.main,
            maxBarThickness: 6,
          };

          const gradientLine = {
            ...dataset,
            type: "line",
            tension: 0.4,
            pointRadius: 0,
            borderWidth: 3,
            borderColor: palette[dataset.color as string]
              ? palette[dataset.color || "dark"]!.main
              : colors.dark.main,
            fill: true,
            maxBarThickness: 6,
            backgroundColor: gradientChartLine(
              chartRef.current!.children[0] as HTMLCanvasElement,
              palette[dataset.color as string]
                ? palette[dataset.color || "dark"]!.main
                : colors.dark.main
            ),
          };

          const bar = {
            ...dataset,
            type: "bar",
            weight: 5,
            borderWidth: 0,
            borderRadius: 4,
            backgroundColor: palette[dataset.color as string]
              ? palette[dataset.color || "dark"]!.main
              : colors.dark.main,
            fill: false,
            maxBarThickness: 35,
          };

          const thinBar = {
            ...dataset,
            type: "bar",
            weight: 5,
            borderWidth: 0,
            borderRadius: 4,
            backgroundColor: palette[dataset.color as string]
              ? palette[dataset.color || "dark"]!.main
              : colors.dark.main,
            fill: false,
            maxBarThickness: 10,
          };

          let finalConfigs:
            | typeof defaultLine
            | typeof gradientLine
            | typeof bar
            | typeof thinBar;

          if (dataset.chartType === "default-line") {
            finalConfigs = defaultLine;
          } else if (dataset.chartType === "gradient-line") {
            finalConfigs = gradientLine;
          } else if (dataset.chartType === "thin-bar") {
            finalConfigs = thinBar;
          } else {
            finalConfigs = bar;
          }

          return { ...finalConfigs };
        })
      : [];

    setChartData(configs(chart.labels || [], chartDatasets as ChartDataset<"line">[]));
  }, [chart]);

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
          <ArgonBox ref={chartRef} sx={{ height }}>
            <Line data={data as ChartData<"line">} options={options} />
          </ArgonBox>
        ),
        [chartData, height]
      )}
    </ArgonBox>
  );

  return title || description ? <Card>{renderChart}</Card> : renderChart;
}

export default MixedChart;
