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

import { useEffect, useMemo, useContext } from "react";
import type { ReactNode } from "react";
import { useTranslation } from 'react-i18next';
import { Name } from "controls/Tables/components/tableComponents";
import Icon from "@mui/material/Icon";
import ArgonButton from "components/ArgonButton";
import { useRoles } from "queries/roles";
import type { Role } from "api/security/roles";
import { LoadingContext } from 'LoadingContext';
import { toCamelCase } from 'utils/stringUtils';
import { useAuth } from "AuthContext";

/** A column descriptor / rendered row for the vendored roles `Table`. */
export interface RoleTableColumn { name: string; title?: string; align?: "left" | "right" | "center"; }
export type RoleTableRow = Record<string, ReactNode>;
export interface RoleTableData { columns: RoleTableColumn[]; rows: RoleTableRow[]; }

function useRoleTableData(fetchData: boolean, handleOpenClick: (roleId: number) => void) {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const { isAuthenticated } = useAuth();

  const rolesQuery = useRoles({ enabled: !!fetchData && isAuthenticated });
  const roles = rolesQuery.data ?? [];

  const handleOpen = (roleId: number) => {
    handleOpenClick(roleId);
  };

  const buildTableData = (roles: Role[]): RoleTableData => ({
    columns: [
      { name: "name", title:t('role.title'), align: "left" },
      { name: "action", title:t('generic.action'), align: "center" },
      { name: "id" }
    ],
    rows: roles.map(role => ({
      name: <Name name={t(`roles.${toCamelCase(role.name)}` as 'roles.manager', { defaultValue: role.name })} />,
      action: (
        <ArgonButton
            variant="text"
            color="dark"
            onClick={() => handleOpen(role.roleId)}>
          <Icon>assignment</Icon>&nbsp;{t('generic.assign')}
        </ArgonButton>
      ),
      id: role.roleId
    })),
  });

  // Keep the global spinner UX for the initial load / invalidation refetch.
  useEffect(() => {
    setLoading(rolesQuery.isFetching);
  }, [rolesQuery.isFetching, setLoading]);

  const data = useMemo(
    () => buildTableData(roles),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [roles, t]
  );

  return {
    data
  };
}

export default useRoleTableData;
