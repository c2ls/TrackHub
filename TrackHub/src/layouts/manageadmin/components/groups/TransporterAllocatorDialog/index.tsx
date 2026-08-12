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

import { useEffect, useState, useContext } from 'react';
import { useTranslation } from 'react-i18next';
import DynamicTableDialog from 'controls/Dialogs/TableDialogs/DynamicTableDialog';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import { useTransportersByAccount, useTransportersByGroup } from 'queries/transporters';
import { createTransporterGroup, deleteTransporterGroup } from 'api/manager/groups';
import type { Transporter } from 'api/manager/transporters';
import { notifyApiError } from 'api/core/errors';
import { LoadingContext } from 'LoadingContext';
import { useAuth } from "AuthContext";

interface SelectOption { value: string; label: string; }

interface TransporterAllocatorDialogProps {
  open: boolean;
  setOpen: (open: boolean) => void;
  groupId: number;
}

function TransporterAllocatorDialog({ open, setOpen, groupId }: TransporterAllocatorDialogProps) {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const { isAuthenticated } = useAuth();
  const [transporterId, setTransporterId] = useState('');

  const accountTransportersQuery = useTransportersByAccount({ enabled: isAuthenticated });
  const accountTransporters = accountTransportersQuery.data ?? [];
  // Assigned transporters only matter while the dialog is open.
  const assignedQuery = useTransportersByGroup(open ? groupId : undefined);
  const assignedTransporters = assignedQuery.data ?? [];

  const columns = [
    { field: 'name', headerName: t('transporter.name') }
  ];

  // Keep the global spinner UX while the assigned-transporter list loads/refreshes.
  useEffect(() => {
    setLoading(assignedQuery.isFetching);
  }, [assignedQuery.isFetching, setLoading]);

  const unassignedTransporters: SelectOption[] = accountTransporters
    .filter(transporter => !assignedTransporters.some(assigned => assigned.transporterId === transporter.transporterId))
    .map(transporter => ({
      value: transporter.transporterId,
      label: transporter.name
    }));

  const handleChange: FormChangeHandler = (event) => {
    setLoading(true);
    setTransporterId(String(event.target.value ?? ''));
    setLoading(false);
  };

  const handleAdd = async () => {
    setLoading(true);
    try {
      // createTransporterGroup surfaces failures via the global toast (legacy handleError).
      await createTransporterGroup(transporterId, groupId);
    } catch (e) {
      notifyApiError(e);
    }
    setTransporterId('');
    // Group membership is read via the transporters query; refetch it manually.
    await assignedQuery.refetch();
    setLoading(false);
  };

  const handleDelete = async (selectedRows: number[]) => {
    setLoading(true);
    // deleteTransporterGroup keeps the legacy silent semantics (handleSilentError).
    const deletePromises = selectedRows.map(index =>
      deleteTransporterGroup(assignedTransporters[index].transporterId, groupId).catch(() => undefined));
    await Promise.all(deletePromises);
    await assignedQuery.refetch();
    setLoading(false);
  };

  const handleClose = async () => {
    setTransporterId('');
    setOpen(false);
  };

  return (
    <DynamicTableDialog
      title={t('group.assignTransporter')}
      handleAdd={handleAdd}
      handleDelete={handleDelete}
      handleClose={handleClose}
      open={open}
      data={assignedTransporters}
      columns={columns}>
      <CustomSelect
        list={unassignedTransporters}
        name="transporterId"
        id="transporterId"
        label={t('transporter.singleTitle')}
        value={transporterId}
        handleChange={handleChange}
        numericValue={false}
        required
      />
    </DynamicTableDialog>
  );
}

export default TransporterAllocatorDialog;
