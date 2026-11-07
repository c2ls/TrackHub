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
 * The trip-scoped panels on the dispatch board seed their `useState` ONCE, at
 * mount. The board looked like it unmounted them between trips — they sit
 * inside a `{!detail ? … }` ternary — but TanStack serves an already-visited
 * trip's detail from cache synchronously, so `detail` never goes undefined on
 * the way back and the panels survive the switch holding the previous trip's
 * values. Assign then posted trip A's staff against trip B.
 *
 * The fix is `key={detail.trip.tripId}`, which is exactly what this test pins:
 * remounting on a trip change is load-bearing behaviour, not a React detail.
 */

import { render, screen, fireEvent } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { TestWrapper } from '../components/testHelpers';
import AssignmentPanel from 'layouts/tripmanager/components/AssignmentPanel';
import type { TripDetail } from 'api/tripManagement/trips';
import type { Staff } from 'api/manager/staff';
import type { TransporterLookup } from 'api/manager/transporters';

vi.mock('api/manager/staff', () => ({
  getStaffLookup: vi.fn().mockResolvedValue([]),
}));

vi.mock('api/manager/transporters', () => ({
  getTransportersByUser: vi.fn().mockResolvedValue({ items: [], totalCount: 0 }),
}));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

const TRIP_A = '11111111-1111-1111-1111-111111111111';
const TRIP_B = '22222222-2222-2222-2222-222222222222';
const STAFF_A = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
const STAFF_B = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb';
const TRANSPORTER = 'cccccccc-cccc-cccc-cccc-cccccccccccc';
const ACCOUNT = 'dddddddd-dddd-dddd-dddd-dddddddddddd';

const makeStaff = (staffId: string, name: string): Staff => ({
  staffId,
  accountId: ACCOUNT,
  name,
  phone: null,
  documentType: null,
  documentNumber: null,
  active: true,
  employeeCode: null,
  licenseNumber: null,
  licenseExpiresAt: null,
  defaultTransporterId: null,
  staffType: 'Driver',
  lastModified: '2026-07-21T08:00:00Z',
  version: 1,
});

const staff: Staff[] = [makeStaff(STAFF_A, 'Staff A'), makeStaff(STAFF_B, 'Staff B')];

const transporters: TransporterLookup[] = [
  { transporterId: TRANSPORTER, name: 'Fleet', transporterType: 'TRUCK', transporterTypeId: 1 },
];

/** A detail payload carrying only what AssignmentPanel actually reads. */
const detailFor = (tripId: string, staffId: string): TripDetail => ({
  trip: {
    tripId,
    accountId: ACCOUNT,
    code: tripId === TRIP_A ? 'TRIP-A' : 'TRIP-B',
    status: 'Created',
    transporterId: TRANSPORTER,
    staffId,
    routePlanId: null,
    serviceOrderId: null,
    externalReference: null,
    customerName: null,
    originName: 'Depot',
    originLatitude: 4.65,
    originLongitude: -74.05,
    originGeofenceId: null,
    originRadiusMeters: 150,
    plannedStartAt: '2026-07-21T08:00:00Z',
    plannedEndAt: null,
    actualStartAt: null,
    actualEndAt: null,
    armedAt: null,
    originArrivedAt: null,
    originDepartedAt: null,
    phase: 'Scheduled',
    phaseStopName: null,
    phaseStopActivity: null,
    phaseEtaAt: null,
    phaseDelayed: false,
    notes: null,
    lastPositionAt: null,
    lastLatitude: null,
    lastLongitude: null,
    actualDistanceMeters: 0,
    tollVehicleClass: null,
    deviationOpenedAt: null,
    cancellationReason: null,
    stopCount: 0,
    pendingStopCount: 0,
    lastModified: '2026-07-21T08:00:00Z',
  },
  stops: [],
  assignment: {
    tripAssignmentId: tripId,
    accountId: ACCOUNT,
    tripId,
    staffId,
    transporterId: TRANSPORTER,
    status: 'Assigned',
    assignedAt: '2026-07-21T08:00:00Z',
    acknowledgedAt: null,
    endedAt: null,
  },
  routePlan: null,
  proofsOfDelivery: [],
  shares: [],
});

test('switching trips does not carry the previous trip’s staff into Assign', () => {
  const onAssign = vi.fn();
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  // Rendered exactly as the board renders it, key included.
  const panel = (detail: TripDetail) => (
    <QueryClientProvider client={queryClient}>
      <TestWrapper>
        <AssignmentPanel
          key={detail.trip.tripId}
          detail={detail}
          staff={staff}
          transporters={transporters}
          onAssign={onAssign}
          assigning={false}
          editable
        />
      </TestWrapper>
    </QueryClientProvider>
  );

  const view = render(panel(detailFor(TRIP_A, STAFF_A)));
  // Trip B is a DIFFERENT trip with a different staff; without the key the
  // panel would keep A's selection and Assign would write it onto B.
  view.rerender(panel(detailFor(TRIP_B, STAFF_B)));

  fireEvent.click(screen.getByText('trips.assignment.assign'));

  expect(onAssign).toHaveBeenCalledTimes(1);
  expect(onAssign).toHaveBeenCalledWith(STAFF_B, TRANSPORTER);
});
