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

// react-countup components
import CountUp from "react-countup";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

// Argon Dashboard 2 MUI base styles
import colors from "assets/theme/base/colors";
import borders from "assets/theme/base/borders";

export type OutlinedCounterCardColor =
  | "primary"
  | "secondary"
  | "info"
  | "success"
  | "warning"
  | "error"
  | "dark";

export interface OutlinedCounterCardProps {
  color?: OutlinedCounterCardColor;
  count: string | number;
  title: string;
  prefix?: ReactNode;
  suffix?: ReactNode;
}

function OutlinedCounterCard({
  color = "info",
  count,
  title,
  prefix = "",
  suffix = "",
}: OutlinedCounterCardProps) {
  const { secondary } = colors;
  const { borderWidth } = borders;

  return (
    <ArgonBox
      borderRadius="md"
      border={`${borderWidth[1]} dashed ${secondary.main}`}
      textAlign="center"
      py={2}
    >
      <ArgonTypography variant="h6" color={color} fontWeight="medium" textTransform="capitalize">
        {title}
      </ArgonTypography>
      <ArgonTypography variant="h4" fontWeight="bold">
        {prefix && (
          <ArgonTypography component="span" variant="h5" fontWeight="bold">
            {prefix}
          </ArgonTypography>
        )}
        <ArgonBox display="inline-block" color="inherit" mx={0.5}>
          <CountUp end={Number(count)} duration={1} separator="," />
        </ArgonBox>
        {suffix && (
          <ArgonTypography component="span" variant="h5" fontWeight="bold">
            {suffix}
          </ArgonTypography>
        )}
      </ArgonTypography>
    </ArgonBox>
  );
}

export default OutlinedCounterCard;
