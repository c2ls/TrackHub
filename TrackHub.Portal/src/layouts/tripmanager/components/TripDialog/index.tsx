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
import Divider from '@mui/material/Divider';
import Grid from '@mui/material/Grid';
import Icon from '@mui/material/Icon';
import ArgonBox from 'components/ArgonBox';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import FormDialog from 'controls/Dialogs/FormDialog';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';
import type { Transporter } from 'api/manager/transporters';
import type { Driver } from 'api/manager/drivers';
import type { PointOfInterestLookup } from 'api/manager/pointsOfInterest';
import type { Geofence } from 'api/geofencing/geofencing';
import type { Trip, TollVehicleClass } from 'api/tripManagement/trips';
import {
  DEFAULT_ARRIVAL_RADIUS_METERS,
  DEFAULT_STOP_ACTIVITY,
  STOP_ACTIVITIES,
  TRIP_TYPES,
} from '../../tripWriteForms';
import type { StopActivity, TripDestinationDraft, TripType } from '../../tripWriteForms';

/** Dialog/form state for a trip. Strings throughout — the caller coerces on save. */
export interface TripFormValues {
  tripId?: string;
  code?: string;
  transporterId?: string;
  driverId?: string;
  serviceOrderId?: string | null;
  externalReference?: string | null;
  customerName?: string | null;
  originName?: string;
  /** Set by the origin place pickers, never typed (spec 11 §8's placement rule). */
  originLatitude?: number | string;
  originLongitude?: number | string;
  /**
   * The account geofence the origin was picked from. It IS a Trip column now: the
   * origin zone is what auto-start is measured against, so its real shape has to
   * survive to the server rather than degrading to a radius circle (spec 11a §4.1).
   * A round trip's auto-appended return stop reuses it too.
   */
  originGeofenceId?: string | null;
  /** Form-only: which POI the origin came from, so the picker shows it. */
  originPoiId?: string | null;
  /** Form-only: shapes the destination section; an existing trip re-derives it from its stops. */
  tripType?: TripType;
  plannedStartAt?: string;
  plannedEndAt?: string | null;
  notes?: string | null;
  tollVehicleClass?: string | null;
}

interface TripDialogProps {
  open: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
  handleSubmit: () => void | Promise<void>;
  values: TripFormValues;
  handleChange: FormChangeHandler;
  errors: Record<string, string>;
  transporters: Transporter[];
  drivers: Driver[];
  vehicleClasses: TollVehicleClass[];
  pois: PointOfInterestLookup[];
  geofences: Geofence[];
  /** Destinations queued for creation; stops are added right after the trip header. */
  destinations: TripDestinationDraft[];
  setDestinations: Dispatch<SetStateAction<TripDestinationDraft[]>>;
  /** Recent trips offered as reusable routes (create mode only). */
  copySources: Trip[];
  onCopyFrom: (tripId: string) => void;
}

/** Section header inside the dialog — the form is grouped, not a wall of fields. */
function SectionTitle({ title, hint }: { title: string; hint?: string }) {
  return (
    <Grid size={{ xs: 12 }}>
      <Divider sx={{ mt: 0.5, mb: 0.5 }} />
      <ArgonTypography variant="button" fontWeight="medium">
        {title}
      </ArgonTypography>
      {hint && (
        <ArgonTypography variant="caption" color="secondary" display="block">
          {hint}
        </ArgonTypography>
      )}
    </Grid>
  );
}

/**
 * Trip editor, place-based by design: the origin and every destination are an
 * existing named place — an account geofence (arrival detection then uses its
 * real shape) or a point of interest (point + default arrival radius). There
 * are NO coordinate fields and NO free map pin here; ad-hoc points and stop
 * fine-tuning (arrival windows, POD flags, reordering) belong to the route
 * planner, which has the map.
 *
 * Creation also queues the trip's destination(s), a trip-type selector
 * (single/round/multi) and a "reuse a previous route" shortcut, so a
 * dispatcher never builds the same route twice.
 */
