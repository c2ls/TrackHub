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

import { useEffect, useState, useContext } from "react";
import type { TFunction } from "i18next";
import { useTransportersByUser } from 'queries/transporters';
import { useOperators } from 'queries/operators';
import { buildTableData } from 'utils/reportUtils';
import type { TableData } from 'utils/reportUtils';
import type { SelectListItem } from 'controls/Dialogs/CustomSelect';
import { ACCOUNT_STATUS_NAME, ACCOUNT_STATUS_I18N } from 'data/accountStatuses';
import { LoadingContext } from "LoadingContext";
import { useTranslation } from 'react-i18next';
import { useAuth } from "AuthContext";

/**
 * A filter field a report consumes. Each kind maps to exactly one FilterDto
 * slot, so a report's spec is an unordered set of these:
 *   transporter | operator | status → stringFilter1 (selectedItem1)
 *   device                 → stringFilter2 (selectedItem2, free-text GUID)
 *   from                   → dateTimeFilter1 (selectedDate1)
 *   to                     → dateTimeFilter2 (selectedDate2)
 *   maxRows|withinDays|lookbackHours → numericFilter1 (selectedNumber1)
 */
export type FilterFieldKind =
  | 'transporter'
  | 'operator'
  | 'device'
  | 'status'
  | 'from'
  | 'to'
  | 'maxRows'
  | 'withinDays'
  | 'lookbackHours';

/** How a string-filter slot is entered: a picker (`select`) or free text. */
export type StringInputKind = 'select' | 'text';

/**
 * Which FilterDto fields each seeded report consumes — derived from the Reporting
 * report factories. Codes are the server's report codes, NOT
 * hardcoded UI ordering; the catalog is still rendered from whatever the server
 * returns. Reports consuming nothing get an explicit empty spec.
 */
export const REPORT_FILTER_SPECS: Record<string, FilterFieldKind[]> = {
  // Operations
  LiveReport: [],
  PositionRecord: ['transporter', 'from', 'to'],
  TransportersInGeofence: [],
  GeofenceEvents: ['transporter', 'from', 'to'],
  // Gps
  'gps.provider-health-summary': ['lookbackHours'],
  'gps.provider-sync-history': ['operator', 'maxRows', 'from', 'to'],
  'gps.sync-statistics': ['maxRows', 'from', 'to'],
  'gps.synchronized-device-inventory': ['operator'],
  'gps.recently-added-devices': ['withinDays', 'from'],
  'gps.unassigned-devices': [],
  'gps.ignored-devices': ['operator'],
  'gps.assignment-history': ['transporter', 'from', 'to'],
  'gps.latest-position-freshness': [],
  'gps.position-history': ['transporter', 'device', 'maxRows', 'from', 'to'],
  // Documents
  'documents-expiring': ['withinDays'],
  'documents-missing-required': [],
  'documents-share-activity': [],
  'documents-upload-volume': ['from', 'to'],
  // Administration (manager-only)
  'accounts-by-status': ['status'],
  'feature-enablement-matrix': [],
  'group-membership-export': [],
};

/**
 * Explicit default for any code without a registered strategy: a date-range
 * filter (language is always sent). Replaces the former silent empty fallback.
 */
export const DEFAULT_FILTER_SPEC: FilterFieldKind[] = ['from', 'to'];

/** Returns the registered filter spec for a report code, or the date-range default. */
export function getReportFilterSpec(reportCode: string): FilterFieldKind[] {
  return REPORT_FILTER_SPECS[reportCode] ?? DEFAULT_FILTER_SPEC;
}

/** True when the report's filter form needs the transporter list loaded. */
export function reportNeedsTransporters(reportCode: string): boolean {
  return getReportFilterSpec(reportCode).includes('transporter');
}

/** True when the report's filter form needs the operator list loaded. */
export function reportNeedsOperators(reportCode: string): boolean {
  return getReportFilterSpec(reportCode).includes('operator');
}

/** Picker option lists a filter form may wire into its string selects. */
export interface FilterPickerOptions {
  transporters: SelectListItem[];
  operators: SelectListItem[];
}

/**
 * Input kind for each of the three string-filter slots (`stringFilter1..3`).
 * Slots are pickers by default; `device` (stringFilter2) is a free-text GUID.
 */
