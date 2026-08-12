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

import axios from 'axios';
import { executeGraphQL } from 'api/core/graphqlClient';
import { ApiError } from 'api/core/errors';
import { tokenStore } from 'api/core/tokenStore';

// Rewritten from the former services/apiService GraphQL suite: the partial-
// success rule now lives in graphqlClient.hasUsableData, exercised here through
// executeGraphQL. A response is a failure ONLY when it carries no usable data.
vi.mock('axios');
vi.mock('api/core/tokenStore', () => ({
  tokenStore: { acquireValidAccessToken: vi.fn() },
}));
// endpoints.ts resolves REACT_APP_* URLs at import time; provide a static map so
// the module graph loads without the runtime env the dev/build pipeline injects.
vi.mock('api/core/endpoints', () => ({
  GRAPHQL_ENDPOINTS: {
    manager: 'https://manager/graphql',
    security: 'https://security/graphql',
    geofencing: 'https://geofencing/graphql',
    router: 'https://router/graphql',
    telemetry: 'https://telemetry/graphql',
  },
}));

interface PositionsData {
  devicePositionsByUser: Array<{ transporterId: string; deviceName?: string }> | null;
}

const QUERY = '{ devicePositionsByUser { transporterId } }';
const run = () => executeGraphQL<PositionsData, Record<string, never>>('router', QUERY);

describe('executeGraphQL partial-success handling', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(tokenStore.acquireValidAccessToken).mockResolvedValue('access-token');
  });

  test('returns the data payload for a clean success', async () => {
    const payload = { data: { devicePositionsByUser: [] } };
    vi.mocked(axios.post).mockResolvedValue({ data: payload });

    const data = await run();

    expect(data).toEqual(payload.data);
  });

  // Regression: the router returns cached positions alongside provider-read
  // errors (partial GraphQL success). Treating that as a failure blanked the
  // live map on every refresh cycle.
  test('returns partial payloads where errors accompany usable data', async () => {
    const payload = {
      errors: [{ message: 'Provider read failed' }],
      data: { devicePositionsByUser: [{ transporterId: 't-1', deviceName: 'Unit 1' }] },
    };
    vi.mocked(axios.post).mockResolvedValue({ data: payload });

    const data = await run();

    expect(data.devicePositionsByUser).toHaveLength(1);
  });

  test('throws when every data field is null despite a 200 response', async () => {
    const payload = {
      errors: [{ message: 'Total failure' }],
      data: { devicePositionsByUser: null },
    };
    vi.mocked(axios.post).mockResolvedValue({ data: payload });

    await expect(run()).rejects.toBeInstanceOf(ApiError);
  });

  test('maps a well-known error code returned with a non-2xx status', async () => {
    const payload = {
      errors: [{ message: 'Forbidden', extensions: { code: 'FEATURE_DISABLED' } }],
      data: null,
    };
    vi.mocked(axios.post).mockRejectedValue({ response: { status: 403, data: payload }, message: 'Request failed' });

    await expect(run()).rejects.toMatchObject({ code: 'FEATURE_DISABLED' });
  });

  test('wraps a transport failure without a GraphQL payload in an ApiError', async () => {
    vi.mocked(axios.post).mockRejectedValue({ message: 'Network Error' });

    await expect(run()).rejects.toBeInstanceOf(ApiError);
  });
});
