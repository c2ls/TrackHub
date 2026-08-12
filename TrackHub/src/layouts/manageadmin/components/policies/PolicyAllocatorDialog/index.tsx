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

import { useState, useEffect, useContext } from 'react';
import { useTranslation } from 'react-i18next';
import DynamicTableDialog from 'controls/Dialogs/TableDialogs/DynamicTableDialog';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import { useUsersByAccount } from 'queries/users';
import { useUsersByPolicy, useCreateUserPolicy, useDeleteUserPolicy } from 'queries/policies';
import { LoadingContext } from 'LoadingContext';

interface PolicyAllocatorDialogProps {
  open: boolean;
  setOpen: (open: boolean) => void;
  policyId: number;
}

function PolicyAllocatorDialog({ open, setOpen, policyId }: PolicyAllocatorDialogProps) {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [userId, setUserId] = useState('');

  const accountUsersQuery = useUsersByAccount({ enabled: open });
  const accountUsers = accountUsersQuery.data ?? [];
  // Assigned users only matter while the dialog is open.
  const assignedQuery = useUsersByPolicy(open ? policyId : undefined);
  const assignedUsers = assignedQuery.data ?? [];
  const createUserPolicy = useCreateUserPolicy();
  const deleteUserPolicy = useDeleteUserPolicy();

  const columns = [
    { field: 'username', headerName: t('user.username') }
  ];

  // Keep the global spinner UX while the lists load/refresh.
  useEffect(() => {
    setLoading(accountUsersQuery.isFetching || assignedQuery.isFetching);
  }, [accountUsersQuery.isFetching, assignedQuery.isFetching, setLoading]);

  const users = accountUsers
    .filter(user => !assignedUsers.some(assignedUser => assignedUser.userId === user.userId))
    .map(user => ({
      value: user.userId,
      label: user.username
    }));

  const handleChange: FormChangeHandler = (event) => {
    setLoading(true);
    setUserId(String(event.target.value ?? ''));
    setLoading(false);
  };

  const handleAdd = async () => {
    setLoading(true);
    try {
      await createUserPolicy.mutateAsync({ userId, policyId });
      setUserId('');
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (selectedRows: number[]) => {
    setLoading(true);
    try {
      const deletePromises = selectedRows.map(index =>
        deleteUserPolicy.mutateAsync({ userId: assignedUsers[index].userId, policyId }));
      await Promise.all(deletePromises);
      setUserId('');
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const handleClose = async () => {
    setUserId('');
    setOpen(false);
  };

  return (
    <DynamicTableDialog
      title={t('policy.assignPolicy')}
      handleAdd={handleAdd}
      handleDelete={handleDelete}
      handleClose={handleClose}
      open={open}
      data={assignedUsers}
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

export default PolicyAllocatorDialog;
