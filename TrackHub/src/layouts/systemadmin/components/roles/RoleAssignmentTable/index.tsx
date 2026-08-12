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

import { useState, useEffect, useMemo, useContext } from 'react';
import { useTranslation } from 'react-i18next';
import CheckboxTableDialog from 'controls/Dialogs/TableDialogs/CheckboxTableDialog';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import { useRoles, useRoleResources } from 'queries/roles';
import { useActions } from 'queries/actions';
import { useResources } from 'queries/resources';
import { createResourceActionRole, deleteResourceActionRole } from 'api/security/roles';
import { LoadingContext } from 'LoadingContext';
import { toCamelCase } from 'utils/stringUtils';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';

// A single resource-action assignment cell (nested under resourceId → actionId).
interface ActionAssignment { actionId: number; actionName: string; resourceId: number }
type AssignmentMap = Record<string, Record<string, ActionAssignment>>;
interface SelectOption { value: number; name: string; label: string }

interface RoleAssignmentTableProps {
  open: boolean;
}

function RoleAssignmentTable({ open }: RoleAssignmentTableProps) {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [role, setRole] = useState(0);

  const actionsQuery = useActions({ enabled: open });
  const resourcesQuery = useResources({ enabled: open });
  const rolesQuery = useRoles({ enabled: open });
  const roleResourcesQuery = useRoleResources(role);

  // Keep the global spinner UX while the lists load/refresh.
  useEffect(() => {
    setLoading(
      actionsQuery.isFetching ||
      resourcesQuery.isFetching ||
      rolesQuery.isFetching ||
      roleResourcesQuery.isFetching
    );
  }, [
    actionsQuery.isFetching,
    resourcesQuery.isFetching,
    rolesQuery.isFetching,
    roleResourcesQuery.isFetching,
    setLoading
  ]);

  const actions = useMemo<SelectOption[]>(
    () => (actionsQuery.data ?? []).map(action => ({
      value: action.actionId,
      name: action.actionName,
      label: t(`actions.${toCamelCase(action.actionName)}` as 'actions.read')
    })),
    [actionsQuery.data, t]
  );

  const resources = useMemo<SelectOption[]>(
    () => (resourcesQuery.data ?? []).map(resource => ({
      value: resource.resourceId,
      name: resource.resourceName,
      label: t(`resources.${toCamelCase(resource.resourceName)}` as 'resources.accounts')
    })),
    [resourcesQuery.data, t]
  );

  const roles = useMemo<SelectOption[]>(
    () => (rolesQuery.data ?? []).map(roleItem => ({
      value: roleItem.roleId,
      name: roleItem.name,
      label: t(`roles.${toCamelCase(roleItem.name)}` as 'roles.manager', { defaultValue: roleItem.name })
    })),
    [rolesQuery.data, t]
  );

  const data = useMemo<AssignmentMap>(() => {
    const roleResources = roleResourcesQuery.data;
    if (!roleResources) return {};
    return roleResources.resources.reduce<AssignmentMap>((map, resource) => {
      // `actions` is a nullable generated array — default to [] when absent.
      map[resource.resourceId] = (resource.actions ?? []).reduce<Record<string, ActionAssignment>>((actionMap, action) => {
        actionMap[action.actionId] = action;
        return actionMap;
      }, {});
      return map;
    }, {});
  }, [roleResourcesQuery.data]);

  const handleChange: FormChangeHandler = (event) => {
    setRole(Number(event.target.value));
  };

  const handleSubmit = async (resourceId: string, actionId: string, checked: boolean): Promise<boolean> => {
    setLoading(true);
    let result = false;
    try {
      if (checked) {
        const created = await createResourceActionRole(Number(resourceId), Number(actionId), role);
        result = created.roleId === role;
      } else {
        const deleted = await deleteResourceActionRole(Number(resourceId), Number(actionId), role);
        result = deleted === role;
      }
    } catch {
      // Silent (matches the old handleSilentError): no toast, treat as failure.
      result = false;
    } finally {
      setLoading(false);
    }
    return result;
  };

  return (
    <CheckboxTableDialog
      key="role"
      handleSave={handleSubmit}
      title={t('role.resources')}
      data={data}
      columns={actions}
      rows={resources}>
      <CustomSelect
        list={roles}
        name="name"
        id="roleId"
        label={t('role.singleTitle')}
        value={role}
        handleChange={handleChange}
        required
      />
    </CheckboxTableDialog>
  );
}

export default RoleAssignmentTable;
