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
 * The spec 11 §7.3 write hooks own cache invalidation. A POD or a delivery
 * outcome also changes the stop's deliveries and the trip timeline, so the
 * whole trip namespace has to go stale — otherwise the detail a dispatcher is
 * looking at silently keeps showing the pre-write state.
 */

import type { ReactNode } from 'react';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import * as api from 'api/tripManagement/trips';
import {
  tripKeys,
  useCreateDelivery,
  useDeleteDelivery,
  useRecordProofOfDelivery,
  useSetTransporterTollClass,
  useUpdateDeliveryOutcome,
} from 'queries/trips';

vi.mock('api/tripManagement/trips', () => ({
  createDelivery: vi.fn(),
  updateDelivery: vi.fn(),
  updateDeliveryOutcome: vi.fn(),
  deleteDelivery: vi.fn(),
  recordProofOfDelivery: vi.fn(),
  setTransporterTollClass: vi.fn(),
}));

const TRIP_ID = '11111111-1111-1111-1111-111111111111';
const STOP_ID = '22222222-2222-2222-2222-222222222222';
const DELIVERY_ID = '33333333-3333-3333-3333-333333333333';
const EVENT_ID = '44444444-4444-4444-4444-444444444444';

function harness() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  const invalidate = vi.spyOn(queryClient, 'invalidateQueries');
  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
  return { wrapper, invalidate };
}

beforeEach(() => vi.clearAllMocks());

test('useCreateDelivery invalidates the trip namespace', async () => {
  vi.mocked(api.createDelivery).mockResolvedValue({ deliveryId: DELIVERY_ID } as never);
  const { wrapper, invalidate } = harness();
  const { result } = renderHook(() => useCreateDelivery(), { wrapper });

  result.current.mutate({ tripStopId: STOP_ID, delivery: { clientName: 'Acme', sequenceIndex: 0 } });

  await waitFor(() => expect(result.current.isSuccess).toBe(true));
  expect(invalidate).toHaveBeenCalledWith({ queryKey: tripKeys.all });
});

test('useDeleteDelivery invalidates the trip namespace', async () => {
  vi.mocked(api.deleteDelivery).mockResolvedValue(DELIVERY_ID);
  const { wrapper, invalidate } = harness();
  const { result } = renderHook(() => useDeleteDelivery(), { wrapper });

  result.current.mutate(DELIVERY_ID);

  await waitFor(() => expect(result.current.isSuccess).toBe(true));
  expect(invalidate).toHaveBeenCalledWith({ queryKey: tripKeys.all });
});

test('useUpdateDeliveryOutcome forwards the caller-owned idempotency key', async () => {
  vi.mocked(api.updateDeliveryOutcome).mockResolvedValue(true);
  const { wrapper, invalidate } = harness();
  const { result } = renderHook(() => useUpdateDeliveryOutcome(), { wrapper });

  result.current.mutate({
    tripId: TRIP_ID,
    deliveryId: DELIVERY_ID,
    status: 'Delivered',
    clientEventId: EVENT_ID,
  });

  await waitFor(() => expect(result.current.isSuccess).toBe(true));
  expect(api.updateDeliveryOutcome).toHaveBeenCalledWith(
    TRIP_ID,
    DELIVERY_ID,
    'Delivered',
    null,
    EVENT_ID
  );
  expect(invalidate).toHaveBeenCalledWith({ queryKey: tripKeys.all });
});

test('useRecordProofOfDelivery invalidates the trip namespace', async () => {
  vi.mocked(api.recordProofOfDelivery).mockResolvedValue({ proofOfDeliveryId: 'pod' } as never);
  const { wrapper, invalidate } = harness();
  const { result } = renderHook(() => useRecordProofOfDelivery(), { wrapper });

  result.current.mutate({
    tripId: TRIP_ID,
    proofOfDelivery: {
      tripStopId: STOP_ID,
      receiverName: 'Ana',
      capturedAt: '2026-07-21T10:00:00.000Z',
      documentIds: [],
      clientEventId: EVENT_ID,
    },
  });

  await waitFor(() => expect(result.current.isSuccess).toBe(true));
  expect(invalidate).toHaveBeenCalledWith({ queryKey: tripKeys.all });
});

test('useSetTransporterTollClass invalidates both the trip and toll namespaces', async () => {
  // The mapping feeds Trip.TollVehicleClass defaulting at trip creation, so a
  // stale trip list and a stale toll panel are both wrong after it changes.
  vi.mocked(api.setTransporterTollClass).mockResolvedValue({
    transporterTollClassId: 'ttc',
  } as never);
  const { wrapper, invalidate } = harness();
  const { result } = renderHook(() => useSetTransporterTollClass(), { wrapper });

  result.current.mutate({ transporterTypeId: 2, tollVehicleClassCode: 'III' });

  await waitFor(() => expect(result.current.isSuccess).toBe(true));
  expect(api.setTransporterTollClass).toHaveBeenCalledWith(2, null, 'III');
  expect(invalidate).toHaveBeenCalledWith({ queryKey: tripKeys.all });
  expect(invalidate).toHaveBeenCalledWith({ queryKey: tripKeys.tolls });
});
