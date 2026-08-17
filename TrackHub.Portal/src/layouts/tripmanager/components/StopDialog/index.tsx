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

import { useEffect, useRef } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import Grid from '@mui/material/Grid';
import Icon from '@mui/material/Icon';
import ArgonButton from 'components/ArgonButton';
import FormDialog from 'controls/Dialogs/FormDialog';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import CustomCheckbox from 'controls/Dialogs/CustomCheckbox';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import { reverseGeocode } from 'api/router/router';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import type { PointOfInterestLookup } from 'api/manager/pointsOfInterest';
import type { Geofence } from 'api/geofencing/geofencing';
import { DEFAULT_STOP_ACTIVITY, STOP_ACTIVITIES } from '../../tripWriteForms';

/** Dialog/form state for a trip stop. */
export interface StopFormValues {
  tripStopId?: string;
  name?: string;
  address?: string | null;
  /**
   * Coarse locality. Deliberately a SEPARATE field from `address`: the anonymous
   * customer snapshot may carry the city but never the full street address, so
   * projecting `address` as the locality would leak every other stop's exact
   * delivery address to any link holder (spec 11 §7.8).
   */
  city?: string | null;
  latitude?: number | string;
  longitude?: number | string;
  geofenceId?: string | null;
  arrivalRadiusMeters?: number | string;
  /**
   * What the vehicle does here — `Load`, `Unload` or `Other`. It is what turns the
   * dwell between arrival and departure from an anonymous number of minutes into
   * loading or unloading time in the reports (spec 11a §4.2).
   */
  activity?: string | null;
  plannedArrivalFrom?: string | null;
  plannedArrivalTo?: string | null;
  requiresPod?: boolean;
  priority?: number | string;
  observations?: string | null;
}

interface StopDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  handleCancel?: () => void;
  values: StopFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
  pois: PointOfInterestLookup[];
  geofences: Geofence[];
  /** Puts the planner map into stop-placement mode and closes this dialog until a click lands. */
  onPlaceOnMap: () => void;
}

/**
 * Stop editor. A stop's coordinates come from exactly three places — a map
 * click, an existing point of interest, or an account geofence. There is
 * deliberately NO free-text address search: forward geocoding is out of scope
 * (spec 11 §18.8), and all three sources already yield coordinates. The address
 * field is filled by the REVERSE geocoder from whichever point was chosen, and
 * stays editable.
 */
