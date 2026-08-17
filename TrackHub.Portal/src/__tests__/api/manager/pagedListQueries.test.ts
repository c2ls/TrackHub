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
 * Finding AD-03: the Manager list queries became paginated (`{ items,
 * totalCount }` behind a non-null input) and a set of unpaged lookups was added
 * for the pickers. The assertions here are about the CONTRACT — that every
 * paged reader forwards skip/take/search and returns the page whole, that the
 * `getAll*` drains really exhaust the server, and above all that a PICKER never
 * binds a single page (the truncation this finding exists to kill).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import { MAX_PAGE_SIZE } from 'api/core/paging';
import {
  getTransportersByAccount,
  getTransporterLookupByAccount,
  getTransporterLookupByUser,
  getAllTransportersByGroup,
  getAllTransporterDeviceAssignmentsByAccount,
} from 'api/manager/transporters';
import { getGroups, getGroupLookup, getAllUsersByGroup } from 'api/manager/groups';
import {
  getDevicesByAccount,
  getDeviceLookup,
  getSynchronizedDevices,
  getAllUnassignedSynchronizedDevices,
} from 'api/manager/devices';
import { getAccounts, getAllAccounts } from 'api/manager/accounts';
import { getPointOfInterestLookup } from 'api/manager/pointsOfInterest';
import { getUsersByAccount, getUserLookupByAccount } from 'api/security/users';

vi.mock('api/core/graphqlClient', () => ({ executeGraphQL: vi.fn() }));

const mockExecute = vi.mocked(executeGraphQL);
const ACCOUNT_ID = '11111111-1111-1111-1111-111111111111';

/** The variables the mocked client saw on call `index`. */
const varsOf = (index = 0) => mockExecute.mock.calls[index][2] as Record<string, unknown>;

beforeEach(() => {
  mockExecute.mockReset();
});

describe('paged list readers', () => {
  test('transportersByAccount forwards skip/take/search and returns the page whole', async () => {
    mockExecute.mockResolvedValue({
      transportersByAccount: { items: [{ transporterId: 't1' }], totalCount: 42 },
    } as never);

    const page = await getTransportersByAccount({ skip: 20, take: 10, search: 'truck' });

    expect(varsOf()).toEqual({ skip: 20, take: 10, search: 'truck' });
    expect(page.totalCount).toBe(42);
    expect(page.items).toHaveLength(1);
  });

  test('an omitted filter travels as an explicit null, never undefined', async () => {
    mockExecute.mockResolvedValue({ groupsByAccount: { items: [], totalCount: 0 } } as never);

    await getGroups();

    expect(varsOf()).toEqual({ skip: null, take: null, search: null });
  });

  test('devicesByAccount and accounts read the same page shape', async () => {
    mockExecute.mockResolvedValueOnce({
      devicesByAccount: { items: [{ deviceId: 'd1' }], totalCount: 7 },
    } as never);
    mockExecute.mockResolvedValueOnce({
      accounts: { items: [{ accountId: 'a1' }], totalCount: 3 },
    } as never);

    expect((await getDevicesByAccount({ take: 10 })).totalCount).toBe(7);
    expect((await getAccounts({ take: 10 })).totalCount).toBe(3);
  });

  test('security usersByAccount reads the paged envelope and forwards search', async () => {
    mockExecute.mockResolvedValue({
      usersByAccount: { items: [{ userId: 'u1' }], totalCount: 31 },
    } as never);

    const page = await getUsersByAccount({ skip: 10, take: 10, search: 'ann' });

    expect(mockExecute.mock.calls[0][0]).toBe('security');
    expect(varsOf()).toEqual({ skip: 10, take: 10, search: 'ann' });
    expect(page.totalCount).toBe(31);
    expect(page.items).toHaveLength(1);
  });

  test('the synchronized-device filters travel as query arguments, not post-filters', async () => {
    mockExecute.mockResolvedValue({
      synchronizedDevices: { items: [], totalCount: 0 },
    } as never);

    await getSynchronizedDevices(ACCOUNT_ID, {
      skip: 0,
      take: 10,
      detectedStatus: null,
      operatorId: null,
      unassignedOnly: true,
      recentOnly: true,
    });

    // The recently-added and unassigned toggles are SERVER arguments now — the
    // whole point of restoring them honestly on a server-paged list.
    expect(varsOf()).toMatchObject({ unassignedOnly: true, recentOnly: true });
  });
});

