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

// Finding AD-03, gotchas 1 and 2: the shared `Table` pages, searches and sorts
// CLIENT-side. On a server-paged screen all three would silently operate within
// the loaded page while looking like they applied to the whole result set, so
// `serverPaged` must switch every one of them off — and `useServerList` must
// push the search to the server and never strand the user on an emptied page.

import type { ReactElement } from 'react';
import { describe, expect, it, vi, beforeAll } from 'vitest';
import { act, fireEvent, render, screen, renderHook } from '@testing-library/react';
import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

import enTranslations from 'locales/en.json';
import { TestWrapper } from '../components/testHelpers';
import Table from 'controls/Tables/Table';
import ServerPagination from 'controls/Tables/ServerPagination';
import useServerList, { useClampPage } from 'controls/Tables/useServerList';

beforeAll(async () => {
  await i18n.use(initReactI18next).init({
    lng: 'en',
    resources: { en: { translation: enTranslations } },
    interpolation: { escapeValue: false },
  });
});

const columns = [{ name: 'name', title: 'Name', align: 'left' as const }, { name: 'id' }];
/** More rows than the control's 10-row client page, so client paging is visible. */
const rows = Array.from({ length: 25 }, (_, index) => ({
  name: <span>{`unit-${index}`}</span>,
  id: `id-${index}`,
}));

/** The Argon controls need the theme + controller providers to style themselves. */
const renderInTheme = (ui: ReactElement) => render(<TestWrapper>{ui}</TestWrapper>);
const rerenderInTheme = (
  rerender: (ui: ReactElement) => void,
  ui: ReactElement
) => rerender(<TestWrapper>{ui}</TestWrapper>);

describe('Table in serverPaged mode', () => {
  it('renders every row it is given instead of a client page of 10', () => {
    renderInTheme(<Table columns={columns} rows={rows} serverPaged />);
    // Without serverPaged the control would show only the first 10.
    expect(screen.getByText('unit-24')).toBeInTheDocument();
  });

  it('drops the client pager, which would page within the page', () => {
    const { container, rerender } = renderInTheme(<Table columns={columns} rows={rows} />);
    expect(container.querySelector('.MuiTablePagination-root')).not.toBeNull();

    rerenderInTheme(rerender, <Table columns={columns} rows={rows} serverPaged />);
    expect(container.querySelector('.MuiTablePagination-root')).toBeNull();
  });

  it('ignores searchQuery rather than filtering the loaded page only', () => {
    renderInTheme(<Table columns={columns} rows={rows} searchQuery="unit-1" serverPaged />);
    // A client-side filter would have removed unit-0 from view.
    expect(screen.getByText('unit-0')).toBeInTheDocument();
  });

  it('removes the sort affordance, which could only ever sort the page', () => {
    const { rerender } = renderInTheme(<Table columns={columns} rows={rows} />);
    const sortableHeader = screen.getByText('NAME');
    expect(sortableHeader).toHaveStyle({ cursor: 'pointer' });

    rerenderInTheme(rerender, <Table columns={columns} rows={rows} serverPaged />);
    expect(screen.getByText('NAME')).toHaveStyle({ cursor: 'default' });
  });

  it('leaves the row order exactly as the server returned it', () => {
    const { container } = renderInTheme(<Table columns={columns} rows={rows} serverPaged />);
    // Two clicks would be a descending sort if the header were live, putting
    // unit-9 first; the server's order must survive both.
    fireEvent.click(screen.getByText('NAME'));
    fireEvent.click(screen.getByText('NAME'));

    const firstCell = container.querySelectorAll('tbody tr td')[0];
    expect(firstCell).toHaveTextContent('unit-0');
  });
});

describe('ServerPagination', () => {
  it('reports the server range, not the loaded row count', () => {
    renderInTheme(
      <ServerPagination page={2} pageSize={10} totalCount={94} pageLength={10} onPageChange={vi.fn()} />
    );
    expect(screen.getByText('Showing 21–30 of 94')).toBeInTheDocument();
  });

  it('disables next on the last page and previous on the first', () => {
    const onPageChange = vi.fn();
    const { rerender } = renderInTheme(
      <ServerPagination page={0} pageSize={10} totalCount={10} pageLength={10} onPageChange={onPageChange} />
    );
    const [previous, next] = screen.getAllByRole('button');
    expect(previous).toBeDisabled();
    expect(next).toBeDisabled();

    rerenderInTheme(
      rerender,
      <ServerPagination page={0} pageSize={10} totalCount={11} pageLength={10} onPageChange={onPageChange} />
    );
    fireEvent.click(screen.getAllByRole('button')[1]);
    expect(onPageChange).toHaveBeenCalledWith(1);
  });

  it('caps the range end at the total on the last, short page', () => {
    // Every paged query now reports the unpaged total, so the range is exact:
    // page 1 of a 14-row set shows 11–14, not 11–20.
    renderInTheme(<ServerPagination page={1} pageSize={10} totalCount={14} pageLength={4} onPageChange={vi.fn()} />);
    expect(screen.getByText('Showing 11–14 of 14')).toBeInTheDocument();
    expect(screen.getAllByRole('button')[1]).toBeDisabled();
  });
});

describe('useServerList', () => {
  it('debounces the search into the query params and returns to page one', () => {
    vi.useFakeTimers();
    try {
      const { result } = renderHook(() => useServerList(10));

      act(() => result.current.setPage(3));
      expect(result.current.params.skip).toBe(30);

      act(() => result.current.setSearchDraft('acme'));
      // Still un-applied: a keystroke must not fire a query per character.
      expect(result.current.params.search).toBeNull();

      act(() => vi.advanceTimersByTime(400));
      expect(result.current.params).toEqual({ skip: 0, take: 10, search: 'acme' });
    } finally {
      vi.useRealTimers();
    }
  });

  it('sends a whitespace-only search as null rather than a blank filter', () => {
    vi.useFakeTimers();
    try {
      const { result } = renderHook(() => useServerList(10));
      act(() => result.current.setSearchDraft('   '));
      act(() => vi.advanceTimersByTime(400));
      expect(result.current.params.search).toBeNull();
    } finally {
      vi.useRealTimers();
    }
  });
});

describe('useClampPage', () => {
  it('snaps back when a delete shrinks the set below the current page', () => {
    const setPage = vi.fn();
    // Page 3 starts at row 31; only 12 rows survive, so the last page is 1.
    renderHook(() => useClampPage(3, 10, 12, setPage));
    expect(setPage).toHaveBeenCalledWith(1);
  });

  it('leaves a page that still has rows alone', () => {
    const setPage = vi.fn();
    renderHook(() => useClampPage(3, 10, 94, setPage));
    expect(setPage).not.toHaveBeenCalled();
  });

  it('falls back to the first page when everything is deleted', () => {
    const setPage = vi.fn();
    renderHook(() => useClampPage(2, 10, 0, setPage));
    expect(setPage).toHaveBeenCalledWith(0);
  });
});
