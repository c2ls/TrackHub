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

import { fetchAllPages, MAX_PAGE_SIZE, MAX_FETCH_ALL_ITEMS } from 'api/core/paging';

/** A fake server holding `total` rows and clamping `take` exactly as the backends do. */
const server = (total: number) => {
  const calls: Array<{ skip: number; take: number }> = [];
  const rows = Array.from({ length: total }, (_, index) => index);
  const fetchPage = async (skip: number, take: number) => {
    calls.push({ skip, take });
    const clamped = Math.min(Math.max(take, 1), MAX_PAGE_SIZE);
    return rows.slice(skip, skip + clamped);
  };
  return { calls, fetchPage };
};

describe('fetchAllPages', () => {
  test('returns every row past the first page boundary', async () => {
    // The exact regression: 60 drivers behind a 50-row default page.
    const { fetchPage } = server(60);
    const items = await fetchAllPages(fetchPage, { pageSize: 50 });
    expect(items).toHaveLength(60);
    expect(items[59]).toBe(59);
  });

  test('exhausts several full pages', async () => {
    const { calls, fetchPage } = server(1250);
    const items = await fetchAllPages(fetchPage);
    expect(items).toHaveLength(1250);
    expect(calls.map((call) => call.skip)).toEqual([0, 500, 1000]);
    expect(calls.every((call) => call.take === MAX_PAGE_SIZE)).toBe(true);
  });

  test('stops on a short page without an extra round trip', async () => {
    const { calls, fetchPage } = server(10);
    const items = await fetchAllPages(fetchPage, { pageSize: 50 });
    expect(items).toHaveLength(10);
    expect(calls).toHaveLength(1);
  });

  test('stops on an exact multiple of the page size after one empty page', async () => {
    const { calls, fetchPage } = server(100);
    const items = await fetchAllPages(fetchPage, { pageSize: 50 });
    expect(items).toHaveLength(100);
    expect(calls).toHaveLength(3);
  });

  test('returns nothing when the first page is empty', async () => {
    const { calls, fetchPage } = server(0);
    expect(await fetchAllPages(fetchPage)).toEqual([]);
    expect(calls).toHaveLength(1);
  });

  test('never asks for more than the server ceiling', async () => {
    const { calls, fetchPage } = server(10);
    await fetchAllPages(fetchPage, { pageSize: 5000 });
    expect(calls[0].take).toBe(MAX_PAGE_SIZE);
  });

  test('caps at maxItems so an unbounded dataset cannot spin forever', async () => {
    const { fetchPage } = server(10_000);
    const items = await fetchAllPages(fetchPage, { maxItems: 1000 });
    expect(items).toHaveLength(1000);
  });

  test('default cap is the shared MAX_FETCH_ALL_ITEMS constant', async () => {
    const { fetchPage } = server(MAX_FETCH_ALL_ITEMS + 600);
    const items = await fetchAllPages(fetchPage);
    expect(items).toHaveLength(MAX_FETCH_ALL_ITEMS);
  });

  test('propagates a failing page instead of returning a partial list', async () => {
    const failing = async (skip: number) => {
      if (skip > 0) throw new Error('boom');
      return Array.from({ length: 500 }, (_, index) => index);
    };
    await expect(fetchAllPages(failing)).rejects.toThrow('boom');
  });
});
