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
import Card from "@mui/material/Card";
import Divider from "@mui/material/Divider";
import Icon from "@mui/material/Icon";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

export type DefaultInfoCardColor =
  | "primary"
  | "secondary"
  | "info"
  | "success"
  | "warning"
  | "error"
  | "dark";

export interface DefaultInfoCardProps {
  color?: DefaultInfoCardColor;
  icon: ReactNode;
  title: string;
  description?: string;
  value?: string | number;
}

function DefaultInfoCard({
  color = "info",
  icon,
  title,
  description = "",
  value = "",
}: DefaultInfoCardProps) {
  return (
    <Card>
      <ArgonBox p={2} mx={3} display="flex" justifyContent="center">
        <ArgonBox
          display="grid"
          justifyContent="center"
          alignItems="center"
          bgColor={color}
          color="white"
          width="4rem"
          height="4rem"
          shadow="md"
          borderRadius="lg"
          variant="gradient"
        >
          <Icon fontSize="medium">{icon}</Icon>
        </ArgonBox>
      </ArgonBox>
      <ArgonBox pb={2} px={2} textAlign="center" lineHeight={1.25}>
        <ArgonTypography variant="h6" fontWeight="medium" textTransform="capitalize">
          {title}
        </ArgonTypography>
        {description && (
          <ArgonTypography variant="caption" color="text" fontWeight="regular">
            {description}
          </ArgonTypography>
        )}
        {description && !value ? null : <Divider />}
        {value && (
          <ArgonTypography variant="h5" fontWeight="medium">
            {value}
          </ArgonTypography>
        )}
      </ArgonBox>
    </Card>
  );
}

export default DefaultInfoCard;
