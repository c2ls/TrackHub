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

import type { ReactNode } from 'react';
import { render, screen } from '@testing-library/react';
import { TestWrapper } from '../components/testHelpers';
import { PermissionsContext } from 'context/permissions';
import { FeaturesContext } from 'context/features';
import { PermissionActions, PermissionResources } from 'constants/permissions';
import Dashboard from 'layouts/dashboard';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));
vi.mock('AuthContext', () => ({ useAuth: () => ({ isAuthenticated: false }) }));
vi.mock('api/manager/settings', () => ({ getAccountSettings: vi.fn() }));
vi.mock('queries/geofences', () => ({ useAllGeofences: () => ({ data: [], isFetching: false }) }));
vi.mock('layouts/dashboard/components/Transporters', () => ({ default: () => null }));
vi.mock('layouts/dashboard/components/Positions', () => ({ default: () => null }));
vi.mock('layouts/manageadmin/components/staff/StaffAssignments', () => ({ default: () => null }));
vi.mock('controls/Navbars/DashboardTabbar', () => ({
  default: ({ tabs, children }: { tabs: string[]; children: ReactNode }) => (
    <div>
      {tabs.map((tab) => (
        <span key={tab}>{tab}</span>
      ))}
      {children}
    </div>
  ),
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

function renderDashboard(permissions: ReturnType<typeof granting>, workforce = true) {
  return render(
    <TestWrapper>
      <FeaturesContext.Provider value={features(workforce)}>
        <PermissionsContext.Provider value={permissions}>
          <Dashboard />
        </PermissionsContext.Provider>
      </FeaturesContext.Provider>
    </TestWrapper>
  );
}

describe('dashboard staff tab', () => {
  test('is offered to a dispatcher holding StaffOperations/Read', () => {
    renderDashboard(granting([PermissionResources.StaffOperations, PermissionActions.Read]));
    expect(screen.getByText('dashboard.staffTitle')).toBeInTheDocument();
  });

  test('is hidden from a caller who only reads the staff catalogue', () => {
    renderDashboard(granting(['Staff', PermissionActions.Read]));
    expect(screen.queryByText('dashboard.staffTitle')).not.toBeInTheDocument();
  });

  test('is hidden when the account has no workforce feature', () => {
    renderDashboard(granting([PermissionResources.StaffOperations, PermissionActions.Read]), false);
    expect(screen.queryByText('dashboard.staffTitle')).not.toBeInTheDocument();
  });
});
