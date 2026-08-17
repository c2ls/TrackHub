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
import { useImportTripsCsv } from 'queries/trips';
import { downloadCsv } from 'utils/csvUtils';
import type { TripCsvImportResult } from 'api/tripManagement/trips';

interface TripImportDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
}

/**
 * The column contract, in the order the backend parses it. Places are named, never
 * coordinates: a dispatcher planning a week in a spreadsheet types "Plant 3", and
 * the server resolves that against the account's geofences and then its POIs
 * (spec 11a §9.1).
 */
const TEMPLATE_COLUMNS = [
  'code',
  'transporter',
  'driver',
  'customer',
  'origin',
  'destinations',
  'activities',
  'tripType',
  'plannedStart',
  'plannedEnd',
  'startedAt',
  'externalReference',
  'notes',
];

/** One filled-in line, so the operator can see the shape rather than infer it. */
const TEMPLATE_EXAMPLE = [
  'TRIP-001',
  'Truck 12',
  'Ana Gomez',
  'ACME',
  'Plant 3',
  'Client X;Client Y',
  'Unload;Unload',
  'single',
  '2026-08-10T07:00',
  '2026-08-10T17:00',
  '',
  'PO-99123',
  '',
];

/**
 * Bulk trip planning — the input side of zero-touch (spec 11a §9.1).
 *
 * A company that dispatches hundreds of trips a day plans a whole week per vehicle
 * at once; without this, a dispatcher who no longer clicks Start per trip would
 * still be creating three hundred of them one dialog at a time.
 *
 * Same contract as the toll-catalog upload: the backend never batch-fails, so valid
 * rows land and each rejected row comes back with its line number and reason — which
 * is exactly what this renders, so the operator fixes those lines and re-imports
 * instead of guessing which of three hundred was wrong.
 */
function TripImportDialog({ open, setOpen }: TripImportDialogProps) {
  const { t } = useTranslation();
  const [csv, setCsv] = useState('');
  const [result, setResult] = useState<TripCsvImportResult | null>(null);
  const importTrips = useImportTripsCsv();

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
      setResult(await importTrips.mutateAsync(csv));
    } catch {
      // Transport/authorization failures surface in the global toast; row-level
      // problems are not errors here — they come back inside the result.
    }
  };

  const errorColumns = [
    { name: 'row', title: t('trips.import.row'), align: 'left' as const },
    { name: 'code', title: t('trips.import.errorCode'), align: 'left' as const },
    { name: 'message', title: t('trips.import.message'), align: 'left' as const },
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
      title={t('trips.import.title')}
      handleSave={handleSave}
      handleCancel={reset}
      open={open}
      setOpen={setOpen}
      maxWidth="md"
    >
      {result ? (
        <ArgonBox>
          <ArgonTypography variant="h6" fontWeight="medium">
            {t('trips.import.result')}
          </ArgonTypography>
          <Grid container spacing={1} sx={{ mt: 1 }}>
            {[
              ['trips.import.rowsRead', result.rowsRead],
              ['trips.import.tripsCreated', result.tripsCreated],
            ].map(([labelKey, value]) => (
              <Grid size={{ xs: 6 }} key={labelKey as string}>
                <ArgonTypography variant="caption" color="secondary">
                  {t(labelKey as 'trips.import.rowsRead')}
                </ArgonTypography>
                <ArgonTypography variant="h6" fontWeight="medium">
                  {value}
                </ArgonTypography>
              </Grid>
            ))}
          </Grid>
          <ArgonBox mt={2}>
            <ArgonTypography variant="button" fontWeight="medium">
              {t('trips.import.errors')}
            </ArgonTypography>
            {errorRows.length === 0 ? (
              <ArgonTypography variant="caption" color="success" display="block">
                {t('trips.import.noErrors')}
              </ArgonTypography>
            ) : (
              <Table columns={errorColumns} rows={errorRows} compact scrollable maxHeight="300px" />
            )}
          </ArgonBox>
        </ArgonBox>
      ) : (
        <ArgonBox>
          <ArgonTypography variant="caption" color="secondary">
            {t('trips.import.hint')}
          </ArgonTypography>
          <ArgonBox mt={2} mb={1} display="flex" gap={1}>
            <ArgonButton variant="outlined" color="info" size="small" component="label">
              {t('trips.import.file')}
              <input type="file" accept=".csv,text/csv" hidden onChange={handleFile} />
            </ArgonButton>
            <ArgonButton
              variant="text"
              color="info"
              size="small"
              onClick={() => downloadCsv('trips-template.csv', TEMPLATE_COLUMNS, [TEMPLATE_EXAMPLE])}
            >
              {t('trips.import.template')}
            </ArgonButton>
          </ArgonBox>
          <CustomTextField
            margin="dense"
            name="csv"
            id="csv"
            label={t('trips.import.paste')}
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

export default TripImportDialog;
