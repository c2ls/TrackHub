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

import type { ReactNode } from "react";

// @mui material components
import Icon from "@mui/material/Icon";

// Argon Dashboard 2 MUI base styles
import typography from "assets/theme/base/typography";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import ArgonProgress from "components/ArgonProgress";

export type ReportsBarChartItemColor =
  | "primary"
  | "secondary"
  | "info"
  | "success"
  | "warning"
  | "error"
  | "dark";

export interface ReportsBarChartItemProps {
  color?: ReportsBarChartItemColor;
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

function ReportsBarChartItem({
  color = "dark",
  icon,
  label,
  progress,
}: ReportsBarChartItemProps) {
  const { size } = typography;

  return (
    <ArgonBox width="100%">
      <ArgonBox display="flex" alignItems="center" mb={2}>
        <ArgonBox
          bgColor={icon.color}
          width="1.25rem"
          height="1.25rem"
          borderRadius="sm"
          color="white"
          fontSize={size.xs}
          display="flex"
          justifyContent="center"
          alignItems="center"
          shadow="md"
          mr={1}
          variant="gradient"
        >
          <Icon>{icon.component}</Icon>
        </ArgonBox>
        <ArgonTypography
          variant="caption"
          textTransform="capitalize"
          fontWeight="medium"
          color="text"
        >
          {label}
        </ArgonTypography>
      </ArgonBox>
      <ArgonBox mt={1}>
        <ArgonTypography variant="h4" fontWeight="bold" color={color}>
          {progress.content}
        </ArgonTypography>
        <ArgonBox width="75%" mt={0.5}>
          <ArgonProgress value={progress.percentage} color={color} />
        </ArgonBox>
      </ArgonBox>
    </ArgonBox>
  );
}

export default ReportsBarChartItem;
