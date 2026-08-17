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

/**
 * Form state and payload builders for the trip manager's write surfaces
 * (deliveries, proof of delivery, transporter → toll class).
 *
 * These are deliberately pure and separate from the dialogs: the payload shapes
 * are contract-critical — an omitted `clientEventId`, a blank string sent where
 * the backend expects null, or a stale coordinate silently changes what the
 * server records — so they are unit-tested directly rather than only through a
 * rendered dialog.
 */

import { toDateTimeLocalInput, fromDateTimeLocalInput } from 'utils/dateUtils';
import type {
  DeliveryDtoInput,
  ProofOfDeliveryDtoInput,
  TripStopDtoInput,
} from 'api/tripManagement/trips';

/**
 * `datetime-local` needs `YYYY-MM-DDTHH:mm` local wall time; the API speaks ISO-8601 UTC.
 * Both directions delegate to the shared implementation in `utils/dateUtils`, so the trip planned
 * times that TripDelayed and TripStartDue are evaluated against round-trip unchanged in any timezone.
 */
export const toLocalInput = (iso?: string | null): string => toDateTimeLocalInput(iso);

export const toIso = (local?: string | null): string | null => fromDateTimeLocalInput(local);

/** RFC 4122 id used as an idempotency key by every progress/POD/outcome command. */
export const newClientEventId = (): string => crypto.randomUUID();

/** Empty, blank and unparseable inputs all mean "not supplied", never 0. */
const toOptionalNumber = (value?: number | string | null): number | null => {
  if (value === null || value === undefined || String(value).trim() === '') return null;
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
};

const trimmedOrNull = (value?: string | null): string | null => {
  const trimmed = (value ?? '').trim();
  return trimmed === '' ? null : trimmed;
};

/* ------------------------------------------------------------------ stops */

/**
 * `TripStop.City` is capped at 200 chars by the backend validator — mirrored
 * here so an over-long locality is caught in the dialog instead of coming back
 * as a 400.
 *
 * City is a SEPARATE field from `Address` on purpose: the anonymous customer
 * snapshot may carry the coarse locality but never the full street address, so
 * the two carry different disclosure levels and must never be derived from one
 * another (spec 11 §7.8).
 */
export const STOP_CITY_MAX_LENGTH = 200;

export const normalizeStopCity = (city?: string | null): string | null => {
  const trimmed = (city ?? '').trim();
  return trimmed === '' ? null : trimmed;
};

export const isStopCityWithinLimit = (city?: string | null): boolean =>
  (normalizeStopCity(city) ?? '').length <= STOP_CITY_MAX_LENGTH;

/* ------------------------------------------- trip creation: destinations  */

/**
 * How the dispatcher shapes a new trip. This is a FORM concept, not a stored
 * column: the backend derives nothing from it, and an existing trip's type is
 * re-derived from its stops (see {@link deriveTripType}). `round` appends a
 * return stop at the origin when the trip is created.
 */
export const TRIP_TYPES = ['single', 'round', 'multi'] as const;

export type TripType = (typeof TRIP_TYPES)[number];

/**
 * A destination queued in the create-trip dialog before the trip exists.
 * Coordinates always come from a picker (map click, POI, geofence) — never
 * typed by hand (spec 11 §8). Address/city are left for the reverse geocoder
 * when the stop is later edited in the planner.
 */
export interface TripDestinationDraft {
  name: string;
  latitude: number;
  longitude: number;
  /** Kept so arrival detection snapshots the real geofence shape at arming. */
  geofenceId: string | null;
  address?: string | null;
  city?: string | null;
  arrivalRadiusMeters: number;
  /** What the vehicle does here — it is what gives this stop's dwell a meaning. */
  activity: StopActivity;
  requiresPod: boolean;
  priority: number;
}

export const DEFAULT_ARRIVAL_RADIUS_METERS = 150;

/**
 * What a stop is FOR (spec 11a §4.2). Without it a dwell figure is an anonymous
 * number of minutes; with it the same measurement reads as loading time at a plant
 * and unloading time at a client.
 */
export const STOP_ACTIVITIES = ['Load', 'Unload', 'Other'] as const;

export type StopActivity = (typeof STOP_ACTIVITIES)[number];

/** A delivery run is the overwhelmingly common case, so a destination unloads by default. */
export const DEFAULT_STOP_ACTIVITY: StopActivity = 'Unload';

