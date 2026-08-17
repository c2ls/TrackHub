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

import { useEffect } from "react";
import Card from "@mui/material/Card";
import Icon from "@mui/material/Icon";
import ArgonButton from "components/ArgonButton";
import ArgonBox from "components/ArgonBox";
import CustomSelect from 'controls/Dialogs/CustomSelect';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import useFiltersData from "layouts/reports/data/filtersData";
import type { ReportFilterDefinition } from "layouts/reports/data/filtersData";
import useForm from 'controls/Dialogs/useForm';
import type { ReportFilterValues } from "api/reporting/reports";
import { formatDateTimeOffSet, formatJSONValue } from 'utils/dataUtils';
import { useTranslation } from 'react-i18next';

/** The action a filter-form button triggers. */
export type ReportAction = 'preview' | 'xlsx' | 'pdf';

/** Raw form state: filter name → the control's raw string value. */
type FormValues = Record<string, string>;

interface ReportFiltersProps {
  selectedReport?: string;
  /** The catalog row's filter-definitions JSON (ReportVm.filters). */
  filtersJson?: string | null;
  supportsPdf?: boolean;
  running?: boolean;
  onRun: (values: ReportFilterValues, action: ReportAction) => void | Promise<void>;
}

/** Maps a definition's raw form value to the wire value the Reporting request carries. */
function toWireValue(definition: ReportFilterDefinition, raw: string | undefined): string | null {
  if (definition.type === 'datetime') return formatDateTimeOffSet(raw || null);
  return formatJSONValue(raw?.trim() || null);
}

/**
 * The report filter form, rendered entirely from the catalog row's filter
 * definitions: a definition with a picker source becomes a select whose empty
 * option is a selectable "All" (every filter is optional — empty means "no
 * filter" server-side); otherwise the datatype picks the input control.
 */
function ReportFilters({ selectedReport, filtersJson, supportsPdf = false, running = false, onRun }: ReportFiltersProps) {
  const [values, handleChange, setValues, setErrors, validate, errors] =
    useForm<FormValues>({});

  const { t } = useTranslation();
  const { definitions, optionsBySource } = useFiltersData(filtersJson);

  // Reset the form whenever the selected report (and therefore its filters) changes.
  useEffect(() => {
    setValues({});
    setErrors({});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedReport]);

  async function run(action: ReportAction) {
    if (validate([])) {
      const wireValues: ReportFilterValues = {};
      for (const definition of definitions) {
        wireValues[definition.name] = toWireValue(definition, values[definition.name]);
      }
      await onRun(wireValues, action);
    }
  }

  const label = (definition: ReportFilterDefinition) =>
    t(definition.labelKey as 'reports.from', { defaultValue: definition.name });

  return (
    <Card>
      <ArgonBox pt={1.5} pb={2} px={2} lineHeight={1.25}>
        {definitions.map((definition) => (
          <ArgonBox display="flex" py={1} mb={0.25} key={definition.name}>
            {definition.source ? (
              <CustomSelect
                list={optionsBySource[definition.source]}
                handleChange={handleChange}
                name={definition.name}
                id={definition.name}
                label={label(definition)}
                value={values[definition.name] || ''}
                numericValue={false}
                allowEmpty
                placeholder={t('reports.all')}
              />
            ) : (
              <CustomTextField
                name={definition.name}
                id={definition.name}
                label={label(definition)}
                type={
                  definition.type === 'datetime'
                    ? 'datetime-local'
                    : definition.type === 'number'
                      ? 'number'
                      : 'text'
                }
                fullWidth
                value={values[definition.name] || ''}
                errorMsg={errors[definition.name]}
                onChange={handleChange} />
            )}
          </ArgonBox>
        ))}

        <ArgonBox display="flex" gap={1} mt={2} flexWrap="wrap">
          <ArgonButton
            variant="gradient"
            onClick={() => run('preview')}
            disabled={running}
            color="info">
            <Icon sx={{ fontWeight: "bold" }}>visibility</Icon>
            &nbsp;{t('reports.preview')}
          </ArgonButton>
          <ArgonButton
            variant="gradient"
            onClick={() => run('xlsx')}
            disabled={running}
            color="success">
            <Icon sx={{ fontWeight: "bold" }}>download</Icon>
            &nbsp;{t('reports.exportExcel')}
          </ArgonButton>
          {supportsPdf && (
            <ArgonButton
              variant="gradient"
              onClick={() => run('pdf')}
              disabled={running}
              color="error">
              <Icon sx={{ fontWeight: "bold" }}>picture_as_pdf</Icon>
              &nbsp;{t('reports.exportPdf')}
            </ArgonButton>)}
        </ArgonBox>
      </ArgonBox>
    </Card>
  );
};

export default ReportFilters;
