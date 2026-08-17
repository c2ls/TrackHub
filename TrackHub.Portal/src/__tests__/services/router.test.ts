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

import { executeGraphQL } from 'api/core/graphqlClient';
import { getDevicePositions } from 'api/router/router';
import { ApiError } from 'api/core/errors';

vi.mock('api/core/graphqlClient', () => ({
  executeGraphQL: vi.fn(),
}));

describe('router api', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  // The api layer no longer swallows failures into an empty list: a GraphQL
  // feature-restriction error propagates as the thrown ApiError, and the
  // query/toast layer owns the fallback UX.
  test('getDevicePositions rejects with the ApiError thrown by the client', async () => {
    const error = new ApiError('Feature disabled', { code: 'FEATURE_DISABLED' });
    vi.mocked(executeGraphQL).mockRejectedValue(error);

    await expect(getDevicePositions()).rejects.toBe(error);
  });
});
