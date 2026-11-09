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

import { MAX_PAGE_SIZE } from 'api/core/paging';
import type { SearchOption } from 'edition/SearchSelect';
import { accountStaffOptions, excludingOptions, PICKER_PAGE, usePointOfInterestOptions } from 'edition/pickerOptions';
import { useStaffMembersPage } from 'queries/staff';

const mockPois = vi.fn();

vi.mock('queries/users', () => ({ useUsersByAccount: vi.fn() }));
vi.mock('queries/transporters', () => ({ useTransportersByAccount: vi.fn(), useTransportersByUser: vi.fn() }));
vi.mock('queries/devices', () => ({ useUnassignedSynchronizedDevices: vi.fn() }));
vi.mock('queries/groups', () => ({ useGroups: vi.fn() }));
vi.mock('queries/accounts', () => ({ useAccounts: vi.fn() }));
vi.mock('queries/staff', () => ({ useStaffOptions: vi.fn(), useStaffMembersPage: vi.fn() }));
vi.mock('queries/pointsOfInterest', () => ({
  usePointsOfInterestByAccount: (params: unknown) => mockPois(params),
}));

const rows = (count: number): SearchOption[] =>
  Array.from({ length: count }, (_, index) => ({ value: `id-${index}`, label: `Row ${index}` }));

const serverOf = (all: SearchOption[]) =>
  vi.fn((_search: string, take?: number) => ({ options: all.slice(0, take ?? PICKER_PAGE), loading: false }));

describe('excludingOptions', () => {
  test('widens the page by the excluded count so held rows cannot hide eligible ones', () => {
    const server = serverOf(rows(60));
    const held = new Set(rows(PICKER_PAGE).map((row) => row.value));

    const { options } = excludingOptions(server, held)('');

    expect(server).toHaveBeenCalledWith('', PICKER_PAGE + held.size);
    expect(options).toHaveLength(PICKER_PAGE);
    expect(options.some((option) => held.has(option.value))).toBe(false);
  });

  test('never asks past the server maximum', () => {
    const server = serverOf(rows(10));
    const held = new Set(rows(MAX_PAGE_SIZE).map((row) => row.value));

    excludingOptions(server, held)('rig');

    expect(server).toHaveBeenCalledWith('rig', MAX_PAGE_SIZE);
  });
});

describe('usePointOfInterestOptions', () => {
  test('asks the server for active points only, one picker page at a time', () => {
    const items = [{ pointOfInterestId: 'p-1', name: 'Depot', active: true }];
    mockPois.mockReturnValue({ data: { items, totalCount: 1 }, isFetching: false });

    const { options } = usePointOfInterestOptions('depot');

    expect(mockPois).toHaveBeenCalledWith(expect.objectContaining({ search: 'depot', take: PICKER_PAGE, active: true }));
    expect(options).toEqual([{ value: 'p-1', label: 'Depot' }]);
  });
});

describe('accountStaffOptions', () => {
  test('can leave inactive staff out of the picker', () => {
    vi.mocked(useStaffMembersPage).mockReturnValue({
      data: { items: [{ staffId: 'a', name: 'Ana', active: true }, { staffId: 'b', name: 'Beto', active: false }], totalCount: 2 },
      isFetching: false,
    } as unknown as ReturnType<typeof useStaffMembersPage>);

    expect(accountStaffOptions('acc')('').options.map((option) => option.value)).toEqual(['a', 'b']);
    expect(accountStaffOptions('acc', true)('').options.map((option) => option.value)).toEqual(['a']);
  });
});
