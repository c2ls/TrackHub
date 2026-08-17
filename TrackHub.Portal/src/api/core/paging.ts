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
 * Skip/take exhaustion helper for the api layer.
 *
 * Every Manager/Security list query clamps `take` to 1..500 server-side, so a
 * single call can never be assumed to return the whole set: a caller that asks
 * for "all drivers" and reads one page silently loses every row past the page
 * boundary. This mirrors the `FetchAllAsync` exhaustion the Reporting service
 * uses — page at the server's maximum until a short page comes back.
 */

/** The server-side clamp ceiling shared by every paged Manager/Security query. */
export const MAX_PAGE_SIZE = 500;

/**
 * Hard ceiling on an exhaustive read. Reaching it means the caller is pulling a
 * genuinely unbounded dataset (e.g. an account's whole assignment history) and
 * must narrow with filters instead — surfaces compare `items.length` against
 * this constant to say so out loud rather than truncating in silence.
 */
export const MAX_FETCH_ALL_ITEMS = 5000;

/**
 * Skip/take/search arguments shared by every paged Manager list query. The
 * server requires the wrapping input object but each member is optional.
 */
export interface ListParams {
  skip?: number | null;
  take?: number | null;
  search?: string | null;
}

/** One page of a `…PageVm` result: the rows plus the unfiltered-by-paging total. */
export interface Page<T> {
  items: T[];
  totalCount: number;
}

export interface FetchAllOptions {
  /** Rows requested per round trip (clamped to the server ceiling). */
  pageSize?: number;
  /** Stop after this many rows; the caller is expected to notice and warn. */
  maxItems?: number;
}

/**
 * Pages through `fetchPage` until it returns a short page (or nothing), and
 * returns the concatenated result.
 */
export async function fetchAllPages<T>(
  fetchPage: (skip: number, take: number) => Promise<T[]>,
  { pageSize = MAX_PAGE_SIZE, maxItems = MAX_FETCH_ALL_ITEMS }: FetchAllOptions = {}
): Promise<T[]> {
  const take = Math.min(Math.max(1, pageSize), MAX_PAGE_SIZE);
  const items: T[] = [];

  for (let skip = 0; items.length < maxItems; skip += take) {
    const page = await fetchPage(skip, take);
    if (!page || page.length === 0) break;
    items.push(...page);
    // A short page means the server had nothing more to give.
    if (page.length < take) break;
  }

  return items.length > maxItems ? items.slice(0, maxItems) : items;
}