export const normalizeStopActivity = (value?: string | null): StopActivity =>
  STOP_ACTIVITIES.find((activity) => activity === value) ?? DEFAULT_STOP_ACTIVITY;

/** Builds the AddTripStop payload for a queued destination. */
export function buildStopPayloadFromDestination(
  destination: TripDestinationDraft
): TripStopDtoInput {
  return {
    name: destination.name.trim(),
    address: trimmedOrNull(destination.address),
    city: normalizeStopCity(destination.city),
    latitude: destination.latitude,
    longitude: destination.longitude,
    geofenceId: destination.geofenceId || null,
    arrivalRadiusMeters: destination.arrivalRadiusMeters || DEFAULT_ARRIVAL_RADIUS_METERS,
    activity: normalizeStopActivity(destination.activity),
    plannedArrivalFrom: null,
    plannedArrivalTo: null,
    requiresPod: destination.requiresPod,
    priority: destination.priority,
  };
}

/**
 * The auto-appended final stop of a round trip: back to the origin. When the
 * origin was picked from a geofence its id travels along, so arrival detection
 * uses the real shape instead of the fallback radius.
 */
export function returnToOriginStop(
  originName: string,
  latitude: number,
  longitude: number,
  geofenceId?: string | null
): TripDestinationDraft {
  return {
    name: originName,
    latitude,
    longitude,
    geofenceId: geofenceId || null,
    arrivalRadiusMeters: DEFAULT_ARRIVAL_RADIUS_METERS,
    // `Other`, not `Unload`: parking back at the depot is neither loading nor
    // unloading, and labelling it either would put a fictional duration into the
    // dwell reports.
    activity: 'Other',
    requiresPod: false,
    priority: 0,
  };
}

/**
 * Two points are "the same spot" within ~11 m — wide enough to survive the
 * 6-decimal rounding pickers apply, narrow enough that a genuine nearby
 * destination is never mistaken for the origin.
 */
const SAME_SPOT_EPSILON_DEGREES = 1e-4;

export const isSameSpot = (
  latitudeA: number,
  longitudeA: number,
  latitudeB: number,
  longitudeB: number
): boolean =>
  Math.abs(latitudeA - latitudeB) <= SAME_SPOT_EPSILON_DEGREES &&
  Math.abs(longitudeA - longitudeB) <= SAME_SPOT_EPSILON_DEGREES;

/** The stop fields route reuse and type derivation read — structural so tests and Vm rows both fit. */
export interface RouteSourceStop {
  sequence: number;
  name: string;
  address?: string | null;
  city?: string | null;
  latitude: number;
  longitude: number;
  geofenceId?: string | null;
  arrivalRadiusMeters: number;
  activity?: string | null;
  requiresPod: boolean;
  priority: number;
}

/** An existing trip's shape, re-derived from its stops rather than stored. */
export function deriveTripType(
  originLatitude: number,
  originLongitude: number,
  stops: RouteSourceStop[]
): TripType {
  if (stops.length === 0) return 'single';
  const ordered = [...stops].sort((a, b) => a.sequence - b.sequence);
  const last = ordered[ordered.length - 1];
  // A one-stop trip ending at the origin is a round trip with no outbound
  // destination — nonsensical, so it only counts as round past one stop.
  if (ordered.length > 1 && isSameSpot(last.latitude, last.longitude, originLatitude, originLongitude)) {
    return 'round';
  }
  return ordered.length > 1 ? 'multi' : 'single';
}

export interface RouteTemplate {
  tripType: TripType;
  destinations: TripDestinationDraft[];
  /** The source trip's return-stop geofence, if its route was a round trip. */
  originGeofenceId: string | null;
}

/**
 * Turns an existing trip's stops into a reusable route for the create dialog
 * (slice 2's "recurring trip templates", in its light form). A detected round
 * trip drops the trailing return stop — the dialog re-appends it at save time —
 * so the copied route passes through the form without duplicating the origin
 * stop.
 */
