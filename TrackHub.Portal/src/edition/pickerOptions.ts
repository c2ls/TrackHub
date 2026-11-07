/**
* Copyright (c) 2026 Sergio Hernandez. All rights reserved.
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

import { useUsersByAccount } from 'queries/users';
import { useTransportersByAccount, useTransportersByUser } from 'queries/transporters';
import { useUnassignedSynchronizedDevices } from 'queries/devices';
import { useGroups } from 'queries/groups';
import { usePointsOfInterestByAccount } from 'queries/pointsOfInterest';
import { useAccounts } from 'queries/accounts';
import { useStaffOptions, useStaffMembersPage } from 'queries/staff';
import { MAX_PAGE_SIZE } from 'api/core/paging';
import type { SearchOptionsHook } from 'edition/SearchSelect';

export const PICKER_PAGE = 20;

const pageParams = (search: string, take = PICKER_PAGE) => ({ skip: 0, take, search: search || null });

export const excludingOptions =
  (useOptions: SearchOptionsHook, excluded: ReadonlySet<string>): SearchOptionsHook =>
  (search) => {
    const { options, loading } = useOptions(search, Math.min(PICKER_PAGE + excluded.size, MAX_PAGE_SIZE));
    return { options: options.filter((option) => !excluded.has(option.value)).slice(0, PICKER_PAGE), loading };
  };

export const useAccountUserOptions: SearchOptionsHook = (search, take) => {
  const { data, isFetching } = useUsersByAccount(pageParams(search, take));
  return {
    options: (data?.items ?? []).map((user) => ({
      value: user.userId,
      label: `${user.firstName} ${user.lastName} (${user.username})`,
    })),
    loading: isFetching,
  };
};

export const useAccountTransporterOptions: SearchOptionsHook = (search, take) => {
  const { data, isFetching } = useTransportersByAccount(pageParams(search, take));
  return {
    options: (data?.items ?? []).map((transporter) => ({ value: transporter.transporterId, label: transporter.name })),
    loading: isFetching,
  };
};

export const useUserTransporterOptions: SearchOptionsHook = (search, take) => {
  const { data, isFetching } = useTransportersByUser(pageParams(search, take));
  return {
    options: (data?.items ?? []).map((transporter) => ({ value: transporter.transporterId, label: transporter.name })),
    loading: isFetching,
  };
};

export const unassignedDeviceOptions =
  (accountId: string | undefined): SearchOptionsHook =>
  (search, take) => {
    const { data, isFetching } = useUnassignedSynchronizedDevices(accountId, pageParams(search, take));
    return {
      options: (data?.items ?? []).map((device) => ({
        value: device.deviceId,
        label: device.name || device.providerDisplayName || device.serial || String(device.identifier),
      })),
      loading: isFetching,
    };
  };

export const useGroupOptions: SearchOptionsHook = (search, take) => {
  const { data, isFetching } = useGroups(pageParams(search, take));
  return {
    options: (data?.items ?? []).map((group) => ({ value: String(group.groupId), label: group.name })),
    loading: isFetching,
  };
};

export const usePointOfInterestOptions: SearchOptionsHook = (search, take) => {
  const { data, isFetching } = usePointsOfInterestByAccount({ ...pageParams(search, take), active: true });
  return {
    options: (data?.items ?? []).map((poi) => ({ value: poi.pointOfInterestId, label: poi.name })),
    loading: isFetching,
  };
};

export const accountStaffOptions =
  (accountId: string | undefined): SearchOptionsHook =>
  (search, take) => {
    const { data, isFetching } = useStaffMembersPage(accountId, pageParams(search, take));
    return {
      options: (data?.items ?? []).map((staff) => ({ value: staff.staffId, label: staff.name })),
      loading: isFetching,
    };
  };

export const useStaffOperationsOptions: SearchOptionsHook = (search, take) => {
  const { data, isFetching } = useStaffOptions(pageParams(search, take));
  return {
    options: (data?.items ?? []).map((staff) => ({ value: staff.staffId, label: staff.name })),
    loading: isFetching,
  };
};

export const useAccountOptions: SearchOptionsHook = (search, take) => {
  const { data, isFetching } = useAccounts(pageParams(search, take));
  return {
    options: (data?.items ?? []).map((account) => ({ value: account.accountId, label: account.name })),
    loading: isFetching,
  };
};
