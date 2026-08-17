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

import Icon from '@mui/material/Icon';
import { useTranslation } from 'react-i18next';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import ArgonPagination from 'components/ArgonPagination';

/**
 * Footer for a SERVER-paged table: the "showing X–Y of Z" range plus prev/next.
 *
 * The shared `Table` control pages, filters and sorts client-side, so a screen
 * that reads one server page must turn that off (`serverPaged`) and render this
 * instead — otherwise the built-in pager silently pages within the page.
 *
 * Every paged query now reports its unpaged total, so the range is always exact
 * and the last page is arithmetic.
 */
export interface ServerPaginationProps {
  /** Zero-based page index. */
  page: number;
  /** Rows requested per page. */
  pageSize: number;
  /** Total rows matching the current server filters. */
  totalCount: number;
  /** Row count in the page currently rendered — drives the range end. */
  pageLength: number;
  onPageChange: (page: number) => void;
}

function ServerPagination({
  page,
  pageSize,
  totalCount,
  pageLength,
  onPageChange,
}: ServerPaginationProps) {
  const { t } = useTranslation();

  const from = pageLength === 0 ? 0 : page * pageSize + 1;
  const to = Math.min((page + 1) * pageSize, totalCount);
  const hasNext = to < totalCount;

  return (
    <ArgonBox display="flex" justifyContent="space-between" alignItems="center" mt={1}>
      <ArgonTypography variant="caption" color="secondary">
        {t('generic.showing', { from, to, total: totalCount })}
      </ArgonTypography>
      <ArgonPagination>
        <ArgonPagination item onClick={() => onPageChange(Math.max(0, page - 1))} disabled={page === 0}>
          <Icon>keyboard_arrow_left</Icon>
        </ArgonPagination>
        <ArgonPagination item onClick={() => hasNext && onPageChange(page + 1)} disabled={!hasNext}>
          <Icon>keyboard_arrow_right</Icon>
        </ArgonPagination>
      </ArgonPagination>
    </ArgonBox>
  );
}

export default ServerPagination;