export function destinationsFromStops(
  stops: RouteSourceStop[],
  originLatitude: number,
  originLongitude: number
): RouteTemplate {
  const ordered = [...stops].sort((a, b) => a.sequence - b.sequence);
  const tripType = deriveTripType(originLatitude, originLongitude, ordered);
  const returnStop = tripType === 'round' ? ordered[ordered.length - 1] : null;
  const outbound = returnStop ? ordered.slice(0, -1) : ordered;
  return {
    tripType,
    originGeofenceId: returnStop?.geofenceId || null,
    destinations: outbound.map((stop) => ({
      name: stop.name,
      latitude: stop.latitude,
      longitude: stop.longitude,
      geofenceId: stop.geofenceId || null,
      address: stop.address ?? null,
      city: stop.city ?? null,
      arrivalRadiusMeters: stop.arrivalRadiusMeters || DEFAULT_ARRIVAL_RADIUS_METERS,
      activity: normalizeStopActivity(stop.activity),
      requiresPod: stop.requiresPod,
      priority: stop.priority,
    })),
  };
}

/* ------------------------------------------------- board: phase & exceptions */

/**
 * The exception filters the board leads with (spec 11a §10). Dispatcher attention
 * is exception-driven now: the system runs the ordinary lifecycle, so what a human
 * needs is the short list of trips that are NOT going to plan.
 */
export const TRIP_EXCEPTIONS = ['overdue', 'delayed', 'offCorridor', 'stalledFinalStop'] as const;

export type TripException = (typeof TRIP_EXCEPTIONS)[number];

/** The trip fields an exception is judged from — structural, so tests and Vm rows both fit. */
export interface ExceptionCandidateTrip {
  phase: string;
  status: string;
  deviationOpenedAt?: string | null;
  pendingStopCount?: number | null;
  phaseDelayed?: boolean | null;
}

/**
 * Whether a trip is currently showing the named exception.
 *
 * Each answer comes from a recorded fact rather than a stored flag: `Overdue` is
 * the derived phase, off-corridor is an open deviation episode, "delayed" is an
 * ETA already past the planned end, and a stalled final stop is a running trip
 * the board can see has nowhere left to go.
 */
export function hasException(trip: ExceptionCandidateTrip, exception: TripException): boolean {
  switch (exception) {
    case 'overdue':
      return trip.phase === 'Overdue';
    case 'offCorridor':
      return !!trip.deviationOpenedAt;
    case 'delayed':
      // The backend's answer, not a second opinion. This used to compare the next stop's
      // ETA against the TRIP's planned end — a looser rule than the one that raises
      // TripDelayed (that stop's own window plus `delayThresholdMinutes`), so the board
      // and the alert disagreed about which trips were late, on the same screen.
      return !!trip.phaseDelayed;
    case 'stalledFinalStop':
      // Running, standing at a stop, and nothing left on the route: the truck arrived
      // at its last destination and never measurably departed, so auto-completion has
      // nothing to close on.
      //
      // The discriminator is `pendingStopCount`, not `!phaseEtaAt`. The resolver never
      // sets an ETA on the AtStop branch — an ETA to a stop you are already parked at is
      // meaningless — so that test was always true and the filter matched every truck
      // unloading anywhere. An exception list that returns the whole board is noise, and
      // noise is what the dispatcher was promised relief from.
      return trip.status === 'InProgress' && trip.phase === 'AtStop' && trip.pendingStopCount === 0;
    default:
      return false;
  }
}

/* ------------------------------------------------------------- deliveries */

/** Outcomes accepted by `updateDeliveryOutcome` (TripManagement `DeliveryStatuses`). */
export const DELIVERY_STATUSES = [
  'Pending',
  'Delivered',
  'PartiallyDelivered',
  'Rejected',
] as const;

export type DeliveryStatus = (typeof DELIVERY_STATUSES)[number];

/** Dialog/form state for a delivery line on a stop. */
export interface DeliveryFormValues {
  deliveryId?: string;
  tripStopId?: string;
  reference?: string | null;
  clientName?: string;
  branchName?: string | null;
  productsSummary?: string | null;
  observations?: string | null;
  sequenceIndex?: number | string;
}

export const DELIVERY_REQUIRED_FIELDS = ['clientName'];

export function buildDeliveryPayload(values: DeliveryFormValues): DeliveryDtoInput {
  return {
    reference: trimmedOrNull(values.reference),
    clientName: (values.clientName ?? '').trim(),
    branchName: trimmedOrNull(values.branchName),
    productsSummary: trimmedOrNull(values.productsSummary),
    observations: trimmedOrNull(values.observations),
    sequenceIndex: toOptionalNumber(values.sequenceIndex) ?? 0,
  };
}

/* ---------------------------------------------------- proof of delivery   */

/** Scan verdict the backend requires before a document may back a POD. */
export const CLEAN_SCAN_STATUS = 'Clean';

