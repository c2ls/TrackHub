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

import { useState, useEffect } from "react";
import type { ChangeEventHandler, ReactNode, SyntheticEvent } from "react";

import { AppBar, Tabs, Tab, Grid } from "@mui/material";
import ArgonBox from "components/ArgonBox";
import breakpoints from "assets/theme/base/breakpoints";
import DashboardLayout from "controls/LayoutContainers/DashboardLayout";
import DashboardNavbar from "controls/Navbars/DashboardNavbar";
import Footer from "controls/Footer";

export interface DashboardTabbarProps {
  stickyNavbar?: boolean;
  searchVisibility?: boolean;
  searchQuery?: string;
  handleSearch?: ChangeEventHandler<HTMLInputElement | HTMLTextAreaElement>;
  tabs: string[];
  onTabChange?: (value: number) => void;
  children: ReactNode;
}

function DashboardTabbar({
  stickyNavbar = false,
  searchVisibility = false,
  searchQuery = "",
  handleSearch = () => {},
  tabs = [],
  onTabChange = () => {},
  children,
}: DashboardTabbarProps) {
  const [tabsOrientation, setTabsOrientation] = useState<"horizontal" | "vertical">("horizontal");
  const [tabValue, setTabValue] = useState(0);

  useEffect(() => {
    // A function that sets the orientation state of the tabs.
    function handleTabsOrientation() {
      return window.innerWidth < breakpoints.values.sm
        ? setTabsOrientation("vertical")
        : setTabsOrientation("horizontal");
    }

    /**
     The event listener that's calling the handleTabsOrientation function when resizing the window.
    */
    window.addEventListener("resize", handleTabsOrientation);

    // Call the handleTabsOrientation function to set the state with the initial value.
    handleTabsOrientation();

    // Remove event listener on cleanup
    return () => window.removeEventListener("resize", handleTabsOrientation);
  }, [tabsOrientation]);

  const handleSetTabValue = (event: SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
    onTabChange(newValue);
  };

  return (
    <DashboardLayout>
      <DashboardNavbar
        absolute={!stickyNavbar}
        searchQuery={searchQuery}
        handleSearch={handleSearch}
        searchVisibility={searchVisibility}
      />
      <ArgonBox mt={stickyNavbar ? 1 : 10}>
        <Grid container>
          <Grid size={{ xs: 12, sm: 8, lg: 4 }}>
            <AppBar position="static">
              <Tabs orientation={tabsOrientation} value={tabValue} onChange={handleSetTabValue}>
                {tabs.map((tab, index) => (
                  <Tab key={index} label={tab} />
                ))}
              </Tabs>
            </AppBar>
          </Grid>
        </Grid>
        {children}
      </ArgonBox>
      <Footer />
    </DashboardLayout>
  );
}

export default DashboardTabbar;
