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

import { useEffect, useState } from "react";

// @mui material components
import Card from "@mui/material/Card";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import { useTranslation } from 'react-i18next';

import type { CurrentUser } from "api/security/users";

type ProfileRole = NonNullable<CurrentUser['roles']>[number];
type ProfilePolicy = NonNullable<CurrentUser['profiles']>[number];

interface UserPartOfProps {
  user: CurrentUser;
}

function UserPartOf({ user }: UserPartOfProps) {
  const { t } = useTranslation();
  const [roles, setRoles] = useState<ProfileRole[]>([]);
  const [policies, setPolicies] = useState<ProfilePolicy[]>([]);

  useEffect(() => {
    const fetchRoles= async () => {
      if (user && Object.keys(user).length > 0) {
        setRoles(user.roles ?? []);
        setPolicies(user.profiles ?? []);
      }
    };
    fetchRoles();
  }, [user]);

  return (
    <Card sx={{ height: "100%" }}>
      <ArgonBox pt={2} px={2}>
        <ArgonTypography variant="h6" fontWeight="medium" textTransform="capitalize">
          {t('role.title')}
        </ArgonTypography>
      </ArgonBox>
      <ArgonBox p={2}>
        <ArgonBox component="ul" display="flex" flexDirection="column" p={0} m={0}>
        {roles.map((role) => (
            role.name &&
            <ArgonBox key={role.name} component="li" display="flex" alignItems="center" py={1} mb={1}>
                <ArgonBox
                display="flex"
                flexDirection="column"
                alignItems="flex-start"
                justifyContent="center">
                <ArgonTypography variant="button" fontWeight="medium">
                    {role.name}
                </ArgonTypography>
                </ArgonBox>
            </ArgonBox>
            ))}
        </ArgonBox>
      </ArgonBox>
      <ArgonBox pt={2} px={2}>
        <ArgonTypography variant="h6" fontWeight="medium" textTransform="capitalize">
          {t('policy.title')}
        </ArgonTypography>
      </ArgonBox>
      <ArgonBox p={2}>
        <ArgonBox component="ul" display="flex" flexDirection="column" p={0} m={0}>
        {policies.map((policy) => (
            policy.name &&
            <ArgonBox key={policy.name} component="li" display="flex" alignItems="center" py={1} mb={1}>
                <ArgonBox
                display="flex"
                flexDirection="column"
                alignItems="flex-start"
                justifyContent="center">
                <ArgonTypography variant="button" fontWeight="medium">
                    {policy.name}
                </ArgonTypography>
                </ArgonBox>
            </ArgonBox>
            ))}
        </ArgonBox>
      </ArgonBox>
    </Card>
  );
}

export default UserPartOf;