function TripDialog({
  open,
  setOpen,
  handleSubmit,
  values,
  handleChange,
  errors,
  transporters,
  drivers,
  vehicleClasses,
  pois,
  geofences,
  destinations,
  setDestinations,
  copySources,
  onCopyFrom,
}: TripDialogProps) {
  const { t } = useTranslation();

  /**
   * What the next destination picked will be FOR. It is a picker-adjacent control,
   * not a per-row editor: a dispatcher queueing a delivery run sets Unload once and
   * adds five clients. Fine-tuning a single stop's activity belongs to the planner,
   * which edits a stop that already exists.
   */
  const [destinationActivity, setDestinationActivity] = useState<StopActivity>(DEFAULT_STOP_ACTIVITY);

  const creating = !values.tripId;
  const tripType: TripType = values.tripType ?? 'single';
  // A single-destination trip takes exactly one destination; the add controls
  // disable rather than silently replacing what was already queued.
  const destinationsFull = creating && tripType === 'single' && destinations.length >= 1;

  const originLatitude = Number(values.originLatitude);
  const originLongitude = Number(values.originLongitude);
  const hasOrigin =
    Number.isFinite(originLatitude) &&
    Number.isFinite(originLongitude) &&
    (originLatitude !== 0 || originLongitude !== 0);

  const transporterOptions = transporters.map((transporter) => ({
    value: transporter.transporterId,
    label: transporter.name,
  }));
  const driverOptions = drivers
    .filter((driver) => driver.active)
    .map((driver) => ({ value: driver.driverId, label: driver.name }));
  // Only active classes are offerable; an inactive class stays on historical
  // trips but must not be picked for a new one.
  const classOptions = vehicleClasses
    .filter((vehicleClass) => vehicleClass.active)
    .map((vehicleClass) => ({
      value: vehicleClass.code,
      label: `${vehicleClass.code} — ${vehicleClass.name}`,
    }));
  const poiOptions = pois
    .filter((poi) => poi.active !== false)
    .map((poi) => ({ value: poi.pointOfInterestId, label: poi.name }));
  const geofenceOptions = geofences
    .filter((geofence) => geofence.active)
    .map((geofence) => ({ value: geofence.geofenceId, label: geofence.name }));
  const typeOptions = TRIP_TYPES.map((type) => ({
    value: type,
    label: t(`trips.type.${type}` as 'trips.type.single'),
  }));
  const activityOptions = STOP_ACTIVITIES.map((activity) => ({
    value: activity,
    label: t(`tripStops.activity.${activity}` as 'tripStops.activity.Unload'),
  }));
  // Only trips that actually have a route are worth copying.
  const copyOptions = copySources
    .filter((trip) => trip.stopCount > 0)
    .map((trip) => ({
      value: trip.tripId,
      label: `${trip.code} — ${trip.originName} (${trip.stopCount})`,
    }));

  /** A circle carries its centre; a polygon's first vertex is the best available representative point. */
  const geofencePoint = (geofence: Geofence) =>
    geofence.circleCenter ?? geofence.geom?.coordinates?.[0];

  /**
   * The origin name FOLLOWS the selection — switching place always rewrites it,
   * so a previously picked geofence's name can never linger on a new point.
   * It stays an editable field for annotations ("Main plant — dock 3").
   */
  const applyOrigin = (
    lat: number,
    lng: number,
    name: string,
    geofenceId: string | null,
    poiId: string | null
  ) => {
    handleChange({ target: { name: 'originLatitude', value: lat.toFixed(6) } });
    handleChange({ target: { name: 'originLongitude', value: lng.toFixed(6) } });
    handleChange({ target: { name: 'originGeofenceId', value: geofenceId ?? '' } });
    handleChange({ target: { name: 'originPoiId', value: poiId ?? '' } });
    handleChange({ target: { name: 'originName', value: name } });
  };

  const handleOriginPoiPick: FormChangeHandler = (event) => {
    const poi = pois.find((candidate) => candidate.pointOfInterestId === event.target.value);
    if (poi) applyOrigin(poi.latitude, poi.longitude, poi.name, null, poi.pointOfInterestId);
  };

  const handleOriginGeofencePick: FormChangeHandler = (event) => {
    const geofence = geofences.find((candidate) => candidate.geofenceId === event.target.value);
    const point = geofence ? geofencePoint(geofence) : null;
    if (geofence && point) {
      applyOrigin(point.latitude, point.longitude, geofence.name, geofence.geofenceId, null);
    }
  };

  const addDestination = (destination: TripDestinationDraft) => {
    if (destinationsFull) return;
    setDestinations((previous) => [...previous, destination]);
  };

  const handleDestinationPoiPick: FormChangeHandler = (event) => {
    const poi = pois.find((candidate) => candidate.pointOfInterestId === event.target.value);
    if (!poi) return;
    addDestination({
      name: poi.name,
      latitude: poi.latitude,
      longitude: poi.longitude,
      geofenceId: null,
      arrivalRadiusMeters: DEFAULT_ARRIVAL_RADIUS_METERS,
      activity: destinationActivity,
      requiresPod: false,
      priority: 0,
    });
  };

  const handleDestinationGeofencePick: FormChangeHandler = (event) => {
    const geofence = geofences.find((candidate) => candidate.geofenceId === event.target.value);
    const point = geofence ? geofencePoint(geofence) : null;
    if (!geofence || !point) return;
    addDestination({
      name: geofence.name,
      latitude: point.latitude,
      longitude: point.longitude,
      geofenceId: geofence.geofenceId,
      arrivalRadiusMeters: DEFAULT_ARRIVAL_RADIUS_METERS,
      activity: destinationActivity,
      requiresPod: false,
      priority: 0,
    });
  };

  return (
    <FormDialog
      title={values.tripId ? t('trips.editTrip') : t('trips.newTrip')}
      handleSave={handleSubmit}
      open={open}
      setOpen={setOpen}
      maxWidth="md"
    >
      <form>
        <Grid container spacing={2}>
          {/* ----------------------------------------------------- details */}
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              autoFocus
              margin="dense"
              name="code"
              id="code"
              label={t('trips.code')}
              type="text"
              value={values.code || ''}
              onChange={handleChange}
              errorMsg={errors.code}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="customerName"
              id="customerName"
              label={t('trips.customerName')}
              type="text"
              value={values.customerName || ''}
              onChange={handleChange}
              errorMsg={errors.customerName}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomSelect
              list={transporterOptions}
              handleChange={handleChange}
              name="transporterId"
              id="transporterId"
              label={t('trips.transporter')}
              value={values.transporterId}
              numericValue={false}
              required
              errorMsg={errors.transporterId}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomSelect
              list={driverOptions}
              handleChange={handleChange}
              name="driverId"
              id="driverId"
              label={t('trips.driver')}
              value={values.driverId}
              numericValue={false}
              placeholder={t('trips.assignment.selectDriver')}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="plannedStartAt"
              id="plannedStartAt"
              label={t('trips.plannedStart')}
              type="datetime-local"
              slotProps={{ inputLabel: { shrink: true } }}
              value={values.plannedStartAt || ''}
              onChange={handleChange}
              errorMsg={errors.plannedStartAt}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="plannedEndAt"
              id="plannedEndAt"
              label={t('trips.plannedEnd')}
              type="datetime-local"
              slotProps={{ inputLabel: { shrink: true } }}
              value={values.plannedEndAt || ''}
              onChange={handleChange}
              errorMsg={errors.plannedEndAt}
            />
          </Grid>

          {/* ------------------------------------------------------- route */}
          <SectionTitle title={t('trips.route.title')} hint={t('trips.route.hint')} />
          {creating && copyOptions.length > 0 && (
            <Grid size={{ xs: 12 }}>
              <CustomSelect
                list={copyOptions}
                handleChange={(event) => onCopyFrom(String(event.target.value))}
                name="copyFrom"
                id="copyFrom"
                label={t('trips.copyFrom.label')}
                value=""
                numericValue={false}
                placeholder={t('trips.copyFrom.placeholder')}
              />
            </Grid>
          )}
          {creating && (
            <Grid size={{ xs: 12, sm: 4 }}>
              <CustomSelect
                list={typeOptions}
                handleChange={handleChange}
                name="tripType"
                id="tripType"
                label={t('trips.type.label')}
                value={tripType}
                numericValue={false}
              />
            </Grid>
          )}
          <Grid size={{ xs: 12, sm: creating ? 4 : 6 }}>
            <CustomSelect
              list={geofenceOptions}
              handleChange={handleOriginGeofencePick}
              name="originGeofencePick"
              id="originGeofencePick"
              label={t('trips.origin.byGeofence')}
              value={values.originGeofenceId ?? ''}
              numericValue={false}
              required={!values.originPoiId}
              placeholder={t('tripStops.placement.selectGeofence')}
              errorMsg={errors.origin}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: creating ? 4 : 6 }}>
            <CustomSelect
              list={poiOptions}
              handleChange={handleOriginPoiPick}
              name="originPoiPick"
              id="originPoiPick"
              label={t('trips.origin.byPoi')}
              value={values.originPoiId ?? ''}
              numericValue={false}
              placeholder={t('tripStops.placement.selectPoi')}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 8 }}>
            <CustomTextField
              margin="dense"
              name="originName"
              id="originName"
              label={t('trips.originName')}
              type="text"
              value={values.originName || ''}
              onChange={handleChange}
              errorMsg={errors.originName}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <ArgonBox mt={3.5}>
              <ArgonTypography variant="caption" color={hasOrigin ? 'success' : 'secondary'}>
                {hasOrigin
                  ? t('trips.origin.chosen', {
                      lat: originLatitude.toFixed(5),
                      lng: originLongitude.toFixed(5),
                    })
                  : t('trips.origin.none')}
              </ArgonTypography>
            </ArgonBox>
          </Grid>

          {/* ------------------------------------------------ destinations */}
          {creating && (
            <>
              <SectionTitle
                title={t('trips.destinations.title')}
                hint={t('trips.destinations.hint')}
              />
              <Grid size={{ xs: 12, sm: 4 }}>
                <CustomSelect
                  list={activityOptions}
                  handleChange={(event) =>
                    setDestinationActivity(String(event.target.value) as StopActivity)
                  }
                  name="destinationActivity"
                  id="destinationActivity"
                  label={t('tripStops.activity.label')}
                  value={destinationActivity}
                  numericValue={false}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <CustomSelect
                  list={destinationsFull ? [] : geofenceOptions}
                  handleChange={handleDestinationGeofencePick}
                  name="destinationGeofencePick"
                  id="destinationGeofencePick"
                  label={t('trips.destinations.byGeofence')}
                  value=""
                  numericValue={false}
                  placeholder={t('tripStops.placement.selectGeofence')}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 4 }}>
                <CustomSelect
                  list={destinationsFull ? [] : poiOptions}
                  handleChange={handleDestinationPoiPick}
                  name="destinationPoiPick"
                  id="destinationPoiPick"
                  label={t('trips.destinations.byPoi')}
                  value=""
                  numericValue={false}
                  placeholder={t('tripStops.placement.selectPoi')}
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                {destinations.map((destination, index) => (
                  <ArgonBox
                    key={`${destination.latitude},${destination.longitude},${index}`}
                    display="flex"
                    alignItems="center"
                    gap={1}
                  >
                    <ArgonTypography variant="caption" fontWeight="medium">
                      {index + 1}.
                    </ArgonTypography>
                    <Icon fontSize="small">
                      {destination.geofenceId ? 'layers' : 'place'}
                    </Icon>
                    <ArgonTypography variant="caption">{destination.name}</ArgonTypography>
                    <ArgonButton
                      variant="text"
                      color="error"
                      size="small"
                      iconOnly
                      onClick={() =>
                        setDestinations((previous) =>
                          previous.filter((_, position) => position !== index)
                        )
                      }
                      aria-label={t('trips.destinations.remove')}
                    >
                      <Icon>close</Icon>
                    </ArgonButton>
                  </ArgonBox>
                ))}
                {(destinations.length === 0 || errors.destinations) && (
                  <ArgonTypography
                    variant="caption"
                    color={errors.destinations ? 'error' : 'secondary'}
                    display="block"
                  >
                    {errors.destinations || t('trips.destinations.empty')}
                  </ArgonTypography>
                )}
                {tripType === 'round' && (
                  <ArgonTypography variant="caption" color="secondary" display="block">
                    {t('trips.destinations.returnNote')}
                  </ArgonTypography>
                )}
              </Grid>
            </>
          )}

          {/* ------------------------------------------------------ extras */}
          <SectionTitle title={t('trips.extras.title')} />
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomTextField
              margin="dense"
              name="externalReference"
              id="externalReference"
              label={t('trips.externalReference')}
              type="text"
              value={values.externalReference || ''}
              onChange={handleChange}
              errorMsg={errors.externalReference}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <CustomSelect
              list={classOptions}
              handleChange={handleChange}
              name="tollVehicleClass"
              id="tollVehicleClass"
              label={t('trips.tollVehicleClass')}
              value={values.tollVehicleClass ?? ''}
              numericValue={false}
              placeholder={t('tolls.selectVehicleClass')}
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <CustomTextField
              margin="dense"
              name="notes"
              id="notes"
              label={t('trips.notes')}
              type="text"
              multiline
              rows={2}
              value={values.notes || ''}
              onChange={handleChange}
              errorMsg={errors.notes}
            />
          </Grid>
        </Grid>
      </form>
    </FormDialog>
  );
}

export default TripDialog;
