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
import Grid from "@mui/material/Grid";
import Icon from "@mui/material/Icon";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

// Argon Dashboard 2 MUI contexts
import { useArgonController } from "context";

type MiniStatisticsPercentageColor =
  | "primary"
  | "secondary"
  | "info"
  | "success"
  | "warning"
  | "error"
  | "dark"
  | "white";

export interface MiniStatisticsCardProps {
  bgColor?: "white" | "primary" | "secondary" | "info" | "success" | "warning" | "error" | "dark";
  title?: {
    fontWeight?: "light" | "regular" | "medium" | "bold";
    text?: string;
  };
  count: string | number;
  percentage?: {
    color: MiniStatisticsPercentageColor;
    text: string | number;
  };
  icon: {
    color: "primary" | "secondary" | "info" | "success" | "warning" | "error" | "dark";
    component: ReactNode;
  };
  direction?: "right" | "left";
}

function MiniStatisticsCard({
  bgColor = "white",
  title = {
    fontWeight: "medium",
    text: "",
  },
  count,
  percentage = {
    color: "success",
    text: "",
  },
  icon,
  direction = "right",
}: MiniStatisticsCardProps) {
  const [controller] = useArgonController();
  const { darkMode } = controller;

  return (
    <Card>
      <ArgonBox bgColor={bgColor === "white" && darkMode ? "transparent" : bgColor}>
        <ArgonBox p={2}>
          <Grid container alignItems="center">
            {direction === "left" ? (
              <Grid>
                <ArgonBox
                  bgColor={bgColor === "white" ? icon.color : "white"}
                  color={bgColor === "white" ? "white" : "dark"}
                  width="3rem"
                  height="3rem"
                  borderRadius="section"
                  display="flex"
                  justifyContent="center"
                  alignItems="center"
                  shadow="md"
                >
                  <Icon fontSize="small" color="inherit">
                    {icon.component}
                  </Icon>
                </ArgonBox>
              </Grid>
            ) : null}
            <Grid size={{ xs: 8 }}>
              <ArgonBox ml={direction === "left" ? 2 : 0} lineHeight={1}>
                <ArgonTypography
                  variant="button"
                  color={bgColor === "white" ? "text" : "white"}
                  opacity={bgColor === "white" ? 1 : 0.7}
                  textTransform="uppercase"
                  fontWeight={title.fontWeight}
                >
                  {title.text}
                </ArgonTypography>
                <ArgonTypography
                  variant="h5"
                  fontWeight="bold"
                  color={bgColor === "white" ? "dark" : "white"}
                >
                  {count}{" "}
                  <ArgonTypography variant="button" color={percentage.color} fontWeight="bold">
                    {percentage.text}
                  </ArgonTypography>
                </ArgonTypography>
              </ArgonBox>
            </Grid>
            {direction === "right" ? (
              <Grid size={{ xs: 4 }}>
                <ArgonBox
                  bgColor={bgColor === "white" ? icon.color : "white"}
                  color={bgColor === "white" ? "white" : "dark"}
                  width="3rem"
                  height="3rem"
                  marginLeft="auto"
                  borderRadius="section"
                  display="flex"
                  justifyContent="center"
                  alignItems="center"
                  shadow="md"
                >
                  <Icon fontSize="small" color="inherit">
                    {icon.component}
                  </Icon>
                </ArgonBox>
              </Grid>
            ) : null}
          </Grid>
        </ArgonBox>
      </ArgonBox>
    </Card>
  );
}

export default MiniStatisticsCard;
