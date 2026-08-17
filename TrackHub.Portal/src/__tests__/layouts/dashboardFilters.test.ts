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

import { filterPositions } from 'layouts/dashboard/utils/dashboard';
import type { Position } from 'api/router/router';

// Minimal Position fixtures — only the fields filterPositions actually reads
// (id, deviceName, transporterType, speed, deviceDateTime) are populated.
const positions = [
  { transporterId: 't-1', deviceName: 'ABC-123', transporterType: 'Truck', speed: 10, deviceDateTime: new Date().toISOString() },
  { transporterId: 't-2', deviceName: 'XYZ-789', transporterType: 'Van', speed: 0, deviceDateTime: new Date().toISOString() },
  { transporterId: 't-3', deviceName: 'JKL-456', transporterType: 'Truck', speed: 0, deviceDateTime: '2001-01-01T00:00:00Z' },
] as unknown as Position[];

describe('filterPositions defaults (live map regression)', () => {
  // Regression: default filter state must mean "no narrowing" — an
  // over-eager default would blank the live map after a refresh cycle.
  test('default Transporters filter state keeps every position', () => {
    const defaults = { transporterType: 'all', groupId: 'all', operatorId: 'all', status: 'all' };
    expect(filterPositions(positions, defaults)).toHaveLength(positions.length);
  });

  test('no options at all keeps every position', () => {
    expect(filterPositions(positions)).toHaveLength(positions.length);
    expect(filterPositions(positions, {})).toHaveLength(positions.length);
  });

  test('null membership sets and empty search do not narrow', () => {
    const result = filterPositions(positions, {
      transporterType: 'all',
      status: 'all',
      searchText: '',
      groupTransporterIds: null,
      operatorTransporterIds: null,
    });
    expect(result).toHaveLength(positions.length);
  });

  test('narrowing still works when explicitly requested', () => {
    expect(filterPositions(positions, { transporterType: 'Truck' })).toHaveLength(2);
    expect(filterPositions(positions, { groupTransporterIds: new Set(['t-2']) })).toHaveLength(1);
    expect(filterPositions(positions, { searchText: 'abc' })).toHaveLength(1);
    expect(filterPositions(positions, { status: 'offline', onlineInterval: 60 })).toHaveLength(1);
  });

  test('an explicitly empty membership set narrows to nothing', () => {
    expect(filterPositions(positions, { groupTransporterIds: new Set() })).toHaveLength(0);
  });
});
