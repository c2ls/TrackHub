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
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import FormDialog from 'controls/Dialogs/FormDialog';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import { DELIVERY_STATUSES } from '../../tripWriteForms';
import type { DeliveryStatus } from '../../tripWriteForms';

interface DeliveryOutcomeDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  /** Delivery being closed out — client + reference, for confirmation. */
  deliveryLabel: string;
  status: DeliveryStatus;
  onStatusChange: (status: DeliveryStatus) => void;
  observations: string;
  onObservationsChange: (observations: string) => void;
}

/**
 * Delivery outcome. The command is idempotent on a `clientEventId` the caller
 * holds for the whole attempt, so pressing save again after a failure updates
 * the same record instead of recording a second outcome (spec 11 §7.3, §9).
 */
function DeliveryOutcomeDialog({
  open,
  setOpen,
  handleSubmit,
  deliveryLabel,
  status,
  onStatusChange,
  observations,
  onObservationsChange,
}: DeliveryOutcomeDialogProps) {
  const { t } = useTranslation();

  const statusOptions = DELIVERY_STATUSES.map((value) => ({
    value,
    label: t(`trips.deliveries.statuses.${value}` as 'trips.deliveries.statuses.Pending'),
  }));

  return (
    <FormDialog
      title={t('trips.deliveries.outcomeTitle')}
      handleSave={handleSubmit}
      open={open}
      setOpen={setOpen}
      maxWidth="sm"
    >
      <ArgonBox mb={1}>
        <ArgonTypography variant="caption" color="secondary" display="block">
          {t('trips.deliveries.outcomeMessage', { delivery: deliveryLabel })}
        </ArgonTypography>
      </ArgonBox>
      <CustomSelect
        list={statusOptions}
        handleChange={(event) => onStatusChange(String(event.target.value) as DeliveryStatus)}
        name="deliveryStatus"
        id="deliveryStatus"
        label={t('trips.deliveries.status')}
        value={status}
        numericValue={false}
        required
      />
      <CustomTextField
        margin="dense"
        name="outcomeObservations"
        id="outcomeObservations"
        label={t('trips.deliveries.observations')}
        type="text"
        multiline
        rows={2}
        value={observations}
        onChange={(event) => onObservationsChange(event.target.value)}
      />
    </FormDialog>
  );
}

export default DeliveryOutcomeDialog;
