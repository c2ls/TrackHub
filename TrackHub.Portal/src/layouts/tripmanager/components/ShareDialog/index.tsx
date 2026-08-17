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
import type { Dispatch, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import Icon from '@mui/material/Icon';
import FormDialog from 'controls/Dialogs/FormDialog';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import CustomCheckbox from 'controls/Dialogs/CustomCheckbox';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import ArgonBox from 'components/ArgonBox';
import ArgonBadge from 'components/ArgonBadge';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import { publicTripUrl } from 'api/tripManagement/publicTrips';
import { useShareTrip, useRevokeTripShare } from 'queries/trips';
import { formatDateTime } from 'utils/dateUtils';
import type { TripDetail } from 'api/tripManagement/trips';

interface ShareDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  detail: TripDetail;
}

/**
 * Exported so the test suite can pin it against the generated backend types:
 * a flag added to `TripShareFieldFlagsDtoInput` server-side must not be able to
 * land silently, because an unrendered flag is an undisclosed disclosure.
 */
export type FieldFlagName =
  | 'includeDriverName'
  | 'includeVehicle'
  | 'includeLivePosition'
  | 'includeStopDetail'
  | 'includePodSummary'
  | 'includeRoute';

export const FIELD_FLAGS: FieldFlagName[] = [
  'includeStopDetail',
  'includeRoute',
  'includeLivePosition',
  'includeVehicle',
  'includeDriverName',
  'includePodSummary',
];

/**
 * Every flag is a DISCLOSURE decision and therefore fails closed. `includeRoute`
 * in particular defaults to false, matching the backend: the planned route used
 * to be handed out unconditionally, so leaving it unticked is now the safe
 * default and ticking it is a deliberate act (spec 11 §7.8).
 */
export const DEFAULT_FLAGS: Record<FieldFlagName, boolean> = {
  includeDriverName: false,
  includeVehicle: false,
  includeLivePosition: true,
  includeStopDetail: true,
  includePodSummary: false,
  includeRoute: false,
};

/**
 * Creates and revokes customer tracking links.
 *
 * The plaintext token comes back exactly ONCE, at creation — the backend never
 * returns it again — so the freshly built URL is shown with a warning to copy
 * it now, and existing links are listed without any token at all.
 */