describe('picker lookups', () => {
  test('the two transporter lookups address DIFFERENT fields', async () => {
    mockExecute.mockResolvedValueOnce({ transporterLookupByAccount: [{ transporterId: 't1' }] } as never);
    mockExecute.mockResolvedValueOnce({ transporterLookupByUser: [{ transporterId: 't2' }] } as never);

    // Collapsing these would let an admin picker show units the user may not
    // track, or hide account units from an admin screen.
    expect(await getTransporterLookupByAccount()).toEqual([{ transporterId: 't1' }]);
    expect(await getTransporterLookupByUser()).toEqual([{ transporterId: 't2' }]);
    expect(mockExecute.mock.calls[0][1]).not.toBe(mockExecute.mock.calls[1][1]);
  });

  test('lookups take no paging arguments at all', async () => {
    mockExecute.mockResolvedValue({ groupLookup: [] } as never);

    await getGroupLookup();

    expect(mockExecute.mock.calls[0][2]).toBeUndefined();
  });

  test('the security user lookup is unpaged', async () => {
    mockExecute.mockResolvedValue({ userLookupByAccount: [{ userId: 'u1' }] } as never);

    await getUserLookupByAccount();

    expect(mockExecute.mock.calls[0][0]).toBe('security');
    expect(mockExecute.mock.calls[0][2]).toBeUndefined();
  });

  // The lookups exist so a picker fetches a narrow projection instead of draining
  // full pages. These assert each widened lookup carries EXACTLY the columns its
  // consumer renders — the drain helpers were deleted, so a missing column would
  // strand the consumer with no fallback.
  test('the POI lookup carries the map projection and stays unpaged', async () => {
    mockExecute.mockResolvedValue({
      pointOfInterestLookup: [
        {
          pointOfInterestId: 'p1',
          name: 'Depot',
          latitude: 1,
          longitude: 2,
          color: 3,
          type: 1,
          description: 'd',
          address: 'a',
          active: true,
        },
      ],
    } as never);

    const rows = await getPointOfInterestLookup();

    expect(mockExecute.mock.calls[0][2]).toBeUndefined();
    // The overlay reads every one of these; the dashboard used to drain full
    // pages precisely because the lookup lacked them.
    expect(rows[0]).toMatchObject({ color: 3, type: 1, description: 'd', address: 'a', active: true });
  });

  test('the device lookup carries operatorId for the operator join', async () => {
    mockExecute.mockResolvedValue({
      deviceLookup: [{ deviceId: 'd1', name: 'Unit', operatorId: 'op1' }],
    } as never);

    const rows = await getDeviceLookup();

    expect(mockExecute.mock.calls[0][2]).toBeUndefined();
    expect(rows[0].operatorId).toBe('op1');
  });

  test('the transporter lookups carry the type the toll-class dialog derives', async () => {
    mockExecute.mockResolvedValue({
      transporterLookupByUser: [
        { transporterId: 't1', name: 'Rig', transporterType: 'TRUCK', transporterTypeId: 2 },
      ],
    } as never);

    const rows = await getTransporterLookupByUser();

    expect(rows[0]).toMatchObject({ transporterType: 'TRUCK', transporterTypeId: 2 });
  });
});

describe('exhaustive drains', () => {
  /** Serves `total` rows from `field`, honouring skip/take like the backends. */
  const pagedServer = (field: string, total: number) => {
    const rows = Array.from({ length: total }, (_, index) => ({ id: `${field}-${index}` }));
    mockExecute.mockImplementation(async (_backend, _doc, variables) => {
      const { skip = 0, take = MAX_PAGE_SIZE } = (variables ?? {}) as {
        skip?: number;
        take?: number;
      };
      return { [field]: { items: rows.slice(skip, skip + take), totalCount: total } } as never;
    });
    return rows;
  };

  test('a group membership drain returns rows past the first page', async () => {
    pagedServer('usersByGroup', 620);

    const users = await getAllUsersByGroup(3);

    // The exact set-difference bug: member 501 must not be missing, or the
    // allocator offers an already-assigned user again.
    expect(users).toHaveLength(620);
    expect(mockExecute).toHaveBeenCalledTimes(2);
  });

  test('the group transporter drain exhausts every page', async () => {
    pagedServer('transportersByGroup', 501);
    expect(await getAllTransportersByGroup(9)).toHaveLength(501);
  });

  test('the unassigned-device drain exhausts every page', async () => {
    pagedServer('unassignedSynchronizedDevices', 501);
    expect(await getAllUnassignedSynchronizedDevices(ACCOUNT_ID)).toHaveLength(501);
  });

  test('the assignment drain keeps activeOnly on every round trip', async () => {
    pagedServer('transporterDeviceAssignmentsByAccount', 700);

    const assignments = await getAllTransporterDeviceAssignmentsByAccount(ACCOUNT_ID, true);

    expect(assignments).toHaveLength(700);
    expect(mockExecute.mock.calls.every((call) => (call[2] as { activeOnly: boolean }).activeOnly)).toBe(true);
  });

  test('the account drain exhausts every page', async () => {
    pagedServer('accounts', 501);
    expect(await getAllAccounts()).toHaveLength(501);
  });

  test('a drain of a single short page costs exactly one round trip', async () => {
    pagedServer('transportersByGroup', 12);

    await getAllTransportersByGroup(1);

    expect(mockExecute).toHaveBeenCalledTimes(1);
  });
});
