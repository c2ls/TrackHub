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

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

// Timeline context
import { TimelineProvider } from "controls/Timeline/context";

export interface TimelineListProps {
  title: string;
  dark?: boolean;
  children: ReactNode;
}

function TimelineList({ title, dark = false, children }: TimelineListProps) {
  return (
    <TimelineProvider value={dark}>
      <Card>
        <ArgonBox bgColor={dark ? "dark" : "white"} variant="gradient">
          <ArgonBox pt={3} px={3}>
            <ArgonTypography variant="h6" fontWeight="medium" color={dark ? "white" : "dark"}>
              {title}
            </ArgonTypography>
          </ArgonBox>
          <ArgonBox p={2}>{children}</ArgonBox>
        </ArgonBox>
      </Card>
    </TimelineProvider>
  );
}

export default TimelineList;
