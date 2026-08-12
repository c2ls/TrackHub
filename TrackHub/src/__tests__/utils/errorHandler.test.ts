/**
 * Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

import { handleSilentError } from 'utils/errorHandler';

describe('handleSilentError', () => {
  let consoleSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
  });

  afterEach(() => {
    consoleSpy.mockRestore();
  });

  test('logs error to console in non-production', () => {
    const error = new Error('silent failure');
    handleSilentError(error);
    expect(consoleSpy).toHaveBeenCalledWith(error);
  });

  test('logs string to console in non-production', () => {
    handleSilentError('something broke');
    expect(consoleSpy).toHaveBeenCalledWith('something broke');
  });

  test('logs null to console in non-production', () => {
    handleSilentError(null);
    expect(consoleSpy).toHaveBeenCalledWith(null);
  });
});
