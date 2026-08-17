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

import { useEffect, useMemo, useContext } from 'react';
import { useTransporterLookupByUser } from 'queries/transporters';
import { useOperatorLookup } from 'queries/operators';
import { useAllGeofences } from 'queries/geofences';
import type { SelectListItem } from 'controls/Dialogs/CustomSelect';
import { ACCOUNT_STATUS_NAME, ACCOUNT_STATUS_I18N } from 'data/accountStatuses';
import { LoadingContext } from 'LoadingContext';
import { useTranslation } from 'react-i18next';
import { useAuth } from 'AuthContext';

/**
 * The report filter form is CATALOG-DRIVEN: each catalog row carries its filter
 * definitions as JSON (Manager `reports.filters`, seeded from the backend catalog
 * contributions), so adding a report or a filter is a backend-only change. A
 * definition names the value key the Reporting request sends back
 * (`filters.values[name]`), its datatype, the portal picker list feeding it
 * (absent = free input by type) and its i18n label key.
 *
 * Every filter is optional by contract: pickers render a selectable "All" empty
 * option and an empty value means "no filter" server-side.
 */
export type FilterDataType = 'text' | 'guid' | 'datetime' | 'number';
export type FilterPickerSource = 'transporters' | 'operators' | 'geofences' | 'accountStatus';

export interface ReportFilterDefinition {
  name: string;
  type: FilterDataType;
  labelKey: string;
  source?: FilterPickerSource;
}

const DATA_TYPES: readonly FilterDataType[] = ['text', 'guid', 'datetime', 'number'];
const PICKER_SOURCES: readonly FilterPickerSource[] = [
  'transporters',
  'operators',
  'geofences',
  'accountStatus',
];

/**
 * Parses a catalog row's `filters` JSON defensively: a malformed document yields an
 * empty filter set (the report still runs, unfiltered), a malformed entry is dropped,
 * an unknown datatype falls back to free text and an unknown picker source falls back
 * to a free input — a newer backend must never break the form.
 */
export function parseFilterDefinitions(json: string | null | undefined): ReportFilterDefinition[] {
  if (!json) return [];

  let parsed: unknown;
  try {
    parsed = JSON.parse(json);
  } catch {
    return [];
  }
  if (!Array.isArray(parsed)) return [];

  const definitions: ReportFilterDefinition[] = [];
  for (const entry of parsed) {
    if (typeof entry !== 'object' || entry === null) continue;
    const { name, type, labelKey, source } = entry as Record<string, unknown>;
    if (typeof name !== 'string' || name === '' || typeof labelKey !== 'string' || labelKey === '') {
      continue;
    }
    definitions.push({
      name,
      type: DATA_TYPES.includes(type as FilterDataType) ? (type as FilterDataType) : 'text',
      labelKey,
      source: PICKER_SOURCES.includes(source as FilterPickerSource)
        ? (source as FilterPickerSource)
        : undefined,
    });
  }
  return definitions;
}

/** The resolved form model: definitions plus the option list behind each picker source. */
export interface ReportFiltersModel {
  definitions: ReportFilterDefinition[];
  optionsBySource: Record<FilterPickerSource, SelectListItem[]>;
}

/**
 * Resolves the filter form model for the selected catalog row: parses its filter
 * definitions and loads exactly the picker lists those definitions reference.
 */
function useFiltersData(filtersJson: string | null | undefined): ReportFiltersModel {
  const { t } = useTranslation();
  const { isAuthenticated } = useAuth();
  const { setLoading } = useContext(LoadingContext);

  const definitions = useMemo(() => parseFilterDefinitions(filtersJson), [filtersJson]);
  const needs = (source: FilterPickerSource) => definitions.some((d) => d.source === source);

  const transportersQuery = useTransporterLookupByUser({
    enabled: isAuthenticated && needs('transporters'),
  });
  const operatorsQuery = useOperatorLookup({ enabled: isAuthenticated && needs('operators') });
  const geofencesQuery = useAllGeofences(false, {}, {
    enabled: isAuthenticated && needs('geofences'),
  });

  // Keep the global spinner UX while the picker lists load.
  useEffect(() => {
    setLoading(
      transportersQuery.isFetching || operatorsQuery.isFetching || geofencesQuery.isFetching
    );
  }, [
    transportersQuery.isFetching,
    operatorsQuery.isFetching,
    geofencesQuery.isFetching,
    setLoading,
  ]);

  const optionsBySource = useMemo<Record<FilterPickerSource, SelectListItem[]>>(
    () => ({
      transporters: (transportersQuery.data ?? []).map((transporter) => ({
        value: transporter.transporterId,
        label: transporter.name,
      })),
      operators: (operatorsQuery.data ?? []).map((operator) => ({
        value: operator.operatorId,
        label: operator.name,
      })),
      // Deactivated geofences stay listed: recorded visit history keeps referencing them.
      geofences: (geofencesQuery.data ?? [])
        .map((geofence) => ({ value: geofence.geofenceId as string, label: geofence.name }))
        .sort((a, b) => a.label.localeCompare(b.label)),
      accountStatus: Object.values(ACCOUNT_STATUS_NAME).map((name) => ({
        value: name,
        label: t(ACCOUNT_STATUS_I18N[name]),
      })),
    }),
    [transportersQuery.data, operatorsQuery.data, geofencesQuery.data, t]
  );

  return { definitions, optionsBySource };
}

export default useFiltersData;
