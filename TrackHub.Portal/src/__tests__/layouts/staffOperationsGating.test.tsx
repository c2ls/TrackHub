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

import { render, screen } from '@testing-library/react';
import { TestWrapper } from '../components/testHelpers';
import { PermissionsContext } from 'context/permissions';
import { FeaturesContext } from 'context/features';
import { PermissionActions, PermissionResources } from 'constants/permissions';
import ManageStaff from 'layouts/manageadmin/components/staff';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));
const idle = { data: undefined, isFetching: false };
vi.mock('queries/accounts', () => ({ useAccountByUser: () => idle }));
vi.mock('queries/staffIdentity', () => ({ useStaffCredentials: () => idle }));
vi.mock('queries/staff', () => ({
  useStaffMembersPage: () => idle,
  useCreateStaffMember: () => ({}),
  useUpdateStaffMember: () => ({}),
  useDeactivateStaffMember: () => ({}),
}));
vi.mock('layouts/manageadmin/components/staff/StaffQualifications', () => ({ default: () => null }));
vi.mock('layouts/manageadmin/components/staff/QualificationExpirations', () => ({ default: () => null }));
vi.mock('layouts/manageadmin/components/staff/StaffDialog', () => ({ default: () => null }));
vi.mock('layouts/manageadmin/components/staff/StaffDetailDialog', () => ({ default: () => null }));
vi.mock('layouts/manageadmin/components/staff/StaffAssignments', () => ({
  default: () => <span>staff-assignments</span>,
}));

const granting = (...grants: Array<[string, string]>) => ({
  actions: [],
  loaded: true,
  can: (resource: string, action: string) => grants.some(([r, a]) => r === resource && a === action),
});

const features = (workforce: boolean) => ({
  features: [],
  isFeatureEnabled: (key?: string | null) => workforce || key !== 'workforce',
  reload: async () => {},
});

function renderStaff(permissions: ReturnType<typeof granting>, workforce = true) {
  return render(
    <TestWrapper>
      <FeaturesContext.Provider value={features(workforce)}>
        <PermissionsContext.Provider value={permissions}>
          <ManageStaff />
        </PermissionsContext.Provider>
      </FeaturesContext.Provider>
    </TestWrapper>
  );
}

describe('staff assignments section', () => {
  test('is offered to a caller holding StaffOperations/Read', () => {
    renderStaff(granting([PermissionResources.StaffOperations, PermissionActions.Read]));
    expect(screen.getByText('staff-assignments')).toBeInTheDocument();
  });

  test('is hidden from a caller who only reads the staff catalogue', () => {
    renderStaff(granting(['Staff', PermissionActions.Read]));
    expect(screen.queryByText('staff-assignments')).not.toBeInTheDocument();
  });

  test('is hidden when the account has no workforce feature', () => {
    renderStaff(granting([PermissionResources.StaffOperations, PermissionActions.Read]), false);
    expect(screen.queryByText('staff-assignments')).not.toBeInTheDocument();
  });
});
