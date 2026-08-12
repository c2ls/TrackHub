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

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

// Argon Dashboard 2 MUI contexts
import { useArgonController } from "context";

type DetailedStatisticsPercentageColor =
  | "primary"
  | "secondary"
  | "info"
  | "success"
  | "warning"
  | "error"
  | "light"
  | "dark"
  | "text"
  | "white"
  | "inherit";

export interface DetailedStatisticsCardProps {
  bgColor?:
    | "transparent"
    | "white"
    | "primary"
    | "secondary"
    | "info"
    | "success"
    | "warning"
    | "error"
    | "dark";
  title: string;
  count: string | number;
  percentage?: {
    color: DetailedStatisticsPercentageColor;
    count: string | number;
    hide: boolean;
  };
  icon?: {
    color: string;
    onClick?: (() => void) | null;
    component: ReactNode;
  };
  direction?: "right" | "left";
}

function DetailedStaticsCard({
  bgColor = "white",
  title,
  count,
  percentage = {
    color: "success",
    count: "",
    hide: false,
  },
  icon = {
    color: "primary",
    onClick: null,
    component: <i className="default-icon" />,
  },
  direction = "right",
}: DetailedStatisticsCardProps) {
  const [controller] = useArgonController();
  const { darkMode } = controller;

  // `disabled` and `placeitems` are non-standard DOM attributes forwarded verbatim by the
  // vendored template; they carry no MUI/Box typing, so pass them through as loose props.
  const iconInnerProps: Record<string, unknown> = {
    onClick: icon.onClick ?? undefined,
    disabled: !icon.onClick,
    placeitems: "center",
  };

  return (
    <Card>
      <ArgonBox
        bgColor={bgColor === "white" && darkMode ? "transparent" : bgColor}
        variant={bgColor === "white" && darkMode ? "contained" : "gradient"}>
        <ArgonBox p={2}>
          <Grid container>
            {direction === "left" ? (
              <Grid>
                <ArgonBox
                  variant="gradient"
                  bgColor={bgColor === "white" ? icon.color : "white"}
                  color={bgColor === "white" ? "white" : "dark"}
                  width="3rem"
                  height="3rem"
                  borderRadius="section"
                  display="flex"
                  justifyContent="center"
                  alignItems="center">
                  <ArgonBox
                    {...iconInnerProps}
                    fontSize="1.125rem"
                    display="grid"
                    color="inherit">
                    {icon.component}
                  </ArgonBox>
                </ArgonBox>
              </Grid>
            ) : null}
            <Grid size={{xs: 8}}>
              <ArgonBox ml={direction === "left" ? 2 : 0} lineHeight={1}>
                <ArgonTypography
                  variant="button"
                  color={bgColor === "white" ? "text" : "white"}
                  textTransform="uppercase"
                  fontWeight="medium">
                  {title}
                </ArgonTypography>
                <ArgonTypography
                  variant="h5"
                  fontWeight="bold"
                  color={bgColor === "white" ? "dark" : "white"}
                  mb={1}>
                  {`${count} `}
                  {!percentage.hide && (
                    <ArgonTypography
                      variant="button"
                      fontWeight="bold"
                      color={percentage.color}>
                      ({percentage.count})
                    </ArgonTypography>
                  )}
                </ArgonTypography>
              </ArgonBox>
            </Grid>
            {direction === "right" ? (
              <Grid size={{xs: 4}}>
                <ArgonBox
                  variant="gradient"
                  bgColor={bgColor === "white" ? icon.color : "white"}
                  color={bgColor === "white" ? "white" : "dark"}
                  width="3rem"
                  height="3rem"
                  borderRadius="section"
                  display="flex"
                  justifyContent="center"
                  alignItems="center"
                  ml="auto">
                  <ArgonBox
                    {...iconInnerProps}
                    fontSize="1.125rem"
                    display="grid"
                    color="inherit">
                    {icon.component}
                  </ArgonBox>
                </ArgonBox>
              </Grid>
            ) : null}
          </Grid>
        </ArgonBox>
      </ArgonBox>
    </Card>
  );
}

export default DetailedStaticsCard;
