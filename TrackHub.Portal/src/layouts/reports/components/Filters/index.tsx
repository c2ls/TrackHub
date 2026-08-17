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
import type { SelectListItem } from 'controls/Dialogs/CustomSelect';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import useFiltersData from "layouts/reports/data/filtersData";
import useForm from 'controls/Dialogs/useForm';
import type { ReportFilterValues } from "api/reporting/reports";
import { useTranslation } from 'react-i18next';

/** The action a filter-form button triggers. */
export type ReportAction = 'preview' | 'xlsx' | 'pdf';

interface ReportFiltersProps {
  selectedReport?: string;
  supportsPdf?: boolean;
  running?: boolean;
  onRun: (values: ReportFilterValues, action: ReportAction) => void | Promise<void>;
}

function ReportFilters({ selectedReport, supportsPdf = false, running = false, onRun }: ReportFiltersProps) {
  const [values, handleChange, setValues, setErrors, validate, errors] =
    useForm<ReportFilterValues>({});

  const { t } = useTranslation();
  const { data, stringKinds } = useFiltersData(selectedReport ?? '');
  const {
    stringFilter1,
    stringFilter2,
    stringFilter3,
    dateTimeFilter1,
    dateTimeFilter2,
    dateTimeFilter3,
    numericFilter1,
    numericFilter2,
    numericFilter3,
  } = data;

  // Reset the form whenever the selected report (and therefore its filters) changes.
  useEffect(() => {
    setValues({});
    setErrors({});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedReport]);

  async function run(action: ReportAction) {
    if (validate([])) {
      await onRun(values, action);
    }
  }

  return (
    <Card>
      <ArgonBox pt={1.5} pb={2} px={2} lineHeight={1.25}>
        {stringFilter1.visible && (
          <ArgonBox display="flex" py={1} mb={0.25}>
            {stringKinds[0] === 'text' ? (
              <CustomTextField
                name="selectedItem1"
                id="selectedItem1"
                label={stringFilter1.label}
                type="text"
                fullWidth
                value={values.selectedItem1 || ''}
                errorMsg={errors.selectedItem1}
                onChange={handleChange} />
            ) : (
              <CustomSelect
                list={stringFilter1.data as SelectListItem[]}
                handleChange={handleChange}
                name="selectedItem1"
                id="selectedItem1"
                label={stringFilter1.label}
                value={values.selectedItem1 || ''}
                numericValue={false}
              />
            )}
          </ArgonBox>)}
        {stringFilter2.visible && (
          <ArgonBox display="flex" py={1} mb={0.25}>
            {stringKinds[1] === 'text' ? (
              <CustomTextField
                name="selectedItem2"
                id="selectedItem2"
                label={stringFilter2.label}
                type="text"
                fullWidth
                value={values.selectedItem2 || ''}
                errorMsg={errors.selectedItem2}
                onChange={handleChange} />
            ) : (
              <CustomSelect
                list={stringFilter2.data as SelectListItem[]}
                handleChange={handleChange}
                name="selectedItem2"
                id="selectedItem2"
                label={stringFilter2.label}
                value={values.selectedItem2 || ''}
                numericValue={false}
              />
            )}
          </ArgonBox>)}
        {stringFilter3.visible && (
          <ArgonBox display="flex" py={1} mb={0.25}>
            {stringKinds[2] === 'text' ? (
              <CustomTextField
                name="selectedItem3"
                id="selectedItem3"
                label={stringFilter3.label}
                type="text"
                fullWidth
                value={values.selectedItem3 || ''}
                errorMsg={errors.selectedItem3}
                onChange={handleChange} />
            ) : (
              <CustomSelect
                list={stringFilter3.data as SelectListItem[]}
                handleChange={handleChange}
                name="selectedItem3"
                id="selectedItem3"
                label={stringFilter3.label}
                value={values.selectedItem3 || ''}
                numericValue={false}
              />
            )}
          </ArgonBox>)}
        {dateTimeFilter1.visible && (
          <ArgonBox display="flex" py={1} mb={0.25}>
            <CustomTextField
              name="selectedDate1"
              id="selectedDate1"
              label={dateTimeFilter1.label}
              type="datetime-local"
              fullWidth
              value={values.selectedDate1 || ''}
              errorMsg={errors.selectedDate1}
              onChange={handleChange} />
          </ArgonBox>)}
        {dateTimeFilter2.visible && (
          <ArgonBox display="flex" py={1} mb={0.25}>
            <CustomTextField
              name="selectedDate2"
              id="selectedDate2"
              label={dateTimeFilter2.label}
              type="datetime-local"
              fullWidth
              value={values.selectedDate2 || ''}
              errorMsg={errors.selectedDate2}
              onChange={handleChange} />
          </ArgonBox>)}
        {dateTimeFilter3.visible && (
          <ArgonBox display="flex" py={1} mb={0.25}>
            <CustomTextField
              name="selectedDate3"
              id="selectedDate3"
              label={dateTimeFilter3.label}
              type="datetime-local"
              fullWidth
              value={values.selectedDate3 || ''}
              errorMsg={errors.selectedDate3}
              onChange={handleChange} />
          </ArgonBox>)}
        {numericFilter1.visible && (
          <ArgonBox display="flex" py={1} mb={0.25}>
            <CustomTextField
              name="selectedNumber1"
              id="selectedNumber1"
              label={numericFilter1.label}
              type="number"
              fullWidth
              value={values.selectedNumber1 ?? ''}
              errorMsg={errors.selectedNumber1}
              onChange={handleChange} />
          </ArgonBox>)}
        {numericFilter2.visible && (
          <ArgonBox display="flex" py={1} mb={0.25}>
            <CustomTextField
              name="selectedNumber2"
              id="selectedNumber2"
              label={numericFilter2.label}
              type="number"
              fullWidth
              value={values.selectedNumber2 ?? ''}
              errorMsg={errors.selectedNumber2}
              onChange={handleChange} />
          </ArgonBox>)}
        {numericFilter3.visible && (
          <ArgonBox display="flex" py={1} mb={0.25}>
            <CustomTextField
              name="selectedNumber3"
              id="selectedNumber3"
              label={numericFilter3.label}
              type="number"
              fullWidth
              value={values.selectedNumber3 ?? ''}
              errorMsg={errors.selectedNumber3}
              onChange={handleChange} />
          </ArgonBox>)}

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