/**
 * One uploaded document queued for a POD. `scanStatus` is carried because the
 * backend rejects the whole capture with `POD_DOCUMENT_NOT_CLEAN` if any
 * attachment has not finished scanning clean — the screen must be able to say
 * which one before the user submits (spec 11 §9).
 */
export interface PodAttachment {
  documentId: string;
  fileName: string;
  scanStatus: string;
}

export const isCleanAttachment = (attachment: PodAttachment): boolean =>
  attachment.scanStatus?.toLowerCase() === CLEAN_SCAN_STATUS.toLowerCase();

/** Dialog/form state for a proof-of-delivery capture. */
export interface PodFormValues {
  tripStopId?: string;
  deliveryId?: string | null;
  receiverName?: string;
  receiverDocument?: string | null;
  notes?: string | null;
  capturedAt?: string | null;
  latitude?: number | string | null;
  longitude?: number | string | null;
}

export const POD_REQUIRED_FIELDS = ['receiverName'];

/**
 * Builds the POD command payload.
 *
 * `clientEventId` is supplied by the caller rather than generated here: the
 * backend is idempotent on `(tripStopId, clientEventId)`, so a retry of the
 * SAME capture attempt has to reuse the id it already sent. Generating one
 * inside this function would mint a fresh id on every retry and turn a
 * duplicate submission into a duplicate POD row (spec 11 §7.3, §9).
 */
export function buildPodPayload(
  values: PodFormValues,
  attachments: PodAttachment[],
  clientEventId: string,
  now: () => string = () => new Date().toISOString()
): ProofOfDeliveryDtoInput {
  return {
    tripStopId: values.tripStopId ?? '',
    deliveryId: values.deliveryId || null,
    receiverName: (values.receiverName ?? '').trim(),
    receiverDocument: trimmedOrNull(values.receiverDocument),
    notes: trimmedOrNull(values.notes),
    capturedAt: toIso(values.capturedAt) ?? now(),
    latitude: toOptionalNumber(values.latitude),
    longitude: toOptionalNumber(values.longitude),
    documentIds: attachments.map((attachment) => attachment.documentId),
    clientEventId,
  };
}

/**
 * Upload metadata for a POD attachment. Trip documents are owned by the trip's
 * TRANSPORTER, not by the trip: `DocumentAccessPolicy` resolves transporter
 * ownership through group visibility for portal users and through
 * `validateDriverAssignment` for drivers, which is exactly the audience a POD
 * should have. A `"Trip"` owner type was rejected because Manager cannot
 * resolve trip ownership without calling TripManagement (spec 11 §11).
 */
export function podDocumentFields(
  accountId: string,
  transporterId: string,
  fileName: string
): Record<string, string> {
  return {
    accountId,
    ownerEntityType: 'Transporter',
    ownerEntityId: transporterId,
    category: 'Pod',
    classification: 'Internal',
    title: fileName,
  };
}

/* -------------------------------------------- transporter → toll class    */

/** A mapping keys on a transporter TYPE, or on one transporter as an override. */
export const TOLL_CLASS_TARGETS = ['transporterType', 'transporter'] as const;

export type TollClassTarget = (typeof TOLL_CLASS_TARGETS)[number];

export interface TollClassFormValues {
  target?: TollClassTarget;
  transporterTypeId?: number | string | null;
  transporterId?: string | null;
  tollVehicleClassCode?: string;
}

export interface TollClassVariables {
  transporterTypeId: number | null;
  transporterId: string | null;
  tollVehicleClassCode: string;
}

/**
 * Exactly one of the two keys travels. Sending both would make the row's unique
 * `(AccountId, TransporterTypeId, TransporterId)` key ambiguous, and sending
 * neither is rejected by the command's validator.
 */
export function buildTollClassVariables(values: TollClassFormValues): TollClassVariables | null {
  const code = (values.tollVehicleClassCode ?? '').trim();
  if (code === '') return null;
  if (values.target === 'transporter') {
    const transporterId = trimmedOrNull(values.transporterId);
    return transporterId === null
      ? null
      : { transporterTypeId: null, transporterId, tollVehicleClassCode: code };
  }
  const transporterTypeId = toOptionalNumber(values.transporterTypeId);
  return transporterTypeId === null
    ? null
    : { transporterTypeId, transporterId: null, tollVehicleClassCode: code };
}