export function getStringInputKinds(spec: FilterFieldKind[]): [StringInputKind, StringInputKind, StringInputKind] {
  const kinds: [StringInputKind, StringInputKind, StringInputKind] = ['select', 'select', 'select'];
  if (spec.includes('device')) {
    kinds[1] = 'text';
  }
  return kinds;
}

/** AccountStatus options (enum name value + localized label) for the status filter. */
function statusOptions(t: TFunction): SelectListItem[] {
  return Object.values(ACCOUNT_STATUS_NAME).map((name) => ({
    value: name,
    label: t(ACCOUNT_STATUS_I18N[name]),
  }));
}

/**
 * Builds the visibility/label/data {@link TableData} for a report's filter spec.
 * Pure given its inputs — the hook wires in the picker lists and translator.
 * Slot mapping (kept in sync with {@link getStringInputKinds}):
 *   transporter|operator|status → stringFilter1 · device → stringFilter2 (text).
 */
export function buildFilterTableData(
  spec: FilterFieldKind[],
  pickers: FilterPickerOptions,
  t: TFunction
): TableData {
  const visibility = [false, false, false, false, false, false, false, false, false];
  const labels = ['', '', '', '', '', '', '', '', ''];
  let list1: SelectListItem[] = [];

  for (const field of spec) {
    switch (field) {
      case 'transporter':
        visibility[0] = true;
        labels[0] = t('reports.transporter');
        list1 = pickers.transporters;
        break;
      case 'operator':
        visibility[0] = true;
        labels[0] = t('reports.operator');
        list1 = pickers.operators;
        break;
      case 'status':
        visibility[0] = true;
        labels[0] = t('reports.status');
        list1 = statusOptions(t);
        break;
      case 'device':
        // Free-text GUID (no device picker source); rendered as a text input.
        visibility[1] = true;
        labels[1] = t('reports.device');
        break;
      case 'from':
        visibility[3] = true;
        labels[3] = t('reports.from');
        break;
      case 'to':
        visibility[4] = true;
        labels[4] = t('reports.to');
        break;
      case 'maxRows':
        visibility[6] = true;
        labels[6] = t('reports.maxRows');
        break;
      case 'withinDays':
        visibility[6] = true;
        labels[6] = t('reports.withinDays');
        break;
      case 'lookbackHours':
        visibility[6] = true;
        labels[6] = t('reports.lookbackHours');
        break;
    }
  }

  return buildTableData({ list1, visibility, labels });
}

/** Resolves the filter form model (TableData + per-string-slot input kinds). */
function useFiltersData(reportCode: string): {
  data: TableData;
  stringKinds: [StringInputKind, StringInputKind, StringInputKind];
} {
  const { t } = useTranslation();
  const { isAuthenticated } = useAuth();
  const { setLoading } = useContext(LoadingContext);
  const [data, setData] = useState<TableData>(buildTableData({}));

  const spec = getReportFilterSpec(reportCode);
  const needsTransporters = reportNeedsTransporters(reportCode);
  const needsOperators = reportNeedsOperators(reportCode);
  const transportersQuery = useTransportersByUser({ enabled: isAuthenticated && needsTransporters });
  const operatorsQuery = useOperators({ enabled: isAuthenticated && needsOperators });

  // Keep the global spinner UX while the picker lists load.
  useEffect(() => {
    setLoading(transportersQuery.isFetching || operatorsQuery.isFetching);
  }, [transportersQuery.isFetching, operatorsQuery.isFetching, setLoading]);

  useEffect(() => {
    if (!reportCode || !isAuthenticated) return;

    const transporters: SelectListItem[] = (transportersQuery.data ?? []).map((transporter) => ({
      value: transporter.transporterId,
      label: transporter.name,
    }));
    const operators: SelectListItem[] = (operatorsQuery.data ?? []).map((operator) => ({
      value: operator.operatorId,
      label: operator.name,
    }));

    setData(buildFilterTableData(getReportFilterSpec(reportCode), { transporters, operators }, t));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [reportCode, isAuthenticated, transportersQuery.data, operatorsQuery.data]);

  return { data, stringKinds: getStringInputKinds(spec) };
}

export default useFiltersData;
