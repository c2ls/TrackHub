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

import { fetchList } from 'utils/reportUtils';

describe('fetchList', () => {
  test('fetches and maps data', async () => {
    const mockFetch = vi.fn().mockResolvedValue([1, 2, 3]);
    const mockMap = vi.fn((x) => x * 2);

    const result = await fetchList(mockFetch, mockMap);
    expect(result).toEqual([2, 4, 6]);
    expect(mockFetch).toHaveBeenCalledTimes(1);
    expect(mockMap).toHaveBeenCalledTimes(3);
  });

  test('returns empty array when fetch returns empty', async () => {
    const mockFetch = vi.fn().mockResolvedValue([]);
    const mockMap = vi.fn();

    const result = await fetchList(mockFetch, mockMap);
    expect(result).toEqual([]);
    expect(mockMap).not.toHaveBeenCalled();
  });

  test('propagates fetch errors', async () => {
    const mockFetch = vi.fn().mockRejectedValue(new Error('Network error'));
    const mockMap = vi.fn();

    await expect(fetchList(mockFetch, mockMap)).rejects.toThrow('Network error');
  });

  test('maps objects with transformation', async () => {
    const data = [{ id: 1, name: 'A' }, { id: 2, name: 'B' }];
    const mockFetch = vi.fn().mockResolvedValue(data);
    const mockMap = (item: { id: number; name: string }) => ({ value: item.id, label: item.name });

    const result = await fetchList(mockFetch, mockMap);
    expect(result).toEqual([
      { value: 1, label: 'A' },
      { value: 2, label: 'B' },
    ]);
  });
});
