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

// @mui material components
import Card from "@mui/material/Card";
import Grid from "@mui/material/Grid";
import type { SxProps, Theme } from "@mui/material/styles";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import ArgonAvatar from "components/ArgonAvatar";

// Argon Dashboard 2 MUI example components
import DashboardNavbar from "controls/Navbars/DashboardNavbar";

// Argon Dashboard 2 MUI base styles
import breakpoints from "assets/theme/base/breakpoints";

// Images
import profileImage from "assets/images/profile.jpeg";

import type { CurrentUser } from "api/security/users";

interface HeaderProps {
  user: CurrentUser;
}

function Header({ user }: HeaderProps) {
  const [tabsOrientation, setTabsOrientation] = useState("horizontal");
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');

  useEffect(() => {
    const fetchData= async () => {
      if (user && Object.keys(user).length > 0) {
        setName(`${user.firstName} ${user.lastName}`);
        setEmail(user.emailAddress);
      }
    };
    fetchData();
  }, [user]);

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

  return (
    <ArgonBox position="relative">
      <DashboardNavbar absolute light />
      <ArgonBox height="220px" />
      <Card
        sx={{
          py: 2,
          px: 2,
          // `boxShadows` is declared on MUI's Theme via src/types/mui-theme.d.ts.
          // The object is still cast because Argon uses a per-property theme callback,
          // which MUI's strict `sx` type does not model.
          boxShadow: ({ boxShadows: { md } }: Theme) => md,
        } as unknown as SxProps<Theme>}
      >
        <Grid container spacing={3} alignItems="center">
          <Grid>
            <ArgonAvatar
              src={profileImage}
              alt="profile-image"
              variant="rounded"
              size="xl"
              shadow="sm"
            />
          </Grid>
          <Grid>
            <ArgonBox height="100%" mt={0.5} lineHeight={1}>
              <ArgonTypography variant="h5" fontWeight="medium">
                {name}
              </ArgonTypography>
              <ArgonTypography variant="button" color="text" fontWeight="medium">
                {email}
              </ArgonTypography>
            </ArgonBox>
          </Grid>
        </Grid>
      </Card>
    </ArgonBox>
  );
}

export default Header;
