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

import { useEffect, useMemo, useState, useContext } from "react";
import type { ReactNode } from "react";
import { Name, Description } from "controls/Tables/components/tableComponents";
import ArgonTypography from "components/ArgonTypography";
import ArgonBadge from "components/ArgonBadge";
import ArgonButton from "components/ArgonButton";
import Icon from "@mui/material/Icon";
import { useAccountByUser, useUpdateAccount } from "queries/accounts";
import type { Account, UpdateAccountDtoInput } from "api/manager/accounts";
import { formatDateTime } from "utils/dateUtils";
import { LoadingContext } from 'LoadingContext';
import { useTranslation } from 'react-i18next';
import { useAuth } from "AuthContext";

/**
 * Dialog/form state for an account. Merges an API {@link Account} (when editing)
 * with the partial dialog shape; all fields are optional and the `name`
 * requirement is enforced by the dialog's validate() gate before save.
 */
export interface AccountFormValues {
  accountId?: string;
  name?: string;
  description?: string | null;
  type?: string;
  typeId?: number;
  active?: boolean;
  lastModified?: string;
}

/** A column descriptor / rendered row for the vendored accounts `Table`. */
export interface AccountTableColumn { name: string; title?: string; align?: "left" | "right" | "center"; }
export type AccountTableRow = Record<string, ReactNode>;
export interface AccountTableData { columns: AccountTableColumn[]; rows: AccountTableRow[]; }

function useAccountTableData(fetchData: boolean, handleEditClick: (account: AccountFormValues) => void) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const { setLoading } = useContext(LoadingContext);
  const { isAuthenticated } = useAuth();

  const accountQuery = useAccountByUser({ enabled: !!fetchData && isAuthenticated });
  const accounts = accountQuery.data ? [accountQuery.data] : [];
  const updateAccount = useUpdateAccount();

  // Keep the global spinner UX for the initial load / invalidation refetch.
  useEffect(() => {
    setLoading(accountQuery.isFetching);
  }, [accountQuery.isFetching, setLoading]);

  const onSave = async (account: AccountFormValues) => {
    setLoading(true);
    try {
      // validate(['name']) gates this call; assert the required update-input
      // fields at the boundary (the row supplies the rest when editing).
      await updateAccount.mutateAsync({
        accountId: account.accountId,
        name: account.name,
        description: account.description,
        typeId: account.typeId,
        active: account.active,
      } as { accountId: string } & Omit<UpdateAccountDtoInput, 'accountId'>);
      setOpen(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open.
    } finally {
      setLoading(false);
    }
  }

  const handleOpen = (account: Account) => {
    handleEditClick(account);
    setOpen(true);
  };

  const buildTableData = (rows: Account[]): AccountTableData => ({
    columns: [
      { name: "name", title:t('account.name'), align: "left" },
      { name: "description", title:t('account.description'), align: "left" },
      { name: "type", title:t('account.type'), align: "center" },
      { name: "modified", title:t('generic.modified'), align: "center" },
      { name: "action", title:t('generic.action'), align: "center" },
      { name: "id" }
    ],
    rows: rows.map(account => ({
      name: <Name name={account.name} />,
      description: <Description description={account.description} />,
      type: (
        <ArgonBadge variant="gradient" badgeContent={account.type} color="success" size="xs" container />
      ),
      modified: (
        <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
          {formatDateTime(account.lastModified)}
        </ArgonTypography>
      ),
      action: (
        <ArgonButton
            variant="text"
            color="dark"
            onClick={() => handleOpen(account)}>
          <Icon>edit</Icon>&nbsp;{t('generic.edit')}
        </ArgonButton>
      ),
      id: account.accountId
    })),
  });

  const data = useMemo(
    () => buildTableData(accounts),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [accounts, t]
  );

  return { data, open, onSave, setOpen };
}

export default useAccountTableData;
