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

import { useEffect, useState, useContext } from "react";
import type { ReactNode } from "react";
import { useTranslation } from 'react-i18next';
import { NameDetail, Description } from "controls/Tables/components/tableComponents";
import Icon from "@mui/material/Icon";
import ArgonTypography from "components/ArgonTypography";
import ArgonBadge from "components/ArgonBadge";
import ArgonButton from "components/ArgonButton";
import { getCredentialByOperator, createCredential, updateCredential } from "api/manager/credential";
import type { Credential, CredentialFormInput } from "api/manager/credential";
import { notifyApiError } from "api/core/errors";
import { pingOperator } from "api/router/router";
import {
  useOperatorsByCurrentAccount,
  useCreateOperator,
  useUpdateOperator,
  useDeleteOperator,
  useSetOperatorEnabled,
  useTriggerOperatorDeviceSync,
} from "queries/operators";
import type { Operator, OperatorDtoInput, UpdateOperatorDtoInput } from "api/manager/operators";
import { formatDateTime } from "utils/dateUtils";
import { handleSaveCredential, handleTestCredential } from "layouts/gpsintegration/actions/credentialActions";
import { LoadingContext } from 'LoadingContext';
import { useAuth } from "AuthContext";
import { notifyGpsIntegrationRefresh } from "layouts/gpsintegration/gpsIntegrationEvents";

/**
 * Dialog/form state for an operator. Merges an API {@link Operator} (when editing)
 * with the loose fresh-add shape (`{ protocolTypeId, syncIntervalMinutes }`); all
 * fields are optional and the `name`/`protocolTypeId` requirement is enforced by
 * the dialog's validate() gate before save.
 */
export interface OperatorFormValues {
  operatorId?: string;
  name?: string;
  description?: string | null;
  phoneNumber?: string | null;
  emailAddress?: string | null;
  address?: string | null;
  contactName?: string | null;
  protocolTypeId?: number;
  syncIntervalMinutes?: number;
  protocolType?: string;
  enabled?: boolean;
  healthStatus?: string;
  lastSuccessfulSyncAt?: string | null;
  lastModified?: string;
}

/** Dialog/form state for a credential (all fields blank/undefined until typed). */
export interface CredentialFormValues {
  credentialId?: string;
  operatorId?: string;
  uri?: string;
  username?: string | null;
  password?: string | null;
  key?: string | null;
  key2?: string | null;
}

/** A column descriptor / rendered row for the vendored operators `Table`. */
export interface OperatorTableColumn { name: string; title?: string; align?: "left" | "right" | "center"; }
export type OperatorTableRow = Record<string, ReactNode>;
export interface OperatorTableData { columns: OperatorTableColumn[]; rows: OperatorTableRow[]; }

type BadgeColor = 'primary' | 'secondary' | 'info' | 'success' | 'warning' | 'error' | 'light' | 'dark';

function healthBadgeColor(status: string): BadgeColor {
  switch ((status || '').toUpperCase()) {
    case 'HEALTHY': return 'success';
    case 'DEGRADED': return 'warning';
    case 'OFFLINE': return 'error';
    case 'DISABLED': return 'secondary';
    default: return 'secondary';
  }
}

