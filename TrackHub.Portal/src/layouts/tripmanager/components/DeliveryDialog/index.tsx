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

import type { Dispatch, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import Grid from '@mui/material/Grid';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import FormDialog from 'controls/Dialogs/FormDialog';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import type { DeliveryFormValues } from '../../tripWriteForms';

interface DeliveryDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: DeliveryFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
  /** Human label of the stop the delivery belongs to — a delivery cannot move stops. */
  stopLabel: string;
}

/**
 * Delivery editor. A delivery always belongs to one stop: the backend rejects
 * cross-stop moves, so the stop is shown as context and is never editable here
 * (spec 11 §7.3). The delivery's OUTCOME is not set from this dialog either —
 * that is an idempotent, separately audited command.
 */
function DeliveryDialog({
  open,
  setOpen,
  handleSubmit,
  values,
  handleChange,
  errors,
  stopLabel,
}: DeliveryDialogProps) {
  const { t } = useTranslation();

  return (
    <FormDialog
      title={values.deliveryId ? t('trips.deliveries.edit') : t('trips.deliveries.add')}
      handleSave={handleSubmit}
      open={open}
      setOpen={setOpen}
      maxWidth="sm"
    >
      <form>
        <ArgonBox mb={1}>
          <ArgonTypography variant="caption" color="secondary" display="block">
            {t('trips.deliveries.stopContext', { stop: stopLabel })}
          </ArgonTypography>
        </ArgonBox>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="clientName"
              id="clientName"
              label={t('trips.deliveries.client')}
              type="text"
              value={values.clientName || ''}
              onChange={handleChange}
              errorMsg={errors.clientName}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="branchName"
              id="branchName"
              label={t('trips.deliveries.branch')}
              type="text"
              value={values.branchName || ''}
              onChange={handleChange}
              errorMsg={errors.branchName}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="reference"
              id="reference"
              label={t('trips.deliveries.reference')}
              type="text"
              value={values.reference || ''}
              onChange={handleChange}
              errorMsg={errors.reference}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="sequenceIndex"
              id="sequenceIndex"
              label={t('trips.deliveries.sequence')}
              type="number"
              value={values.sequenceIndex ?? 0}
              onChange={handleChange}
              errorMsg={errors.sequenceIndex}
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <CustomTextField
              margin="dense"
              name="productsSummary"
              id="productsSummary"
              label={t('trips.deliveries.products')}
              type="text"
              multiline
              rows={2}
              value={values.productsSummary || ''}
              onChange={handleChange}
              errorMsg={errors.productsSummary}
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <CustomTextField
              margin="dense"
              name="observations"
              id="observations"
              label={t('trips.deliveries.observations')}
              type="text"
              multiline
              rows={2}
              value={values.observations || ''}
              onChange={handleChange}
              errorMsg={errors.observations}
            />
          </Grid>
        </Grid>
      </form>
    </FormDialog>
  );
}

export default DeliveryDialog;
