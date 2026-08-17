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

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import ArgonBadge from "components/ArgonBadge";

// Timeline context
import { useTimeline } from "controls/Timeline/context";

// Custom styles for the TimelineItem
import { timelineItem, timelineItemIcon } from "controls/Timeline/TimelineItem/styles";

export interface TimelineItemProps {
  color?: "primary" | "secondary" | "info" | "success" | "warning" | "error" | "dark" | "light";
  icon: ReactNode;
  title: string;
  dateTime: string;
  description?: string;
  badges?: (string | number)[];
  lastItem?: boolean;
}

function TimelineItem({
  color = "info",
  icon,
  title,
  dateTime,
  description = "",
  badges = [],
  lastItem = false,
}: TimelineItemProps) {
  const isDark = useTimeline();

  const renderBadges =
    badges.length > 0
      ? badges.map((badge, key) => {
          const badgeKey = `badge-${key}`;

          return (
            <ArgonBox key={badgeKey} mr={key === badges.length - 1 ? 0 : 0.5}>
              <ArgonBadge color={color} size="xs" badgeContent={badge} container />
            </ArgonBox>
          );
        })
      : null;

  return (
    <ArgonBox position="relative" sx={(theme) => timelineItem(theme, { lastItem })}>
      <ArgonBox
        bgColor={isDark ? "dark" : "white"}
        width="1.625rem"
        height="1.625rem"
        borderRadius="50%"
        position="absolute"
        top="3.25%"
        left="2px"
        zIndex={2}
      >
        <Icon sx={(theme) => timelineItemIcon(theme, { color })}>{icon}</Icon>
      </ArgonBox>
      <ArgonBox ml={5.75} pt={description ? 0.7 : 0.5} lineHeight={0} maxWidth="30rem">
        <ArgonTypography variant="button" fontWeight="medium" color={isDark ? "white" : "dark"}>
          {title}
        </ArgonTypography>
        <ArgonBox mt={0.5}>
          <ArgonTypography
            variant="caption"
            fontWeight="medium"
            color={isDark ? "secondary" : "text"}
          >
            {dateTime}
          </ArgonTypography>
        </ArgonBox>
        <ArgonBox mt={2} mb={1.5}>
          {description ? (
            <ArgonTypography variant="button" fontWeight="regular" color="text">
              {description}
            </ArgonTypography>
          ) : null}
        </ArgonBox>
        {badges.length > 0 ? (
          <ArgonBox display="flex" pb={lastItem ? 1 : 2}>
            {renderBadges}
          </ArgonBox>
        ) : null}
      </ArgonBox>
    </ArgonBox>
  );
}

export default TimelineItem;
