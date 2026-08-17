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

import { useRef } from 'react';
import type { ChangeEvent, Dispatch, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import Grid from '@mui/material/Grid';
import Icon from '@mui/material/Icon';
import ArgonBox from 'components/ArgonBox';
import ArgonBadge from 'components/ArgonBadge';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import FormDialog from 'controls/Dialogs/FormDialog';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import type { TripDelivery } from 'api/tripManagement/trips';
import { isCleanAttachment } from '../../tripWriteForms';
import type { PodAttachment, PodFormValues } from '../../tripWriteForms';

interface PodDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  handleCancel?: () => void;
  values: PodFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
  /** Human label of the stop the evidence belongs to. */
  stopLabel: string;
  /** Deliveries on that stop — leaving the picker empty closes the whole stop. */
  deliveries: TripDelivery[];
  attachments: PodAttachment[];
  onUploadFiles: (files: File[]) => void;
  onRemoveAttachment: (documentId: string) => void;
  onRefreshAttachment: (documentId: string) => void;
  uploading: boolean;
}

/**
 * Proof-of-delivery capture from the dispatch desk. Spec 11 §9 makes this the
 * module's whole pre-driver-app operating mode: dispatchers record arrivals,
 * departures and POD from the portal.
 *
 * Two contract details drive the shape of this dialog:
 *
 * 1. **Attachments are ordinary spec 04 documents.** They are uploaded through
 *    the existing Manager multipart endpoint and referenced by id — there is no
 *    POD-specific upload surface. A document is only accepted once its scan
 *    reports `Clean`, so each attachment shows its verdict and can be
 *    re-checked; submitting with a non-clean one comes back as
 *    `POD_DOCUMENT_NOT_CLEAN`.
 * 2. **The capture is idempotent on `(tripStopId, clientEventId)`.** The id is
 *    minted once when the dialog opens and reused for every retry of that same
 *    attempt, which is what makes a failed-then-retried submission produce one
 *    POD rather than two.
 */
