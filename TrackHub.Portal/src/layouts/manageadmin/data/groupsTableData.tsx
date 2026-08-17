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
import ArgonButton from "components/ArgonButton";
import { useGroups, useCreateGroup, useUpdateGroup, useDeleteGroup } from 'queries/groups';
import type { Group, GroupDtoInput, UpdateGroupDtoInput } from 'api/manager/groups';
import { LoadingContext } from 'LoadingContext';
import { useAuth } from "AuthContext";

/**
 * Dialog/form state for a group. Merges an API {@link Group} (when editing) with
 * the fresh-add shape; all fields are optional and the `name`/`description`
 * requirement is enforced by the dialog's validate() gate before save.
 */
export interface GroupFormValues {
  groupId?: number;
  name?: string;
  description?: string;
  active?: boolean;
}

/** A column descriptor / rendered row for the vendored groups `Table`. */
export interface GroupColumn { name: string; title?: string; align?: "left" | "right" | "center"; }
export interface GroupRow {
  [key: string]: ReactNode;
  group: ReactNode;
  description: ReactNode;
  action: ReactNode;
  user: ReactNode;
  transporter: ReactNode;
  id: number;
}
export interface GroupTableData { columns: GroupColumn[]; rows: GroupRow[]; }

function useGroupTableData(
  fetchData: boolean,
  handleEditClick: (group: GroupFormValues) => void,
  handleDeleteClick: (groupId: number) => void,
  handleUserClick: (groupId: number) => void,
  handleTransporterClick: (groupId: number) => void
) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const { setLoading } = useContext(LoadingContext);
  const { isAuthenticated } = useAuth();

  const groupsQuery = useGroups({ enabled: !!fetchData && isAuthenticated });
  const groups = groupsQuery.data ?? [];
  const createGroup = useCreateGroup();
  const updateGroup = useUpdateGroup();
  const deleteGroup = useDeleteGroup();

  // Keep the global spinner UX for the initial load / invalidation refetch.
  useEffect(() => {
    setLoading(groupsQuery.isFetching);
  }, [groupsQuery.isFetching, setLoading]);

  const onSave = async (group: GroupFormValues) => {
    setLoading(true);
    try {
      // validate(['name','description']) gates this call, so the required
      // create/update input fields are present — assert them at the boundary.
      if (group.groupId) {
        await updateGroup.mutateAsync({
          groupId: group.groupId,
          name: group.name,
          description: group.description,
          active: group.active,
        } as { groupId: number } & Omit<UpdateGroupDtoInput, 'groupId'>);
      } else {
        await createGroup.mutateAsync({
          name: group.name,
          description: group.description,
          active: group.active,
        } as GroupDtoInput);
      }
      setOpen(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open.
    } finally {
      setLoading(false);
    }
  };

  const onDelete = async (groupId: number) => {
    setLoading(true);
    try {
      await deleteGroup.mutateAsync(groupId);
      setConfirmOpen(false);
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const handleOpenUser = (groupId: number) => {
    handleUserClick(groupId);
  };

  const handleOpenTransporter = (groupId: number) => {
    handleTransporterClick(groupId);
  };

  const handleOpen = (group: Group) => {
    handleEditClick(group);
    setOpen(true);
  };

  const handleOpenDelete = (groupId: number) => {
    handleDeleteClick(groupId);
    setConfirmOpen(true);
  };

  const buildTableData = (rows: Group[]): GroupTableData => ({
    columns: [
      { name: "group", title:t('group.title'), align: "left" },
      { name: "description", title:t('group.description'), align: "left" },
      { name: "action", title:t('generic.action'), align: "center" },
      { name: "user", title:t('user.title'), align: "center" },
      { name: "transporter", title:t('transporter.title'), align: "center" },
      { name: "id" }
    ],
    rows: rows.map(group => ({
      group: <Name name={group.name} />,
      description: <Description description={group.description} />,
      action: (
        <>
          <ArgonButton
              variant="text"
              color="dark"
              onClick={() => handleOpen(group)}>
            <Icon>edit</Icon>&nbsp;{t('generic.edit')}
          </ArgonButton>
          <ArgonButton
            variant="text"
            color="error"
            onClick={() => handleOpenDelete(group.groupId)}>
            <Icon>delete</Icon>&nbsp;{t('generic.delete')}
          </ArgonButton>
        </>
      ),
      user: (
        <ArgonButton
            variant="text"
            color="dark"
            onClick={() => handleOpenUser(group.groupId)}>
          <Icon>assignment</Icon>&nbsp;{t('generic.assign')}
        </ArgonButton>
      ),
      transporter: (
        <ArgonButton
            variant="text"
            color="dark"
            onClick={() => handleOpenTransporter(group.groupId)}>
          <Icon>assignment</Icon>&nbsp;{t('generic.assign')}
        </ArgonButton>
      ),
      id: group.groupId
    })),
  });

  const data = useMemo(
    () => buildTableData(groups),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [groups, t]
  );

  return {
    data,
    open,
    confirmOpen,
    onSave,
    onDelete,
    setOpen,
    setConfirmOpen };
}

export default useGroupTableData;
