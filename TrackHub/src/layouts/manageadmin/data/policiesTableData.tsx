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
import { usePolicies } from "queries/policies";
import type { Policy } from "api/security/policies";
import { LoadingContext } from 'LoadingContext';
import { toCamelCase } from 'utils/stringUtils';
import { useAuth } from "AuthContext";

/** A column descriptor / rendered row for the vendored policies `Table`. */
export interface PolicyTableColumn { name: string; title?: string; align?: "left" | "right" | "center"; }
export type PolicyTableRow = Record<string, ReactNode>;
export interface PolicyTableData { columns: PolicyTableColumn[]; rows: PolicyTableRow[]; }

function usePolicyTableData(fetchData: boolean, handleOpenClick: (policyId: number) => void) {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const { isAuthenticated } = useAuth();

  const policiesQuery = usePolicies({ enabled: !!fetchData && isAuthenticated });
  const policies = policiesQuery.data ?? [];

  const handleOpen = (policyId: number) => {
    handleOpenClick(policyId);
  };

  const buildTableData = (policies: Policy[]): PolicyTableData => ({
    columns: [
      { name: "name", title:t('policy.title'), align: "left" },
      { name: "action", title:t('generic.action'), align: "center" },
      { name: "id" }
    ],
    rows: policies.map(policy => ({
      name: <Name name={t(`policies.${toCamelCase(policy.name)}` as 'policies.fullAccess', { defaultValue: policy.name })} />,
      action: (
        <ArgonButton
            variant="text"
            color="dark"
            onClick={() => handleOpen(policy.policyId)}>
          <Icon>assignment</Icon>&nbsp;{t('generic.assign')}
        </ArgonButton>
      ),
      id: policy.policyId
    })),
  });

  // Keep the global spinner UX for the initial load / invalidation refetch.
  useEffect(() => {
    setLoading(policiesQuery.isFetching);
  }, [policiesQuery.isFetching, setLoading]);

  const data = useMemo(
    () => buildTableData(policies),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [policies, t]
  );

  return {
    data
  };
}

export default usePolicyTableData;