function PodDialog({
  open,
  setOpen,
  handleSubmit,
  handleCancel,
  values,
  handleChange,
  errors,
  stopLabel,
  deliveries,
  attachments,
  onUploadFiles,
  onRemoveAttachment,
  onRefreshAttachment,
  uploading,
}: PodDialogProps) {
  const { t } = useTranslation();
  const inputRef = useRef<HTMLInputElement | null>(null);

  const deliveryOptions = deliveries.map((delivery) => ({
    value: delivery.deliveryId,
    label: [delivery.clientName, delivery.reference].filter(Boolean).join(' · '),
  }));

  const pickFiles = (event: ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(event.target.files ?? []);
    if (files.length > 0) onUploadFiles(files);
    // Reset so re-picking the same file still fires a change event.
    event.target.value = '';
  };

  const pending = attachments.filter((attachment) => !isCleanAttachment(attachment));

  return (
    <FormDialog
      title={t('pod.captureTitle')}
      handleSave={handleSubmit}
      handleCancel={handleCancel}
      open={open}
      setOpen={setOpen}
      maxWidth="md"
    >
      <form>
        <ArgonBox mb={1}>
          <ArgonTypography variant="caption" color="secondary" display="block">
            {t('pod.captureFor', { stop: stopLabel })}
          </ArgonTypography>
        </ArgonBox>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="receiverName"
              id="receiverName"
              label={t('pod.receiver')}
              type="text"
              value={values.receiverName || ''}
              onChange={handleChange}
              errorMsg={errors.receiverName}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="receiverDocument"
              id="receiverDocument"
              label={t('pod.receiverDocument')}
              type="text"
              value={values.receiverDocument || ''}
              onChange={handleChange}
              errorMsg={errors.receiverDocument}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="capturedAt"
              id="capturedAt"
              label={t('pod.capturedAt')}
              type="datetime-local"
              slotProps={{ inputLabel: { shrink: true } }}
              value={values.capturedAt || ''}
              onChange={handleChange}
              errorMsg={errors.capturedAt}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            {deliveryOptions.length > 0 ? (
              <CustomSelect
                list={deliveryOptions}
                handleChange={handleChange}
                name="deliveryId"
                id="deliveryId"
                label={t('pod.delivery')}
                value={values.deliveryId ?? ''}
                numericValue={false}
                placeholder={t('pod.allDeliveries')}
              />
            ) : (
              <ArgonBox mt={2}>
                <ArgonTypography variant="caption" color="secondary">
                  {t('pod.noDeliveries')}
                </ArgonTypography>
              </ArgonBox>
            )}
          </Grid>
          <Grid size={{ xs: 12 }}>
            {/* A POD naming one delivery leaves the others alone; leaving it
                blank marks every delivery on the stop as delivered. */}
            <ArgonTypography variant="caption" color="secondary">
              {t('pod.deliveryHint')}
            </ArgonTypography>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="latitude"
              id="latitude"
              label={t('pod.latitude')}
              type="number"
              value={values.latitude ?? ''}
              onChange={handleChange}
              errorMsg={errors.latitude}
              helperText={t('pod.locationHint')}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="longitude"
              id="longitude"
              label={t('pod.longitude')}
              type="number"
              value={values.longitude ?? ''}
              onChange={handleChange}
              errorMsg={errors.longitude}
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <CustomTextField
              margin="dense"
              name="notes"
              id="notes"
              label={t('pod.notes')}
              type="text"
              multiline
              rows={2}
              value={values.notes || ''}
              onChange={handleChange}
              errorMsg={errors.notes}
            />
          </Grid>

          <Grid size={{ xs: 12 }}>
            <ArgonTypography variant="button" fontWeight="medium">
              {t('pod.documents')}
            </ArgonTypography>
            <ArgonTypography variant="caption" color="secondary" display="block">
              {t('pod.documentsHint')}
            </ArgonTypography>
            <ArgonBox mt={1}>
              <ArgonButton
                variant="outlined"
                color="info"
                size="small"
                disabled={uploading}
                onClick={() => inputRef.current?.click()}
              >
                <Icon>upload_file</Icon>&nbsp;
                {uploading ? t('pod.uploading') : t('pod.attach')}
              </ArgonButton>
              <input ref={inputRef} type="file" multiple hidden onChange={pickFiles} />
            </ArgonBox>
            <ArgonBox mt={1} display="flex" flexDirection="column" gap={0.5}>
              {attachments.map((attachment) => (
                <ArgonBox
                  key={attachment.documentId}
                  display="flex"
                  alignItems="center"
                  gap={1}
                  flexWrap="wrap"
                >
                  <ArgonTypography variant="caption" fontWeight="medium">
                    {attachment.fileName}
                  </ArgonTypography>
                  <ArgonBadge
                    variant="gradient"
                    color={isCleanAttachment(attachment) ? 'success' : 'warning'}
                    size="xs"
                    container
                    badgeContent={t(
                      `pod.scanStatuses.${attachment.scanStatus}` as 'pod.scanStatuses.Clean',
                      { defaultValue: attachment.scanStatus }
                    )}
                  />
                  {!isCleanAttachment(attachment) && (
                    <ArgonButton
                      variant="text"
                      color="info"
                      size="small"
                      onClick={() => onRefreshAttachment(attachment.documentId)}
                    >
                      <Icon>refresh</Icon>&nbsp;{t('pod.recheck')}
                    </ArgonButton>
                  )}
                  <ArgonButton
                    variant="text"
                    color="error"
                    size="small"
                    onClick={() => onRemoveAttachment(attachment.documentId)}
                  >
                    <Icon>close</Icon>&nbsp;{t('generic.delete')}
                  </ArgonButton>
                </ArgonBox>
              ))}
            </ArgonBox>
            {/* The backend rejects the WHOLE capture when any attachment is not
                Clean, so the condition is surfaced before the round-trip too. */}
            {pending.length > 0 && (
              <ArgonTypography variant="caption" color="warning" fontWeight="medium" display="block">
                {t('pod.notCleanWarning', { count: pending.length })}
              </ArgonTypography>
            )}
          </Grid>
        </Grid>
      </form>
    </FormDialog>
  );
}

export default PodDialog;
