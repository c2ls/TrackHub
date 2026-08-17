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

import { useEffect, useMemo, useRef, useState } from 'react';
import type { ListParams } from 'api/core/paging';

/** Milliseconds of quiet before a search draft is pushed to the server query. */
const SEARCH_DEBOUNCE_MS = 350;

export interface UseServerListResult {
  /** Zero-based page index. */
  page: number;
  setPage: (page: number) => void;
  /** Live value of the search box (not yet applied). */
  searchDraft: string;
  setSearchDraft: (value: string) => void;
  /** `{ skip, take, search }` for the paged query — stable between keystrokes. */
  params: Required<ListParams>;
}

/**
 * Paging + server-side search state for a list screen backed by a paged query.
 *
 * The shared `Table` control searches and pages CLIENT-side, so a screen that
 * reads one server page must drive both from here instead: the draft is
 * debounced into `params.search`, jumping back to the first page since the new
 * results start from the top.
 *
 * Call {@link useClampPage} with the server total afterwards so a delete that
 * empties the last page does not strand the user on it.
 */
export function useServerList(pageSize: number): UseServerListResult {
  const [page, setPage] = useState(0);
  const [searchDraft, setSearchDraft] = useState('');
  const [search, setSearch] = useState('');

  // The ref guards against re-applying an unchanged value (e.g. the initial
  // empty draft), which would otherwise reset the page on every mount.
  const appliedSearchRef = useRef('');
  useEffect(() => {
    const handle = setTimeout(() => {
      if (appliedSearchRef.current === searchDraft) return;
      appliedSearchRef.current = searchDraft;
      setPage(0);
      setSearch(searchDraft);
    }, SEARCH_DEBOUNCE_MS);
    return () => clearTimeout(handle);
  }, [searchDraft]);

  const params = useMemo(
    () => ({
      skip: page * pageSize,
      take: pageSize,
      search: search.trim() === '' ? null : search.trim(),
    }),
    [page, pageSize, search]
  );

  return { page, setPage, searchDraft, setSearchDraft, params };
}

/**
 * Snaps the page back when the result set shrinks below its start — deleting the
 * last row of a non-first page otherwise leaves the user on an empty page with
 * no rows to act on. Call it after the paged query has reported its total.
 */
export function useClampPage(
  page: number,
  pageSize: number,
  totalCount: number,
  setPage: (page: number) => void
): void {
  useEffect(() => {
    if (page > 0 && page * pageSize >= totalCount) {
      setPage(Math.max(0, Math.ceil(totalCount / pageSize) - 1));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [totalCount, page, pageSize]);
}

export default useServerList;
