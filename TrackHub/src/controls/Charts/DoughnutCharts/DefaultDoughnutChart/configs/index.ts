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

/* eslint-disable no-dupe-keys */
import type { ChartData, ChartDataset, ChartOptions } from "chart.js";

// Argon Dashboard 2 MUI base styles
import colors from "assets/theme/base/colors";

const { gradients, dark } = colors;

export interface DoughnutChartDatasets {
  label?: string;
  data?: number[];
  backgroundColors?: string[];
}

function configs(
  labels: unknown[],
  datasets: DoughnutChartDatasets,
  cutout = 60
): { data: ChartData<"doughnut">; options: ChartOptions<"doughnut"> } {
  const backgroundColors: string[] = [];
  const gradientPalette = gradients as Record<string, { main: string; state: string } | undefined>;

  if (datasets.backgroundColors) {
    datasets.backgroundColors.forEach((color) => {
      if (gradientPalette[color]) {
        if (color === "info") {
          backgroundColors.push(gradients.info.main);
        } else {
          backgroundColors.push(gradientPalette[color]!.state);
        }
      } else {
        backgroundColors.push(dark.main);
      }
    });
  } else {
    backgroundColors.push(dark.main);
  }

  return {
    data: {
      labels,
      datasets: [
        {
          label: datasets.label,
          weight: 9,
          cutout,
          tension: 0.9,
          pointRadius: 2,
          borderWidth: 2,
          backgroundColor: backgroundColors,
          fill: false,
          data: datasets.data,
        } as ChartDataset<"doughnut">,
      ],
    } as ChartData<"doughnut">,
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false,
        },
      },
      interaction: {
        intersect: false,
        mode: "index",
      },
      scales: {
        y: {
          grid: {
            drawBorder: false,
            display: false,
            drawOnChartArea: false,
            drawTicks: false,
          },
          ticks: {
            display: false,
          },
        },
        x: {
          grid: {
            drawBorder: false,
            display: false,
            drawOnChartArea: false,
            drawTicks: false,
          },
          ticks: {
            display: false,
          },
        },
      },
    } as ChartOptions<"doughnut">,
  };
}

export default configs;
