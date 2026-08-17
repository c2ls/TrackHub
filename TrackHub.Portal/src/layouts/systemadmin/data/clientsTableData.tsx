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
import ArgonButton from "components/ArgonButton";
import { useClients, useCreateClient, useUpdateClient, useDeleteClient } from "queries/clients";
import type { Client, ClientDtoInput } from "api/security/clients";
import { useIntegrationUsers } from "queries/users";
import { formatDateTime } from "utils/dateUtils";
import { LoadingContext } from 'LoadingContext';
import { getStringValue } from 'utils/booleanUtils';

/**
 * Dialog/form state for a client. Merges an API {@link Client} (when editing)
 * with the create-only `secret` field; all fields are optional and the
 * requirement set is enforced by the dialog's validate() gate before save.
 */
export interface ClientFormValues {
  clientId?: string;
  userId?: string | null;
  name?: string;
  description?: string;
  secret?: string;
  processed?: boolean;
  lastModified?: string;
}

/** Option row for the linked-user select. */
export interface ClientUserOption { value: string; label: string; }

/** A column descriptor / rendered row for the vendored clients `Table`. */
export interface ClientTableColumn { name: string; title?: string; align?: "left" | "right" | "center"; }
export type ClientTableRow = Record<string, ReactNode>;
export interface ClientTableData { columns: ClientTableColumn[]; rows: ClientTableRow[]; }

function useClientsTableData(
  fetchData: boolean,
  handleEditClick: (client: ClientFormValues) => void,
  handleDeleteClick: (clientId: string) => void
) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const { setLoading } = useContext(LoadingContext);

  const enabled = !!fetchData;
  const clientsQuery = useClients({ enabled });
  const clients = clientsQuery.data ?? [];
  const usersQuery = useIntegrationUsers({ enabled });
  const createClient = useCreateClient();
  const updateClient = useUpdateClient();
  const deleteClient = useDeleteClient();

  const users = useMemo<ClientUserOption[]>(
    () => (usersQuery.data ?? []).map(user => ({ value: user.userId, label: user.emailAddress })),
    [usersQuery.data]
  );

  // Keep the global spinner UX for the initial load / invalidation refetch.
  useEffect(() => {
    setLoading(clientsQuery.isFetching || usersQuery.isFetching);
  }, [clientsQuery.isFetching, usersQuery.isFetching, setLoading]);

  const onSave = async (client: ClientFormValues) => {
    setLoading(true);
    try {
      // validate() gates this call; assert the required create/update fields.
      if (client.clientId) {
        await updateClient.mutateAsync({ clientId: client.clientId, userId: client.userId } as { clientId: string; userId: string });
      } else {
        await createClient.mutateAsync(client as ClientDtoInput);
      }
      setOpen(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open so the
      // user can retry without re-entering the values.
    } finally {
      setLoading(false);
    }
  };

  const onDelete = async (clientId: string) => {
    setLoading(true);
    try {
      await deleteClient.mutateAsync(clientId);
      setConfirmOpen(false);
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const handleOpen = (client: Client) => {
    handleEditClick(client);
    setOpen(true);
  };

  const handleOpenDelete = (clientId: string) => {
    handleDeleteClick(clientId);
    setConfirmOpen(true);
  };

  const buildTableData = (rows: Client[]): ClientTableData => ({
    columns: [
      { name: "name", title:t('clients.name'), align: "left" },
      { name: "description", title:t('clients.description'), align: "left" },
      { name: "processed", title:t('clients.processed'), align: "center" },
      { name: "modified", title:t('generic.modified'), align: "center" },
      { name: "action", title:t('generic.action'), align: "center" },
      { name: "id" }
    ],
    rows: rows.map(client => ({
      name: <Name name={client.name} />,
      description: <Description description={client.description} />,
      processed: <Description description={t(`generic.${getStringValue(client.processed)}` as 'generic.yes')} />,
      modified: (
        <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
          {formatDateTime(client.lastModified)}
        </ArgonTypography>
      ),
      action: (
        <>
          <ArgonButton
              variant="text"
              color="dark"
              onClick={() => handleOpen(client)}>
            <Icon>edit</Icon>&nbsp;{t('generic.edit')}
          </ArgonButton>
          <ArgonButton
            variant="text"
            color="error"
            onClick={() => handleOpenDelete(client.clientId)}>
            <Icon>delete</Icon>&nbsp;{t('generic.delete')}
          </ArgonButton>
        </>
      ),
      id: client.clientId
    })),
  });

  const data = useMemo(
    () => buildTableData(clients),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [clients, t]
  );

  return {
    data,
    users,
    open,
    confirmOpen,
    onSave,
    onDelete,
    setOpen,
    setConfirmOpen};
}

export default useClientsTableData;
