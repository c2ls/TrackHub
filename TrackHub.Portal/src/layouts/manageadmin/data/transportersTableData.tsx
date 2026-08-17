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
import { Name } from "controls/Tables/components/tableComponents";
import Icon from "@mui/material/Icon";
import ArgonBadge from "components/ArgonBadge";
import ArgonButton from "components/ArgonButton";
import { useAccountByUser } from "queries/accounts";
import {
  useTransportersByAccount,
  useCreateTransporter,
  useUpdateTransporter,
  useDeleteTransporter,
} from 'queries/transporters';
import type {
  Transporter,
  TransporterDtoInput,
  UpdateTransporterDtoInput,
} from 'api/manager/transporters';
import { cleanString } from 'utils/stringUtils';
import { LoadingContext } from 'LoadingContext';
import { useAuth } from "AuthContext";

/**
 * Dialog/form state for a transporter. Merges an API {@link Transporter} (when
 * editing) with the fresh-add shape; all fields are optional and the
 * `name`/`transporterTypeId` requirement is enforced by the dialog's validate()
 * gate before save.
 */
export interface TransporterFormValues {
  transporterId?: string;
  name?: string;
  transporterType?: string;
  transporterTypeId?: number;
}

/** A column descriptor / rendered row for the vendored transporters `Table`. */
export interface TransporterTableColumn { name: string; title?: string; align?: "left" | "right" | "center"; }
export type TransporterTableRow = Record<string, ReactNode>;
export interface TransporterTableData { columns: TransporterTableColumn[]; rows: TransporterTableRow[]; }

function useTransporterTableData(
  fetchData: boolean,
  handleEditClick: (transporter: TransporterFormValues) => void,
  handleDeleteClick: (transporterId: string) => void
) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const { setLoading } = useContext(LoadingContext);
  const { isAuthenticated } = useAuth();

  const transportersQuery = useTransportersByAccount({ enabled: !!fetchData && isAuthenticated });
  const transporters = transportersQuery.data ?? [];
  const createTransporter = useCreateTransporter();
  const updateTransporter = useUpdateTransporter();
  const deleteTransporter = useDeleteTransporter();

  // Current account id, required to create a transporter (TransporterDtoInput.accountId).
  // Loaded the same way sibling manageadmin screens (drivers, accountFeatures) obtain it.
  const accountQuery = useAccountByUser({ enabled: !!fetchData && isAuthenticated });
  const accountId = accountQuery.data?.accountId ?? null;

  // Keep the global spinner UX for the initial load / invalidation refetch.
  useEffect(() => {
    setLoading(transportersQuery.isFetching);
  }, [transportersQuery.isFetching, setLoading]);

  const handleOpen = (transporter: Transporter) => {
    handleEditClick(transporter);
    setOpen(true);
  };

  const handleOpenDelete = (transporterId: string) => {
    handleDeleteClick(transporterId);
    setConfirmOpen(true);
  };

  const onSave = async (transporter: TransporterFormValues) => {
    setLoading(true);
    try {
      // validate(['name','transporterTypeId']) gates this call, so the required
      // create/update input fields are present — assert them at the boundary.
      if (transporter.transporterId) {
        await updateTransporter.mutateAsync({
          transporterId: transporter.transporterId,
          name: transporter.name,
          transporterTypeId: transporter.transporterTypeId,
        } as { transporterId: string } & Omit<UpdateTransporterDtoInput, 'transporterId'>);
      } else {
        await createTransporter.mutateAsync({
          name: transporter.name,
          transporterTypeId: transporter.transporterTypeId,
          accountId,
        } as TransporterDtoInput);
      }
      setOpen(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open so the
      // user can retry without re-entering the values.
    } finally {
      setLoading(false);
    }
  };

  const onDelete = async (transporterId: string) => {
    setLoading(true);
    try {
      await deleteTransporter.mutateAsync(transporterId);
      setConfirmOpen(false);
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const buildTableData = (rows: Transporter[]): TransporterTableData => ({
    columns: [
      { name: "name", title:t('transporter.name'), align: "left" },
      { name: "transporterType", title:t('transporter.type'), align: "center" },
      { name: "action", title:t('generic.action'), align: "center" },
      { name: "id" }
    ],
    rows: rows.map(transporter => ({
      name: <Name name={transporter.name} />,
      transporterType: (
        <ArgonBadge variant="gradient"
          badgeContent={t(`transporterTypes.${cleanString(transporter.transporterType)}` as 'transporterTypes.car')}
          color="success" size="xs" container />
      ),
      action: (
        <>
          <ArgonButton
              variant="text"
              color="dark"
              onClick={() => handleOpen(transporter)}>
            <Icon>edit</Icon>&nbsp;{t('generic.edit')}
          </ArgonButton>
          <ArgonButton
            variant="text"
            color="error"
            onClick={() => handleOpenDelete(transporter.transporterId)}>
            <Icon>delete</Icon>&nbsp;{t('generic.delete')}
          </ArgonButton>
        </>
      ),
      id: transporter.transporterId
    })),
  });

  const data = useMemo(
    () => buildTableData(transporters),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [transporters, t]
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

export default useTransporterTableData;
