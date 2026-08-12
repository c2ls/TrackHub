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
import type { TypographyProps } from "@mui/material/Typography";

// @mui material components
import Card from "@mui/material/Card";
import Icon from "@mui/material/Icon";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

export interface PlaceholderCardProps {
  icon?: ReactNode;
  title: {
    variant: TypographyProps["variant"];
    text: string;
  };
  hasBorder?: boolean;
  outlined?: boolean;
}

function PlaceholderCard({
  icon = "add",
  title,
  hasBorder = false,
  outlined = false,
}: PlaceholderCardProps) {
  return (
    <Card
      raised
      sx={({ borders: { borderWidth, borderColor } }) => ({
        height: "100%",
        backgroundColor: outlined ? "transparent" : undefined,
        boxShadow: outlined ? "none" : undefined,
        border: hasBorder || outlined ? `${borderWidth[1]} solid ${borderColor}` : "none",
      })}
    >
      <ArgonBox
        display="flex"
        flexDirection="column"
        justifyContent="center"
        textAlign="center"
        height="100%"
        p={3}
      >
        <ArgonBox color="secondary" mb={0.5}>
          <Icon fontSize="medium" sx={{ fontWeight: "bold" }}>
            {icon}
          </Icon>
        </ArgonBox>
        <ArgonTypography variant={title.variant} color="secondary">
          {title.text}
        </ArgonTypography>
      </ArgonBox>
    </Card>
  );
}

export default PlaceholderCard;
