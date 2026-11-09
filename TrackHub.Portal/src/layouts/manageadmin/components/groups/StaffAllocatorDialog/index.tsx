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

import { useEffect, useMemo, useState, useContext } from 'react';
import { useTranslation } from 'react-i18next';
import DynamicTableDialog from 'controls/Dialogs/TableDialogs/DynamicTableDialog';
import SearchSelect from 'edition/SearchSelect';
import type { SearchOption } from 'edition/SearchSelect';
import { accountStaffOptions, excludingOptions } from 'edition/pickerOptions';
import { useAccountByUser } from 'queries/accounts';
import { useStaffByGroup } from 'queries/groups';
import { createStaffGroup, deleteStaffGroup } from 'api/manager/groups';
import { notifyApiError } from 'api/core/errors';
import { LoadingContext } from 'LoadingContext';

interface StaffAllocatorDialogProps {
  open: boolean;
  setOpen: (open: boolean) => void;
  groupId: number;
}

function StaffAllocatorDialog({ open, setOpen, groupId }: StaffAllocatorDialogProps) {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [staff, setStaff] = useState<SearchOption | null>(null);
  const accountId = useAccountByUser({ enabled: open }).data?.accountId;

  const assignedQuery = useStaffByGroup(open ? groupId : undefined);
  const assignedStaff = assignedQuery.data ?? [];
  const useAvailableStaffOptions = useMemo(
    () => excludingOptions(accountStaffOptions(accountId, true), new Set(assignedStaff.map((assigned) => assigned.staffId))),
    [accountId, assignedStaff]
  );

  const columns = [{ field: 'name', headerName: t('staff.name') }];

  useEffect(() => {
    setLoading(assignedQuery.isFetching);
  }, [assignedQuery.isFetching, setLoading]);

  const handleAdd = async () => {
    if (!staff) return;
    setLoading(true);
    try {
      await createStaffGroup(staff.value, groupId);
    } catch (e) {
      notifyApiError(e);
    }
    setStaff(null);
    await assignedQuery.refetch();
    setLoading(false);
  };

  const handleDelete = async (selectedRows: number[]) => {
    setLoading(true);
    await Promise.all(selectedRows.map((index) =>
      deleteStaffGroup(assignedStaff[index].staffId, groupId).catch(() => undefined)));
    await assignedQuery.refetch();
    setLoading(false);
  };

  const handleClose = () => {
    setStaff(null);
    setOpen(false);
  };

  return (
    <DynamicTableDialog
      title={t('group.assignStaff')}
      handleAdd={handleAdd}
      handleDelete={handleDelete}
      handleClose={handleClose}
      open={open}
      data={assignedStaff}
      columns={columns}>
      <SearchSelect
        id="staffId"
        label={t('staff.singleTitle')}
        value={staff?.value ?? null}
        valueLabel={staff?.label}
        onChange={setStaff}
        useOptions={useAvailableStaffOptions}
      />
    </DynamicTableDialog>
  );
}

export default StaffAllocatorDialog;