function useOperatorTableData(
  fetchData: boolean,
  handleEditClick: (operator: OperatorFormValues) => void,
  handleEditCredentialClick: (credential: CredentialFormValues) => void,
  handleDeleteClick: (operatorId: string) => void
) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [openCredential, setOpenCredential] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [testOpen, setTestOpen] = useState(false);
  const [testTitle, setTestTitle] = useState("");
  const [testMessage, setTestMessage] = useState("");
  const [syncingMap, setSyncingMap] = useState<Record<string, boolean>>({});
  const { setLoading } = useContext(LoadingContext);
  const { isAuthenticated } = useAuth();

  const operatorsQuery = useOperatorsByCurrentAccount({ enabled: !!fetchData && isAuthenticated });
  const operators = operatorsQuery.data ?? [];
  const createOperator = useCreateOperator();
  const updateOperator = useUpdateOperator();
  const deleteOperator = useDeleteOperator();
  const setOperatorEnabled = useSetOperatorEnabled();
  const triggerOperatorDeviceSync = useTriggerOperatorDeviceSync();

  // Keep the global spinner UX for the initial load / invalidation refetch.
  useEffect(() => {
    setLoading(operatorsQuery.isFetching);
  }, [operatorsQuery.isFetching, setLoading]);

  // Silent op: connectivity failures surface in the test dialog, not the
  // global toast, so call the api function directly and swallow the error.
  const testConnectivity = async (operatorId: string): Promise<boolean> => {
    try {
      return await pingOperator(operatorId);
    } catch (error) {
      if (process.env.NODE_ENV !== 'production') console.error(error);
      return false;
    }
  };

  const healthLabel = (status: string | null | undefined): string => {
    const key = (status || 'unknown').toLowerCase();
    return t(`gpsIntegration.health.${key}` as 'gpsIntegration.health.unknown', { defaultValue: status || t('gpsIntegration.health.unknown') });
  };

  const onSave = async (operator: OperatorFormValues) => {
    setLoading(true);
    try {
      // validate(['name','protocolTypeId']) gates this call, so the required
      // create/update input fields are present — assert them at the boundary.
      if (operator.operatorId) {
        await updateOperator.mutateAsync(
          { operatorId: operator.operatorId, ...operator } as { operatorId: string } & Omit<UpdateOperatorDtoInput, 'operatorId'>
        );
      } else {
        await createOperator.mutateAsync(operator as OperatorDtoInput);
      }
      setOpen(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open so the
      // user can retry without re-entering the values.
    } finally {
      setLoading(false);
    }
  };

  const onDelete = async (operatorId: string) => {
    setLoading(true);
    try {
      await deleteOperator.mutateAsync(operatorId);
      setConfirmOpen(false);
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  const onSaveCredential = async (credential: CredentialFormValues) => {
    setLoading(true);
    try {
      // validate(['uri']) gates this; operatorId is carried on the loaded form.
      await handleSaveCredential(
        credential as CredentialFormInput,
        createCredential,
        updateCredential);
      setOpenCredential(false);
      await operatorsQuery.refetch();
      notifyGpsIntegrationRefresh();
    } catch (error) {
      // Keep the credential dialog open on failure so the user can retry.
      notifyApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const handleOpen = (operator: Operator) => {
    handleEditClick(operator);
    setOpen(true);
  };

  const handleOpenCredential = async (operatorId: string) => {
    // Silent read: an operator without a stored credential opens an empty form.
    let credential: Credential | undefined;
    try {
      credential = await getCredentialByOperator(operatorId);
    } catch (error) {
      if (process.env.NODE_ENV !== 'production') console.error(error);
    }
    handleEditCredentialClick(credential || { operatorId });
    setOpenCredential(true);
  };

  const handleOpenDelete = (operatorId: string) => {
    handleDeleteClick(operatorId);
    setConfirmOpen(true);
  };

  const onTestCredential = async (operatorId: string) => {
    setLoading(true);
    try {
      let result = await handleTestCredential(
        operatorId,
        testConnectivity);
      setTestTitle(t('credential.connectivityTest'));
      setTestMessage(result ? t('credential.testSuccess') : t('credential.testError'));
      setTestOpen(true);
    } finally {
      setLoading(false);
    }
  };

  const handleToggleEnabled = async (operator: Operator) => {
    setLoading(true);
    try {
      // Mutation success invalidates the operators cache, so the list refetches
      // with the new enabled/health values (replaces the old manual re-read).
      await setOperatorEnabled.mutateAsync({ operatorId: operator.operatorId, enabled: !operator.enabled });
      notifyGpsIntegrationRefresh();
    } catch {
      // Failure is surfaced by the global toast; the cache is left unchanged.
    } finally {
      setLoading(false);
    }
  };

  const handleSync = async (operator: Operator, resetDeviceCatalog = false) => {
    setLoading(true);
    try {
      setSyncingMap(prev => ({ ...prev, [operator.operatorId]: true }));
      const completed = await triggerOperatorDeviceSync.mutateAsync({
        operatorId: operator.operatorId,
        resetDeviceCatalog,
      });
      if (!completed) {
        setTestTitle(t('gpsIntegration.actions.sync'));
        setTestMessage(t('gpsIntegration.actions.syncNotCompleted'));
        setTestOpen(true);
      }
      notifyGpsIntegrationRefresh();
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setSyncingMap(prev => ({ ...prev, [operator.operatorId]: false }));
      setLoading(false);
    }
  };

  const handleResetSync = async (operator: Operator) => {
    if (!window.confirm(t('gpsIntegration.actions.resetSyncConfirm'))) return;
    await handleSync(operator, true);
  };

  const buildTableData = (operators: Operator[]): OperatorTableData => ({
    columns: [
      { name: "name", title:t('operator.name'), align: "left" },
      { name: "description", title:t('operator.description'), align: "left" },
      { name: "contactname", title:t('operator.contactName'), align: "left" },
      { name: "protocoltype", title:t('operator.type'), align: "center" },
      { name: "syncInterval", title:t('gpsIntegration.columns.syncInterval'), align: "center" },
      { name: "enabled", title:t('generic.enabled'), align: "center" },
      { name: "health", title:t('gpsIntegration.columns.health'), align: "center" },
      { name: "lastSuccess", title:t('gpsIntegration.columns.lastSuccess'), align: "center" },
      { name: "modified", title:t('generic.modified'), align: "center" },
      { name: "action", title:t('generic.action'), align: "center" },
      { name: "testcredential", title:t('credential.testCredential'), align: "center" },
      { name: "gpsActions", title:t('gpsIntegration.sections.operators'), align: "center" },
      { name: "id" }
    ],
    rows: operators.map(operator => ({
      name: <NameDetail name={operator.name} detail={operator.emailAddress} />,
      description: <Description description={operator.description} />,
      contactname: <Description description={operator.contactName} />,
      protocoltype: (
        <ArgonBadge variant="gradient" badgeContent={operator.protocolType} color="success" size="xs" container />
      ),
      syncInterval: (
        <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
          {operator.syncIntervalMinutes ?? 30}
        </ArgonTypography>
      ),
      enabled: (
        <ArgonBadge
          variant="gradient"
          badgeContent={operator.enabled ? t('generic.yes') : t('generic.no')}
          color={operator.enabled ? 'success' : 'secondary'}
          size="xs"
          container
        />
      ),
      health: (
        <ArgonBadge
          variant="gradient"
          badgeContent={healthLabel(operator.healthStatus)}
          color={healthBadgeColor(operator.healthStatus)}
          size="xs"
          container
        />
      ),
      lastSuccess: (
        <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
          {formatDateTime(operator.lastSuccessfulSyncAt)}
        </ArgonTypography>
      ),
      modified: (
        <ArgonTypography variant="caption" color="secondary" fontWeight="medium">
          {formatDateTime(operator.lastModified)}
        </ArgonTypography>
      ),
      action: (
        <>
            <ArgonButton
                variant="text"
                color="dark"
                onClick={() => handleOpen(operator)}>
              <Icon>edit</Icon>&nbsp;{t('generic.edit')}
            </ArgonButton>
            <ArgonButton variant="text" color="dark" onClick={() => handleToggleEnabled(operator)}>
              <Icon>power_settings_new</Icon>&nbsp;{operator.enabled ? t('generic.disable') : t('generic.enable')}
            </ArgonButton>
            <ArgonButton
              variant="text"
              color="error"
              onClick={() => handleOpenDelete(operator.operatorId)}>
              <Icon>delete</Icon>&nbsp;{t('generic.delete')}
            </ArgonButton>
        </>
      ),
      testcredential: (
        <ArgonButton
            variant="text"
            color="dark"
            onClick={async() => await onTestCredential(operator.operatorId)}>
          <Icon>check</Icon>
        </ArgonButton>
      ),
      gpsActions: (
        <>
          <ArgonButton variant="text" color="info" onClick={() => handleSync(operator)} disabled={!!syncingMap[operator.operatorId]}>
            <Icon>sync</Icon>&nbsp;{t('gpsIntegration.actions.sync')}
          </ArgonButton>
          <ArgonButton variant="text" color="warning" onClick={async() => await handleOpenCredential(operator.operatorId)}>
            <Icon>vpn_key</Icon>&nbsp;{t('credential.title')}
          </ArgonButton>
          <ArgonButton variant="text" color="warning" onClick={() => handleResetSync(operator)} disabled={!!syncingMap[operator.operatorId]}>
            <Icon>restart_alt</Icon>&nbsp;{t('gpsIntegration.actions.resetSync')}
          </ArgonButton>
        </>
      ),
      id: operator.operatorId
    })),
  });

  const data = buildTableData(operators);

  return {
    data,
    open,
    openCredential,
    confirmOpen,
    testOpen,
    testTitle,
    testMessage,
    onSave,
    onDelete,
    onSaveCredential,
    setOpen,
    setOpenCredential,
    setConfirmOpen,
    setTestOpen
  };
}

export default useOperatorTableData;
