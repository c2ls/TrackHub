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

import { useEffect, useContext, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQueryClient } from '@tanstack/react-query';
import DynamicTableDialog from 'controls/Dialogs/TableDialogs/DynamicTableDialog';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import { useUsersByAccount } from 'queries/users';
import { useUsersByGroup, groupKeys } from 'queries/groups';
import { createUserGroup, deleteUserGroup } from 'api/manager/groups';
import type { GroupUser } from 'api/manager/groups';
import { notifyApiError } from 'api/core/errors';
import { LoadingContext } from 'LoadingContext';

interface SelectOption { value: string; label: string; }

interface UserAllocatorDialogProps {
  open: boolean;
  setOpen: (open: boolean) => void;
  groupId: number;
}

function UserAllocatorDialog({ open, setOpen, groupId }: UserAllocatorDialogProps) {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const queryClient = useQueryClient();
  const [userId, setUserId] = useState('');

  // Account users come from the security query layer; group membership now comes
  // from the manager groups query layer (invalidated after each add/remove).
  const accountUsersQuery = useUsersByAccount({ enabled: open });
  const accountUsers = accountUsersQuery.data ?? [];
  const assignedUsersQuery = useUsersByGroup(open ? groupId : undefined);
  const data = assignedUsersQuery.data ?? [];

  const columns = [
    { field: 'username', headerName: t('user.username') }
  ];

  const users: SelectOption[] = accountUsers
    .filter(user => !data.some(assignedUser => assignedUser.userId === user.userId))
    .map(user => ({
      value: user.userId,
      label: user.username
    }));

  const reloadData = async () => {
    await queryClient.invalidateQueries({ queryKey: groupKeys.usersByGroup(groupId) });
    setUserId('');
  };

  // Keep the global spinner UX while the account-user / membership lists load.
  useEffect(() => {
    setLoading(accountUsersQuery.isFetching || assignedUsersQuery.isFetching);
  }, [accountUsersQuery.isFetching, assignedUsersQuery.isFetching, setLoading]);

  const handleChange: FormChangeHandler = (event) => {
    setLoading(true);
    setUserId(String(event.target.value ?? ''));
    setLoading(false);
  };

  const handleAdd = async () => {
    setLoading(true);
    try {
      // createUserGroup surfaces failures via the global toast (legacy handleError).
      await createUserGroup(userId, groupId);
    } catch (e) {
      notifyApiError(e);
    }
    await reloadData();
    setLoading(false);
  };

  const handleDelete = async (selectedRows: number[]) => {
    setLoading(true);
    // deleteUserGroup keeps the legacy silent semantics (handleSilentError).
    const deletePromises = selectedRows.map(index =>
      deleteUserGroup(data[index].userId, groupId).catch(() => undefined));
    await Promise.all(deletePromises);
    await reloadData();
    setLoading(false);
  };

  const handleClose = async () => {
    setUserId('');
    setOpen(false);
  };

  return (
    <DynamicTableDialog
      title={t('group.assignUser')}
      handleAdd={handleAdd}
      handleDelete={handleDelete}
      handleClose={handleClose}
      open={open}
      data={data}
      columns={columns}>
      <CustomSelect
        list={users}
        name="userId"
        id="userId"
        label={t('user.singleTitle')}
        value={userId}
        handleChange={handleChange}
        numericValue={false}
        required
      />
    </DynamicTableDialog>
  );
}

export default UserAllocatorDialog;
