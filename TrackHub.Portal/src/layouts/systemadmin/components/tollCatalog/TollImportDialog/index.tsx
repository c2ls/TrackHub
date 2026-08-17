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

import { useState } from 'react';
import type { ChangeEvent, Dispatch, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import Grid from '@mui/material/Grid';
import FormDialog from 'controls/Dialogs/FormDialog';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import Table from 'controls/Tables/Table';
import { Name, Description } from 'controls/Tables/components/tableComponents';
import ArgonBox from 'components/ArgonBox';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import { useImportTollCatalog } from 'queries/trips';
import type { TollCatalogImportResult } from 'api/tripManagement/trips';

interface TollImportDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
}

/**
 * CSV import with a row-level error report. The backend never batch-fails: valid
 * rows land and each rejected row comes back with its number and reason, which
 * is exactly what this dialog renders — the operator fixes those rows and
 * re-imports rather than guessing which of a thousand lines was wrong.
 */
function TollImportDialog({ open, setOpen }: TollImportDialogProps) {
  const { t } = useTranslation();
  const [csv, setCsv] = useState('');
  const [result, setResult] = useState<TollCatalogImportResult | null>(null);
  const importCatalog = useImportTollCatalog();

  const reset = () => {
    setCsv('');
    setResult(null);
  };

  const handleFile = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;
    setCsv(await file.text());
    setResult(null);
  };

  const handleSave = async () => {
    // Second press, after a result is on screen, closes the dialog.
    if (result) {
      reset();
      setOpen(false);
      return;
    }
    if (!csv.trim()) return;
    try {
      setResult(await importCatalog.mutateAsync(csv));
    } catch {
      // Transport/authorization failures surface in the global toast; row-level
      // problems are not errors here — they come back inside the result.
    }
  };

  const errorColumns = [
    { name: 'row', title: t('tolls.catalog.row'), align: 'left' as const },
    { name: 'code', title: t('tolls.catalog.errorCode'), align: 'left' as const },
    { name: 'message', title: t('tolls.catalog.message'), align: 'left' as const },
    { name: 'id' },
  ];

  const errorRows = (result?.errors ?? []).map((error) => ({
    row: <Name name={error.rowNumber} />,
    code: <Description description={error.errorCode} />,
    message: <Description description={error.message} />,
    id: `${error.rowNumber}-${error.errorCode}`,
  }));

  return (
    <FormDialog
      title={t('tolls.catalog.importTitle')}
      handleSave={handleSave}
      handleCancel={reset}
      open={open}
      setOpen={setOpen}
      maxWidth="md"
    >
      {result ? (
        <ArgonBox>
          <ArgonTypography variant="h6" fontWeight="medium">
            {t('tolls.catalog.importResult')}
          </ArgonTypography>
          <Grid container spacing={1} sx={{ mt: 1 }}>
            {[
              ['tolls.catalog.rowsRead', result.rowsRead],
              ['tolls.catalog.stationsCreated', result.stationsCreated],
              ['tolls.catalog.stationsUpdated', result.stationsUpdated],
              ['tolls.catalog.tariffsCreated', result.tariffsCreated],
            ].map(([labelKey, value]) => (
              <Grid size={{ xs: 6, sm: 3 }} key={labelKey as string}>
                <ArgonTypography variant="caption" color="secondary">
                  {t(labelKey as 'tolls.catalog.rowsRead')}
                </ArgonTypography>
                <ArgonTypography variant="h6" fontWeight="medium">
                  {value}
                </ArgonTypography>
              </Grid>
            ))}
          </Grid>
          <ArgonBox mt={2}>
            <ArgonTypography variant="button" fontWeight="medium">
              {t('tolls.catalog.importErrors')}
            </ArgonTypography>
            {errorRows.length === 0 ? (
              <ArgonTypography variant="caption" color="success" display="block">
                {t('tolls.catalog.noErrors')}
              </ArgonTypography>
            ) : (
              <Table columns={errorColumns} rows={errorRows} compact scrollable maxHeight="300px" />
            )}
          </ArgonBox>
        </ArgonBox>
      ) : (
        <ArgonBox>
          <ArgonTypography variant="caption" color="secondary">
            {t('tolls.catalog.importHint')}
          </ArgonTypography>
          <ArgonBox mt={2} mb={1}>
            <ArgonButton variant="outlined" color="info" size="small" component="label">
              {t('tolls.catalog.importFile')}
              <input type="file" accept=".csv,text/csv" hidden onChange={handleFile} />
            </ArgonButton>
          </ArgonBox>
          <CustomTextField
            margin="dense"
            name="csv"
            id="csv"
            label={t('tolls.catalog.import')}
            type="text"
            multiline
            rows={10}
            value={csv}
            onChange={(event) => setCsv(event.target.value)}
          />
        </ArgonBox>
      )}
    </FormDialog>
  );
}

export default TollImportDialog;
