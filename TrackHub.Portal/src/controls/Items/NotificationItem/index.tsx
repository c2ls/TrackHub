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

import { forwardRef } from "react";
import type { ReactNode } from "react";

// @mui material components
import MenuItem from "@mui/material/MenuItem";
import type { MenuItemProps } from "@mui/material/MenuItem";
import Icon from "@mui/material/Icon";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

// custom styles for the NotificationItem
import { menuItem, menuImage } from "controls/Items/NotificationItem/styles";

export interface NotificationItemProps extends Omit<MenuItemProps, "title"> {
  color?: "primary" | "secondary" | "info" | "success" | "warning" | "error" | "light" | "dark";
  image: ReactNode;
  title: string[];
  date: string;
}

const NotificationItem = forwardRef<HTMLLIElement, NotificationItemProps>(
  ({ color = "dark", image, title, date, ...rest }, ref) => (
    <MenuItem {...rest} ref={ref} sx={(theme) => menuItem(theme)}>
      <ArgonBox
        width="2.25rem"
        height="2.25rem"
        mt={0.25}
        mr={2}
        mb={0.25}
        borderRadius="lg"
        sx={(theme) => menuImage(theme, { color })}
      >
        {image}
      </ArgonBox>
      <ArgonBox>
        <ArgonTypography variant="button" textTransform="capitalize" fontWeight="regular">
          <strong>{title[0]}</strong> {title[1]}
        </ArgonTypography>
        <ArgonTypography
          variant="caption"
          color="secondary"
          sx={{
            display: "flex",
            alignItems: "center",
            mt: 0.5,
          }}
        >
          <ArgonTypography variant="button" color="secondary">
            <Icon
              sx={{
                lineHeight: 1.2,
                mr: 0.5,
              }}
            >
              watch_later
            </Icon>
          </ArgonTypography>
          {date}
        </ArgonTypography>
      </ArgonBox>
    </MenuItem>
  )
);

export default NotificationItem;
