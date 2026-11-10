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

import Card from '@mui/material/Card';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import CompactSelect from 'controls/Selects/CompactSelect';
import type { CompactSelectOption } from 'controls/Selects/CompactSelect';
import { useTranslation } from 'react-i18next';

/** An option for one of the filter-bar selects. */
export type FilterOption = CompactSelectOption;

/** Client-side filter state driving the live map narrowing. */
export interface DashboardFilters {
  transporterType: string;
  groupId: string | number;
  operatorId: string;
  status: string;
}

/** One figure of the fleet summary; `share` is its percentage of the total, when it has one. */
export interface FleetStat {
  title: string;
  count: number;
  share?: string;
}

interface FilterBarProps {
  stats: FleetStat[];
  typeOptions?: FilterOption[];
  groupOptions?: FilterOption[];
  operatorOptions?: FilterOption[];
  filters: DashboardFilters;
  onFilterChange: (name: string, value: string | number) => void;
}

// Fleet summary plus the client-side filters of the live map. Filters only narrow the
// already-authorized set shown on the map; the summary and the unit list keep the full set.
// Unit search-by-plate lives in the page's top-right search box.
function FilterBar({
  stats,
  typeOptions = [],
  groupOptions = [],
  operatorOptions = [],
  filters,
  onFilterChange,
}: FilterBarProps) {
  const { t } = useTranslation();

  const statusOptions: FilterOption[] = [
    { value: 'all', label: t('dashboard.allStatuses') },
    { value: 'moving', label: t('dashboard.statusMoving') },
    { value: 'stopped', label: t('dashboard.statusStopped') },
    { value: 'offline', label: t('dashboard.statusOffline') }
  ];

  return (
    <ArgonBox mb={1.5} display="flex" flexWrap="wrap" alignItems="center" gap={1.5}>
      <Card sx={{ flexDirection: 'row', flexWrap: 'wrap', overflow: 'hidden' }}>
        {stats.map(({ title, count, share }) => (
          <ArgonBox
            key={title}
            px={2}
            py={0.75}
            sx={({ borders: { borderWidth, borderColor } }) => ({
              '&:not(:last-of-type)': { borderRight: `${borderWidth[1]} solid ${borderColor}` },
            })}
          >
            <ArgonTypography variant="caption" color="secondary" display="block" noWrap>
              {title}
            </ArgonTypography>
            <ArgonBox display="flex" alignItems="baseline" gap={0.75}>
              <ArgonTypography variant="h6" fontWeight="bold" lineHeight={1.3}>
                {count}
              </ArgonTypography>
              {share !== undefined && (
                <ArgonTypography variant="caption" color="secondary">
                  {share}%
                </ArgonTypography>
              )}
            </ArgonBox>
          </ArgonBox>
        ))}
      </Card>
      <ArgonBox display="flex" flexWrap="wrap" alignItems="center" gap={1} ml="auto">
        <CompactSelect
          name="groupId"
          value={filters.groupId ?? 'all'}
          options={[{ value: 'all', label: t('dashboard.allGroups') }, ...groupOptions]}
          label={t('dashboard.filterGroup')}
          onChange={onFilterChange}
        />
        <CompactSelect
          name="transporterType"
          value={filters.transporterType || 'all'}
          options={[{ value: 'all', label: t('dashboard.allTypes') }, ...typeOptions]}
          label={t('dashboard.filterType')}
          onChange={onFilterChange}
        />
        <CompactSelect
          name="operatorId"
          value={filters.operatorId || 'all'}
          options={[{ value: 'all', label: t('dashboard.allOperators') }, ...operatorOptions]}
          label={t('dashboard.filterOperator')}
          onChange={onFilterChange}
        />
        <CompactSelect
          name="status"
          value={filters.status || 'all'}
          options={statusOptions}
          label={t('dashboard.filterStatus')}
          onChange={onFilterChange}
        />
      </ArgonBox>
    </ArgonBox>
  );
}

export default FilterBar;
