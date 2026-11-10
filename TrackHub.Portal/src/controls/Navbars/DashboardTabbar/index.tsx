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

import { useState } from "react";
import type { ChangeEventHandler, ReactNode, SyntheticEvent } from "react";

import { Tabs, Tab } from "@mui/material";
import DashboardLayout from "controls/LayoutContainers/DashboardLayout";
import DashboardNavbar from "controls/Navbars/DashboardNavbar";

export interface DashboardTabbarProps {
  searchVisibility?: boolean;
  searchQuery?: string;
  handleSearch?: ChangeEventHandler<HTMLInputElement | HTMLTextAreaElement>;
  tabs: string[];
  onTabChange?: (value: number) => void;
  children: ReactNode;
}

function DashboardTabbar({
  searchVisibility = false,
  searchQuery = "",
  handleSearch = () => {},
  tabs = [],
  onTabChange = () => {},
  children,
}: DashboardTabbarProps) {
  const [tabValue, setTabValue] = useState(0);

  const handleSetTabValue = (event: SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
    onTabChange(newValue);
  };

  return (
    <DashboardLayout>
      <DashboardNavbar
        searchQuery={searchQuery}
        handleSearch={handleSearch}
        searchVisibility={searchVisibility}
        tabs={
          <Tabs value={tabValue} onChange={handleSetTabValue}>
            {tabs.map((tab, index) => (
              <Tab key={index} label={tab} />
            ))}
          </Tabs>
        }
      />
      {children}
    </DashboardLayout>
  );
}

export default DashboardTabbar;