function ShareDialog({ open, setOpen, detail }: ShareDialogProps) {
  const { t } = useTranslation();
  const shareTrip = useShareTrip();
  const revokeShare = useRevokeTripShare();

  const [purpose, setPurpose] = useState('');
  const [expiresAt, setExpiresAt] = useState('');
  const [flags, setFlags] = useState<Record<FieldFlagName, boolean>>(DEFAULT_FLAGS);
  const [createdUrl, setCreatedUrl] = useState<string | null>(null);
  const [toRevoke, setToRevoke] = useState<string | null>(null);

  const reset = () => {
    setPurpose('');
    setExpiresAt('');
    setCreatedUrl(null);
    // Flags fail closed on every new share too — a previous, more generous
    // selection must never carry over silently into the next link.
    setFlags(DEFAULT_FLAGS);
  };

  const handleSave = async () => {
    if (createdUrl) {
      reset();
      setOpen(false);
      return;
    }
    if (!expiresAt) return;
    try {
      const share = await shareTrip.mutateAsync({
        tripId: detail.trip.tripId,
        expiresAt: new Date(expiresAt).toISOString(),
        purpose: purpose || 'Trip tracking',
        fieldFlags: flags,
      });
      // No token means the backend declined to disclose one; do not fabricate a URL.
      if (share.token) {
        setCreatedUrl(
          publicTripUrl(window.location.origin, {
            publicLinkGrantId: share.publicLinkGrantId,
            accountId: share.accountId,
            resourceId: share.tripId,
            token: share.token,
          })
        );
      } else {
        setOpen(false);
      }
    } catch {
      // Failure surfaces in the global toast; keep the dialog open to retry.
    }
  };

  const shareState = (share: TripDetail['shares'][number]) => {
    if (share.revokedAt) return { color: 'error' as const, label: t('tripShare.revoked') };
    if (new Date(share.expiresAt).getTime() < Date.now()) {
      return { color: 'secondary' as const, label: t('tripShare.expired') };
    }
    return { color: 'success' as const, label: t('tripShare.active') };
  };

  return (
    <>
      <FormDialog
        title={t('tripShare.title')}
        handleSave={handleSave}
        handleCancel={reset}
        open={open}
        setOpen={setOpen}
        maxWidth="md"
      >
        {createdUrl ? (
          <ArgonBox>
            <ArgonTypography variant="caption" color="error" fontWeight="medium">
              {t('tripShare.tokenWarning')}
            </ArgonTypography>
            <ArgonBox mt={2}>
              <CustomTextField
                margin="normal"
                name="shareUrl"
                id="shareUrl"
                label={t('tripShare.url')}
                type="text"
                multiline
                minRows={2}
                value={createdUrl}
                slotProps={{ htmlInput: { readOnly: true } }}
                onChange={() => undefined}
              />
            </ArgonBox>
          </ArgonBox>
        ) : (
          <form>
            <CustomTextField
              autoFocus
              margin="dense"
              name="purpose"
              id="purpose"
              label={t('tripShare.purpose')}
              type="text"
              value={purpose}
              onChange={(event) => setPurpose(event.target.value)}
            />
            <CustomTextField
              margin="normal"
              name="expiresAt"
              id="expiresAt"
              label={t('tripShare.expiresAt')}
              type="datetime-local"
              slotProps={{ inputLabel: { shrink: true } }}
              value={expiresAt}
              onChange={(event) => setExpiresAt(event.target.value)}
              required
            />
            <ArgonBox mt={2}>
              <ArgonTypography variant="button" fontWeight="medium">
                {t('tripShare.fields')}
              </ArgonTypography>
              <ArgonTypography variant="caption" color="secondary" display="block">
                {t('tripShare.fieldsHint')}
              </ArgonTypography>
              <ArgonBox display="flex" flexDirection="column">
                {FIELD_FLAGS.map((flag) => (
                  <CustomCheckbox
                    key={flag}
                    name={flag}
                    id={flag}
                    value={flags[flag]}
                    label={t(`tripShare.${flag}` as 'tripShare.includeDriverName')}
                    handleChange={(event) =>
                      setFlags((previous) => ({
                        ...previous,
                        [flag]: !!event.target.checked,
                      }))
                    }
                  />
                ))}
              </ArgonBox>
              {/* Say plainly what the customer will see either way, so an
                  unticked route is a choice rather than an accident. */}
              <ArgonTypography variant="caption" color="secondary" display="block">
                {flags.includeRoute
                  ? t('tripShare.routeIncludedHint')
                  : t('tripShare.routeExcludedHint')}
              </ArgonTypography>
            </ArgonBox>

            <ArgonBox mt={2}>
              <ArgonTypography variant="button" fontWeight="medium">
                {t('tripShare.existing')}
              </ArgonTypography>
              {detail.shares.length === 0 ? (
                <ArgonTypography variant="caption" color="secondary" display="block">
                  {t('tripShare.empty')}
                </ArgonTypography>
              ) : (
                detail.shares.map((share) => {
                  const state = shareState(share);
                  // What an EXISTING link actually discloses, read back from the
                  // stored share rather than from DEFAULT_FLAGS. A link outlives
                  // the dialog that made it and the defaults it was made under,
                  // so "revoke the ones that show the route" is only answerable
                  // if the list states each link's flags (spec 11 §7.8).
                  // Indexing the share by `FieldFlagName` also means dropping a
                  // flag from the GraphQL fragment stops compiling.
                  const granted = FIELD_FLAGS.filter((flag) => share[flag]);
                  return (
                    <ArgonBox key={share.tripShareId} py={0.5}>
                      <ArgonBox display="flex" alignItems="center" gap={1}>
                        <ArgonBadge
                          variant="gradient"
                          color={state.color}
                          size="xs"
                          container
                          badgeContent={state.label}
                        />
                        <ArgonTypography variant="caption" color="text">
                          {t('tripShare.expires')}: {formatDateTime(share.expiresAt)}
                        </ArgonTypography>
                        {!share.revokedAt && (
                          <ArgonButton
                            variant="text"
                            color="error"
                            size="small"
                            onClick={() => setToRevoke(share.tripShareId)}
                          >
                            <Icon>link_off</Icon>&nbsp;{t('tripShare.revoke')}
                          </ArgonButton>
                        )}
                      </ArgonBox>
                      <ArgonTypography variant="caption" color="secondary" display="block">
                        {t('tripShare.fields')}:{' '}
                        {granted.length === 0
                          ? t('tripShare.nothingShared')
                          : granted
                              .map((flag) => t(`tripShare.${flag}` as 'tripShare.includeDriverName'))
                              .join(' · ')}
                      </ArgonTypography>
                    </ArgonBox>
                  );
                })
              )}
            </ArgonBox>
          </form>
        )}
      </FormDialog>

      <ConfirmDialog
        title={t('tripShare.revokeTitle')}
        message={t('tripShare.revokeMessage')}
        open={!!toRevoke}
        setOpen={() => setToRevoke(null)}
        onConfirm={async () => {
          await revokeShare
            .mutateAsync({ tripId: detail.trip.tripId, tripShareId: toRevoke as string })
            .catch(() => undefined);
          setToRevoke(null);
        }}
      />
    </>
  );
}

export default ShareDialog;
