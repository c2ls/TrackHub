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

import { render, screen, fireEvent } from '@testing-library/react';
import { TestWrapper } from '../components/testHelpers';
import DeviceFormDialog from 'layouts/gpsintegration/components/devices/DeviceDialog';
import type { GpsOperator } from 'api/manager/operators';

// i18next is not initialised in tests; echo the key so assertions read stable ids.
vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

const operator: GpsOperator = {
  operatorId: '33333333-3333-3333-3333-333333333333',
  name: 'Manual Provider',
  protocolType: 'TRACCAR',
  protocolTypeId: 9,
  enabled: true,
  lastDeviceSyncAt: null,
  lastPositionSyncAt: null,
  syncIntervalMinutes: 30,
};

function renderDialog(overrides: Partial<React.ComponentProps<typeof DeviceFormDialog>> = {}) {
  const props: React.ComponentProps<typeof DeviceFormDialog> = {
    open: true,
    setOpen: vi.fn(),
    handleSubmit: vi.fn(),
    values: { operatorId: operator.operatorId, deviceTypeId: 4, identifier: 0, autoAssign: true },
    handleChange: vi.fn(),
    errors: {},
    operators: [operator],
    ...overrides,
  };
  render(
    <TestWrapper>
      <DeviceFormDialog {...props} />
    </TestWrapper>
  );
  return props;
}

describe('DeviceFormDialog', () => {
  test('explains the name must be the plate for plate-keyed providers', () => {
    renderDialog();
    expect(screen.getByText('gpsIntegration.deviceForm.nameHelp')).toBeInTheDocument();
  });

  test('offers the auto-assign convenience', () => {
    renderDialog();
    expect(screen.getByText('gpsIntegration.deviceForm.autoAssign')).toBeInTheDocument();
  });

  test('surfaces field validation errors', () => {
    renderDialog({ errors: { name: 'validation.required' } });
    expect(screen.getByText('validation.required')).toBeInTheDocument();
  });

  test('saving invokes the submit handler', () => {
    const props = renderDialog();
    fireEvent.click(screen.getByText('generic.save'));
    expect(props.handleSubmit).toHaveBeenCalled();
  });
});
