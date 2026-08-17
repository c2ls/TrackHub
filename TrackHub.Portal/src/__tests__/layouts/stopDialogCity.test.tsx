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

/**
 * A trip stop's `city` is a separate disclosure level from its `address`: the
 * anonymous customer snapshot may carry the locality but never the full street
 * label. The city therefore has to come from the reverse geocoder's OWN city
 * field — deriving it from the address string would be the same leak in a new
 * form (spec 11 §7.8).
 */

import { render, waitFor } from '@testing-library/react';
import { TestWrapper } from '../components/testHelpers';
import StopDialog from 'layouts/tripmanager/components/StopDialog';
import { reverseGeocode } from 'api/router/router';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));
vi.mock('api/router/router', () => ({ reverseGeocode: vi.fn() }));

const props = (overrides: Partial<React.ComponentProps<typeof StopDialog>> = {}) => ({
  open: true,
  setOpen: vi.fn(),
  handleSubmit: vi.fn(),
  values: { latitude: 4.65, longitude: -74.05 },
  handleChange: vi.fn(),
  errors: {},
  pois: [],
  geofences: [],
  onPlaceOnMap: vi.fn(),
  ...overrides,
});

beforeEach(() => vi.clearAllMocks());

test('fills city from the geocoder city field, not from the address string', async () => {
  vi.mocked(reverseGeocode).mockResolvedValue({
    address: 'Cra 7 #71-52, Bogotá',
    city: 'Bogotá',
  } as never);
  const dialogProps = props();

  render(
    <TestWrapper>
      <StopDialog {...dialogProps} />
    </TestWrapper>
  );

  await waitFor(() =>
    expect(dialogProps.handleChange).toHaveBeenCalledWith({
      target: { name: 'city', value: 'Bogotá' },
    })
  );
  expect(dialogProps.handleChange).toHaveBeenCalledWith({
    target: { name: 'address', value: 'Cra 7 #71-52, Bogotá' },
  });
});

test('re-resolves the city for a stop that already has an address but no city', async () => {
  // The stop's city is now selected and seeded from the stored value, so this
  // is the remaining real case: a stop created before the locality was captured
  // has an address but no city. The resolve has to run anyway, or editing it
  // for any other reason saves the gap back permanently.
  vi.mocked(reverseGeocode).mockResolvedValue({
    address: 'Cra 7 #71-52, Bogotá',
    city: 'Bogotá',
  } as never);
  const dialogProps = props({
    values: { latitude: 4.65, longitude: -74.05, address: 'Cra 7 #71-52, Bogotá', city: '' },
  });

  render(
    <TestWrapper>
      <StopDialog {...dialogProps} />
    </TestWrapper>
  );

  await waitFor(() =>
    expect(dialogProps.handleChange).toHaveBeenCalledWith({
      target: { name: 'city', value: 'Bogotá' },
    })
  );
  // The address the dispatcher already has is never overwritten.
  expect(dialogProps.handleChange).not.toHaveBeenCalledWith({
    target: { name: 'address', value: 'Cra 7 #71-52, Bogotá' },
  });
});

test('does not call the geocoder when both fields are already filled', async () => {
  const dialogProps = props({
    values: { latitude: 4.65, longitude: -74.05, address: 'Cra 7 #71-52', city: 'Bogotá' },
  });

  render(
    <TestWrapper>
      <StopDialog {...dialogProps} />
    </TestWrapper>
  );

  await waitFor(() => expect(reverseGeocode).not.toHaveBeenCalled());
});

test('a geocoder outage never blocks placing a stop', async () => {
  vi.mocked(reverseGeocode).mockRejectedValue(new Error('geocoder down'));
  const dialogProps = props();

  render(
    <TestWrapper>
      <StopDialog {...dialogProps} />
    </TestWrapper>
  );

  await waitFor(() => expect(reverseGeocode).toHaveBeenCalled());
  expect(dialogProps.handleChange).not.toHaveBeenCalledWith(
    expect.objectContaining({ target: expect.objectContaining({ name: 'city' }) })
  );
});
