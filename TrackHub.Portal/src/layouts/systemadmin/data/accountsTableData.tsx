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
import { useTranslation } from 'react-i18next';
import { Name, Description } from "controls/Tables/components/tableComponents";
import Icon from "@mui/material/Icon";
import ArgonTypography from "components/ArgonTypography";
import ArgonBadge from "components/ArgonBadge";
import ArgonButton from "components/ArgonButton";
import {
  useAccounts,
  useCreateAccount,
  useUpdateAccount,
  useChangeAccountStatus,
} from "queries/accounts";
import type { Account, AccountDtoInput, UpdateAccountDtoInput, AccountStatus } from "api/manager/accounts";
import { useCreateManager } from "queries/users";
import type { CreateUserDtoInput } from "api/security/users";
import { formatDateTime } from "utils/dateUtils";
import { LoadingContext } from 'LoadingContext';
import {
  ACCOUNT_STATUS_NAME,
  ACCOUNT_STATUS_COLOR,
  ACCOUNT_STATUS_I18N,
  ALLOWED_TRANSITIONS
} from "data/accountStatuses";
import type { AccountStatusId } from "data/accountStatuses";

/**
 * Dialog/form state for an account. Merges an API {@link Account} (when editing)
 * with the create-only user fields collected by the dialog; all fields are
 * optional and the `name`/`typeId` requirement is enforced by the dialog's
 * validate() gate before save.
 */
export interface AccountFormValues {
  accountId?: string;
  name?: string;
  description?: string | null;
  typeId?: number;
  statusId?: number;
  active?: boolean;
  // Create-only user fields.
  password?: string;
  firstName?: string;
  lastName?: string;
  emailAddress?: string;
  lastModified?: string;
}

/** Dialog/form state for the "add manager" flow (loose until the validate() gate). */
export interface AccountUserFormValues {
  accountId?: string;
  active?: boolean;
  username?: string;
  password?: string;
  emailAddress?: string;
  firstName?: string;
  lastName?: string;
  secondName?: string | null;
  secondSurname?: string | null;
  dob?: string | null;
  integrationUser?: boolean;
}

/** Dialog/form state for the account lifecycle (status change) dialog. */
export interface AccountStatusFormValues {
  accountId?: string;
  statusId?: number;
  targetStatus?: AccountStatus | '';
  reason?: string | null;
}

/** A column descriptor / rendered row for the vendored accounts `Table`. */
export interface AccountTableColumn { name: string; title?: string; align?: "left" | "right" | "center"; }
export type AccountTableRow = Record<string, ReactNode>;
export interface AccountTableData { columns: AccountTableColumn[]; rows: AccountTableRow[]; }

function useAccountsTableData(
  fetchData: boolean,
  handleEditClick: (account: AccountFormValues) => void,
  handleAddManagerClick: (accountId: string) => void,
  handleStatusClick: (account: Account) => void
) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [openUser, setOpenUser] = useState(false);
  const [openStatus, setOpenStatus] = useState(false);
  const { setLoading } = useContext(LoadingContext);

  const accountsQuery = useAccounts({ enabled: !!fetchData });
  const accounts = accountsQuery.data ?? [];
  const createAccount = useCreateAccount();
  const updateAccount = useUpdateAccount();
  const changeAccountStatus = useChangeAccountStatus();
  const createManager = useCreateManager();

  // Keep the global spinner UX for the initial load / invalidation refetch.
  useEffect(() => {
    setLoading(accountsQuery.isFetching);
  }, [accountsQuery.isFetching, setLoading]);

  const onSave = async (account: AccountFormValues) => {
    setLoading(true);
    try {
      // validate(['name','typeId']) gates this call; assert the required create/
      // update input fields at the boundary.
      if (account.accountId) {
        await updateAccount.mutateAsync({
          accountId: account.accountId,
          name: account.name,
          description: account.description,
          typeId: account.typeId,
          active: account.active,
        } as { accountId: string } & Omit<UpdateAccountDtoInput, 'accountId'>);
      } else {
        await createAccount.mutateAsync({
          active: account.active,
          typeId: account.typeId,
          password: account.password,
          name: account.name,
          lastName: account.lastName,
          firstName: account.firstName,
          emailAddress: account.emailAddress,
          description: account.description,
        } as AccountDtoInput);
      }
      setOpen(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open so the
      // user can retry without re-entering the values.
    } finally {
      setLoading(false);
    }
  };

  const onSaveUser = async (user: AccountUserFormValues) => {
    setLoading(true);
    try {
      await createManager.mutateAsync({ user: user as CreateUserDtoInput, accountId: user.accountId as string });
      setOpenUser(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open.
    } finally {
      setLoading(false);
    }
  };

  const handleOpen = (account: Account) => {
    handleEditClick(account);
    setOpen(true);
  };

  const handleOpenUser = (accountId: string) => {
    handleAddManagerClick(accountId);
    setOpenUser(true);
  };

  const handleOpenStatus = (account: Account) => {
    handleStatusClick(account);
    setOpenStatus(true);
  };

  const onChangeStatus = async (statusValues: AccountStatusFormValues) => {
    setLoading(true);
    try {
      await changeAccountStatus.mutateAsync({
        accountId: statusValues.accountId as string,
        targetStatus: statusValues.targetStatus as AccountStatus,
        reason: statusValues.reason,
      });
      setOpenStatus(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open.
    } finally {
      setLoading(false);
    }
  };

  const buildTableData = (rows: Account[]): AccountTableData => ({
    columns: [
      { name: "name", title:t('account.name'), align: "left" },
      { name: "description", title:t('account.description'), align: "left" },
      { name: "type", title:t('account.type'), align: "center" },
      { name: "status", title:t('account.status'), align: "center" },
      { name: "modified", title:t('generic.modified'), align: "center" },
      { name: "action", title:t('generic.action'), align: "center" },
      { name: "lifecycle", title:t('account.changeStatus'), align: "center" },
      { name: "user", title:t('account.addUser'), align: "center" },
      { name: "id" }
    ],
    rows: rows.map(account => ({
      name: <Name name={account.name} />,
      description: <Description description={account.description} />,
      type: (
        <ArgonBadge variant="gradient" badgeContent={t(`accountType.${(account.type || '').toLowerCase()}` as 'accountType.personal', { defaultValue: account.type })} color="success" size="xs" container />
      ),
      status: (() => {
        const name = ACCOUNT_STATUS_NAME[account.statusId as AccountStatusId] || 'ACTIVE';
        return (
          <ArgonBadge
            variant="gradient"
            badgeContent={t(ACCOUNT_STATUS_I18N[name])}
            color={ACCOUNT_STATUS_COLOR[name]}
            size="xs"
            container />
        );
      })(),
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
      lifecycle: (
        (ALLOWED_TRANSITIONS[account.statusId as AccountStatusId] || []).length > 0 ? (
          <ArgonButton
              variant="text"
              color="dark"
              onClick={() => handleOpenStatus(account)}>
            <Icon>sync_alt</Icon>&nbsp;{t('account.changeStatus')}
          </ArgonButton>
        ) : null
      ),
      user: (
        <ArgonButton
            variant="text"
            color="dark"
            onClick={() => handleOpenUser(account.accountId)}>
          <Icon>add</Icon>&nbsp;{t('generic.add')}
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

  return {
    data,
    open,
    openUser,
    openStatus,
    onSave,
    onSaveUser,
    onChangeStatus,
    setOpen,
    setOpenUser,
    setOpenStatus,};
}

export default useAccountsTableData;