function StopDialog({
  open,
  setOpen,
  handleSubmit,
  handleCancel,
  values,
  handleChange,
  errors,
  pois,
  geofences,
  onPlaceOnMap,
}: StopDialogProps) {
  const { t } = useTranslation();

  const latitude = Number(values.latitude);
  const longitude = Number(values.longitude);
  const hasPoint =
    Number.isFinite(latitude) && Number.isFinite(longitude) && (latitude !== 0 || longitude !== 0);

  // Reverse-geocode the chosen point into the display address AND the coarse
  // locality. Keyed on the coordinate pair so the same point is never resolved
  // twice, and never overwrites what the dispatcher typed. Both fields come
  // from the SAME Router response — the city is read from `AddressVm.city`, not
  // parsed out of the street label — and the resolve runs whenever EITHER is
  // still missing, so editing an older stop that has an address but no city
  // fills the gap instead of saving it back empty.
  const resolvedFor = useRef<string | null>(null);
  useEffect(() => {
    if (!open || !hasPoint) return;
    const key = `${latitude},${longitude}`;
    if (resolvedFor.current === key) return;
    resolvedFor.current = key;
    if (values.address && values.city) return;
    let cancelled = false;
    reverseGeocode(latitude, longitude, null)
      .then((result) => {
        if (cancelled || !result) return;
        if (result.address && !values.address) {
          handleChange({ target: { name: 'address', value: result.address } });
        }
        if (result.city && !values.city) {
          handleChange({ target: { name: 'city', value: result.city } });
        }
      })
      // Address enrichment is a convenience: a geocoder outage must not block
      // placing a stop, so the failure is deliberately silent (the Router
      // reverseGeocode callers do the same).
      .catch(() => undefined);
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, hasPoint, latitude, longitude]);

  const poiOptions = pois
    .filter((poi) => poi.active !== false)
    .map((poi) => ({ value: poi.pointOfInterestId, label: poi.name }));

  const geofenceOptions = geofences
    .filter((geofence) => geofence.active)
    .map((geofence) => ({ value: geofence.geofenceId, label: geofence.name }));

  const applyPoint = (lat: number, lng: number, name?: string, geofenceId?: string | null) => {
    handleChange({ target: { name: 'latitude', value: lat.toFixed(6) } });
    handleChange({ target: { name: 'longitude', value: lng.toFixed(6) } });
    handleChange({ target: { name: 'geofenceId', value: geofenceId ?? '' } });
    // A fresh point invalidates the previously resolved address AND locality.
    handleChange({ target: { name: 'address', value: '' } });
    handleChange({ target: { name: 'city', value: '' } });
    resolvedFor.current = null;
    if (name && !values.name) handleChange({ target: { name: 'name', value: name } });
  };

  const handlePoiPick: FormChangeHandler = (event) => {
    const poi = pois.find((candidate) => candidate.pointOfInterestId === event.target.value);
    if (poi) applyPoint(poi.latitude, poi.longitude, poi.name, null);
  };

  const handleGeofencePick: FormChangeHandler = (event) => {
    const geofence = geofences.find((candidate) => candidate.geofenceId === event.target.value);
    if (!geofence) return;
    // A circle carries its centre; a polygon's first vertex is the best
    // available representative point. The stop keeps the geofenceId either way,
    // so arrival detection snapshots the real shape at trip start.
    const point = geofence.circleCenter ?? geofence.geom?.coordinates?.[0];
    if (!point) return;
    applyPoint(point.latitude, point.longitude, geofence.name, geofence.geofenceId);
  };

  return (
    <FormDialog
      title={values.tripStopId ? t('tripStops.edit') : t('tripStops.add')}
      handleSave={handleSubmit}
      handleCancel={handleCancel}
      open={open}
      setOpen={setOpen}
      maxWidth="md"
    >
      <form>
        <ArgonBox mb={1}>
          <ArgonTypography variant="button" fontWeight="medium">
            {t('tripStops.placement.title')}
          </ArgonTypography>
          <ArgonTypography variant="caption" color="secondary" display="block">
            {t('tripStops.placement.noSearch')}
          </ArgonTypography>
        </ArgonBox>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 4 }}>
            <ArgonBox mt={1}>
              <ArgonButton variant="outlined" color="info" fullWidth onClick={onPlaceOnMap}>
                <Icon>add_location_alt</Icon>&nbsp;{t('tripStops.placement.byMap')}
              </ArgonButton>
            </ArgonBox>
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <CustomSelect
              list={poiOptions}
              handleChange={handlePoiPick}
              name="poiPick"
              id="poiPick"
              label={t('tripStops.placement.byPoi')}
              numericValue={false}
              placeholder={t('tripStops.placement.selectPoi')}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <CustomSelect
              list={geofenceOptions}
              handleChange={handleGeofencePick}
              name="geofencePick"
              id="geofencePick"
              label={t('tripStops.placement.byGeofence')}
              value={values.geofenceId ?? ''}
              numericValue={false}
              placeholder={t('tripStops.placement.selectGeofence')}
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <ArgonTypography variant="caption" color={hasPoint ? 'success' : 'secondary'}>
              {hasPoint
                ? t('tripStops.placement.chosen', {
                    lat: latitude.toFixed(5),
                    lng: longitude.toFixed(5),
                  })
                : t('tripStops.placement.none')}
            </ArgonTypography>
          </Grid>

          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="name"
              id="name"
              label={t('tripStops.name')}
              type="text"
              value={values.name || ''}
              onChange={handleChange}
              errorMsg={errors.name}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="address"
              id="address"
              label={t('tripStops.address')}
              type="text"
              value={values.address || ''}
              onChange={handleChange}
              errorMsg={errors.address}
              helperText={t('tripStops.addressHint')}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            {/* The ONLY location detail a shared tracking link may show. The
                full address above stays internal — see spec 11 §7.8. */}
            <CustomTextField
              margin="dense"
              name="city"
              id="city"
              label={t('tripStops.city')}
              type="text"
              value={values.city || ''}
              onChange={handleChange}
              errorMsg={errors.city}
              helperText={t('tripStops.cityHint')}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <CustomTextField
              margin="dense"
              name="latitude"
              id="latitude"
              label={t('tripStops.latitude')}
              type="number"
              value={values.latitude ?? ''}
              onChange={handleChange}
              errorMsg={errors.latitude}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <CustomTextField
              margin="dense"
              name="longitude"
              id="longitude"
              label={t('tripStops.longitude')}
              type="number"
              value={values.longitude ?? ''}
              onChange={handleChange}
              errorMsg={errors.longitude}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <CustomTextField
              margin="dense"
              name="arrivalRadiusMeters"
              id="arrivalRadiusMeters"
              label={t('tripStops.arrivalRadius')}
              type="number"
              value={values.arrivalRadiusMeters ?? 150}
              onChange={handleChange}
              errorMsg={errors.arrivalRadiusMeters}
              helperText={t('tripStops.arrivalRadiusHint')}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <CustomSelect
              list={STOP_ACTIVITIES.map((activity) => ({
                value: activity,
                label: t(`tripStops.activity.${activity}` as 'tripStops.activity.Unload'),
              }))}
              handleChange={handleChange}
              name="activity"
              id="activity"
              label={t('tripStops.activity.label')}
              value={values.activity ?? DEFAULT_STOP_ACTIVITY}
              numericValue={false}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="plannedArrivalFrom"
              id="plannedArrivalFrom"
              label={t('tripStops.plannedFrom')}
              type="datetime-local"
              slotProps={{ inputLabel: { shrink: true } }}
              value={values.plannedArrivalFrom || ''}
              onChange={handleChange}
              errorMsg={errors.plannedArrivalFrom}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="plannedArrivalTo"
              id="plannedArrivalTo"
              label={t('tripStops.plannedTo')}
              type="datetime-local"
              slotProps={{ inputLabel: { shrink: true } }}
              value={values.plannedArrivalTo || ''}
              onChange={handleChange}
              errorMsg={errors.plannedArrivalTo}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <CustomTextField
              margin="dense"
              name="priority"
              id="priority"
              label={t('tripStops.priority')}
              type="number"
              value={values.priority ?? 0}
              onChange={handleChange}
              errorMsg={errors.priority}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 8 }}>
            <ArgonBox mt={2}>
              <CustomCheckbox
                handleChange={handleChange}
                name="requiresPod"
                id="requiresPod"
                value={!!values.requiresPod}
                label={t('tripStops.requiresPod')}
              />
            </ArgonBox>
          </Grid>
          <Grid size={{ xs: 12 }}>
            <CustomTextField
              margin="dense"
              name="observations"
              id="observations"
              label={t('tripStops.observations')}
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

export default StopDialog;
