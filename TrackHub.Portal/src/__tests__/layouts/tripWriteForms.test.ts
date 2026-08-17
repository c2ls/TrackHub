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

import {
  buildDeliveryPayload,
  buildPodPayload,
  buildStopPayloadFromDestination,
  buildTollClassVariables,
  deriveTripType,
  destinationsFromStops,
  isCleanAttachment,
  podDocumentFields,
  returnToOriginStop,
  toIso,
  toLocalInput,
  normalizeStopCity,
  isStopCityWithinLimit,
  normalizeStopActivity,
  hasException,
  DEFAULT_ARRIVAL_RADIUS_METERS,
  DELIVERY_STATUSES,
  STOP_CITY_MAX_LENGTH,
} from 'layouts/tripmanager/tripWriteForms';
import type {
  PodAttachment,
  RouteSourceStop,
  TripDestinationDraft,
} from 'layouts/tripmanager/tripWriteForms';

const STOP_ID = '11111111-1111-1111-1111-111111111111';
const DOC_ID = '22222222-2222-2222-2222-222222222222';
const EVENT_ID = '33333333-3333-3333-3333-333333333333';
const TRANSPORTER_ID = '44444444-4444-4444-4444-444444444444';
const ACCOUNT_ID = '55555555-5555-5555-5555-555555555555';

const clean = (overrides: Partial<PodAttachment> = {}): PodAttachment => ({
  documentId: DOC_ID,
  fileName: 'signature.png',
  scanStatus: 'Clean',
  ...overrides,
});

describe('stop city', () => {
  test('a blank city is null, so nothing is written as an empty locality', () => {
    expect(normalizeStopCity('   ')).toBeNull();
    expect(normalizeStopCity(null)).toBeNull();
    expect(normalizeStopCity(undefined)).toBeNull();
  });

  test('a supplied city is trimmed', () => {
    expect(normalizeStopCity('  Bogotá  ')).toBe('Bogotá');
  });

  test('mirrors the backend 200-char cap', () => {
    expect(STOP_CITY_MAX_LENGTH).toBe(200);
    expect(isStopCityWithinLimit('Bogotá')).toBe(true);
    expect(isStopCityWithinLimit('x'.repeat(200))).toBe(true);
    expect(isStopCityWithinLimit('x'.repeat(201))).toBe(false);
    // Trailing whitespace is trimmed before the length is judged.
    expect(isStopCityWithinLimit(`${'x'.repeat(200)}   `)).toBe(true);
  });

  test('an absent city is within the limit rather than a validation failure', () => {
    expect(isStopCityWithinLimit(null)).toBe(true);
  });
});

describe('buildDeliveryPayload', () => {
  test('blank optional fields become null rather than empty strings', () => {
    const payload = buildDeliveryPayload({
      clientName: '  Acme  ',
      reference: '   ',
      branchName: '',
      productsSummary: null,
      observations: undefined,
      sequenceIndex: '',
    });

    expect(payload).toEqual({
      reference: null,
      clientName: 'Acme',
      branchName: null,
      productsSummary: null,
      observations: null,
      sequenceIndex: 0,
    });
  });

  test('a supplied sequence index survives as a number', () => {
    expect(buildDeliveryPayload({ clientName: 'Acme', sequenceIndex: '3' }).sequenceIndex).toBe(3);
  });

  test('the outcome vocabulary matches the backend DeliveryStatuses', () => {
    expect([...DELIVERY_STATUSES]).toEqual([
      'Pending',
      'Delivered',
      'PartiallyDelivered',
      'Rejected',
    ]);
  });
});

describe('buildPodPayload', () => {
  test('carries the caller-supplied clientEventId verbatim', () => {
    // The backend is idempotent on (tripStopId, clientEventId). Generating an id
    // inside the builder would mint a fresh one on every retry and turn a
    // duplicate submission into a duplicate POD row.
    const first = buildPodPayload({ tripStopId: STOP_ID, receiverName: 'Ana' }, [], EVENT_ID);
    const retry = buildPodPayload({ tripStopId: STOP_ID, receiverName: 'Ana' }, [], EVENT_ID);

    expect(first.clientEventId).toBe(EVENT_ID);
    expect(retry.clientEventId).toBe(EVENT_ID);
  });

  test('an unset delivery becomes null so the POD closes the whole stop', () => {
    const payload = buildPodPayload(
      { tripStopId: STOP_ID, receiverName: 'Ana', deliveryId: '' },
      [],
      EVENT_ID
    );

    expect(payload.deliveryId).toBeNull();
  });

  test('empty coordinates are null, never zero', () => {
    const payload = buildPodPayload(
      { tripStopId: STOP_ID, receiverName: 'Ana', latitude: '', longitude: '   ' },
      [],
      EVENT_ID
    );

    expect(payload.latitude).toBeNull();
    expect(payload.longitude).toBeNull();
  });

  test('supplied coordinates are numbers', () => {
    const payload = buildPodPayload(
      { tripStopId: STOP_ID, receiverName: 'Ana', latitude: '4.65', longitude: '-74.05' },
      [],
      EVENT_ID
    );

    expect(payload.latitude).toBe(4.65);
    expect(payload.longitude).toBe(-74.05);
  });

  test('a blank capturedAt falls back to now instead of sending an empty timestamp', () => {
    const payload = buildPodPayload({ tripStopId: STOP_ID, receiverName: 'Ana' }, [], EVENT_ID, () =>
      '2026-07-21T10:00:00.000Z'
    );

    expect(payload.capturedAt).toBe('2026-07-21T10:00:00.000Z');
  });

  test('a local capturedAt is converted to ISO-8601', () => {
    const payload = buildPodPayload(
      { tripStopId: STOP_ID, receiverName: 'Ana', capturedAt: '2026-07-21T08:30' },
      [],
      EVENT_ID
    );

    expect(payload.capturedAt).toBe(new Date('2026-07-21T08:30').toISOString());
  });

  test('attachments travel as ids only', () => {
    const payload = buildPodPayload(
      { tripStopId: STOP_ID, receiverName: 'Ana' },
      [clean()],
      EVENT_ID
    );

    expect(payload.documentIds).toEqual([DOC_ID]);
  });

  test('the receiver name is trimmed', () => {
    expect(
      buildPodPayload({ tripStopId: STOP_ID, receiverName: '  Ana Pérez ' }, [], EVENT_ID)
        .receiverName
    ).toBe('Ana Pérez');
  });
});

describe('isCleanAttachment', () => {
  test('only a Clean scan verdict qualifies', () => {
    expect(isCleanAttachment(clean())).toBe(true);
    expect(isCleanAttachment(clean({ scanStatus: 'clean' }))).toBe(true);
    expect(isCleanAttachment(clean({ scanStatus: 'Pending' }))).toBe(false);
    expect(isCleanAttachment(clean({ scanStatus: 'Infected' }))).toBe(false);
  });
});

describe('podDocumentFields', () => {
  test('POD evidence is owned by the trip transporter, not by the trip', () => {
    // Manager cannot resolve a "Trip" owner type without calling TripManagement,
    // so spec 11 §11 owns POD documents through the transporter instead.
    expect(podDocumentFields(ACCOUNT_ID, TRANSPORTER_ID, 'signature.png')).toEqual({
      accountId: ACCOUNT_ID,
      ownerEntityType: 'Transporter',
      ownerEntityId: TRANSPORTER_ID,
      category: 'Pod',
      classification: 'Internal',
      title: 'signature.png',
    });
  });
});

describe('buildTollClassVariables', () => {
  test('a transporter-type mapping never also sends a transporter id', () => {
    expect(
      buildTollClassVariables({
        target: 'transporterType',
        transporterTypeId: 2,
        transporterId: TRANSPORTER_ID,
        tollVehicleClassCode: 'III',
      })
    ).toEqual({ transporterTypeId: 2, transporterId: null, tollVehicleClassCode: 'III' });
  });

  test('a transporter override never also sends a type id', () => {
    expect(
      buildTollClassVariables({
        target: 'transporter',
        transporterTypeId: 2,
        transporterId: TRANSPORTER_ID,
        tollVehicleClassCode: 'IV',
      })
    ).toEqual({ transporterTypeId: null, transporterId: TRANSPORTER_ID, tollVehicleClassCode: 'IV' });
  });

  test('an incomplete form yields null instead of a request the validator rejects', () => {
    expect(buildTollClassVariables({ target: 'transporterType', tollVehicleClassCode: 'III' })).toBeNull();
    expect(buildTollClassVariables({ target: 'transporter', tollVehicleClassCode: 'III' })).toBeNull();
    expect(
      buildTollClassVariables({ target: 'transporterType', transporterTypeId: 2, tollVehicleClassCode: ' ' })
    ).toBeNull();
  });

  test('the class code is trimmed', () => {
    expect(
      buildTollClassVariables({
        target: 'transporterType',
        transporterTypeId: 1,
        tollVehicleClassCode: ' II ',
      })?.tollVehicleClassCode
    ).toBe('II');
  });
});

describe('datetime-local helpers', () => {
  // Timezone-INDEPENDENT by construction: asserting a fixed literal here would only hold when the
  // runner sits in UTC, which is exactly the condition under which a dropped local offset is
  // invisible. These assert the round-trip property instead, so they fail in any zone if it breaks.
  test('an ISO instant survives a round-trip through the input format', () => {
    const iso = '2026-07-21T08:30:00.000Z';
    expect(toIso(toLocalInput(iso))).toBe(iso);
  });

  test('the input value is local wall time, not the UTC wall clock', () => {
    const iso = '2026-07-21T08:30:00.000Z';
    const local = toLocalInput(iso);
    // Whatever the runner's zone, the rendered value must be the instant as the BROWSER would
    // display it — i.e. shifted by the local offset. In UTC these coincide; elsewhere they must not.
    const expected = new Date(
      new Date(iso).getTime() - new Date(iso).getTimezoneOffset() * 60_000
    )
      .toISOString()
      .slice(0, 16);
    expect(local).toBe(expected);
  });

  test('empty and unparseable values mean "not supplied"', () => {
    expect(toLocalInput(null)).toBe('');
    expect(toLocalInput('not-a-date')).toBe('');
    expect(toIso('')).toBeNull();
    expect(toIso(null)).toBeNull();
    expect(toIso('not-a-date')).toBeNull();
  });
});

describe('trip creation destinations', () => {
  const GEOFENCE_ID = '66666666-6666-6666-6666-666666666666';

  const destination = (overrides: Partial<TripDestinationDraft> = {}): TripDestinationDraft => ({
    name: 'Warehouse',
    latitude: 4.6,
    longitude: -74.08,
    geofenceId: null,
    arrivalRadiusMeters: 150,
    activity: 'Unload',
    requiresPod: false,
    priority: 0,
    ...overrides,
  });

  const stop = (overrides: Partial<RouteSourceStop> = {}): RouteSourceStop => ({
    sequence: 1,
    name: 'Warehouse',
    latitude: 4.6,
    longitude: -74.08,
    geofenceId: null,
    arrivalRadiusMeters: 150,
    requiresPod: false,
    priority: 0,
    ...overrides,
  });

  test('buildStopPayloadFromDestination normalizes blanks and keeps the geofence link', () => {
    const payload = buildStopPayloadFromDestination(
      destination({ name: '  Depot  ', address: '   ', city: '', geofenceId: GEOFENCE_ID })
    );
    expect(payload.name).toBe('Depot');
    expect(payload.address).toBeNull();
    expect(payload.city).toBeNull();
    expect(payload.geofenceId).toBe(GEOFENCE_ID);
    expect(payload.arrivalRadiusMeters).toBe(DEFAULT_ARRIVAL_RADIUS_METERS);
    expect(payload.plannedArrivalFrom).toBeNull();
    expect(payload.plannedArrivalTo).toBeNull();
  });

  test('a zeroed arrival radius falls back to the default rather than sending 0', () => {
    const payload = buildStopPayloadFromDestination(destination({ arrivalRadiusMeters: 0 }));
    expect(payload.arrivalRadiusMeters).toBe(DEFAULT_ARRIVAL_RADIUS_METERS);
  });

  test('returnToOriginStop carries the origin geofence so detection uses the real shape', () => {
    const returnStop = returnToOriginStop('Plant', 4.6, -74.08, GEOFENCE_ID);
    expect(returnStop.name).toBe('Plant');
    expect(returnStop.latitude).toBe(4.6);
    expect(returnStop.geofenceId).toBe(GEOFENCE_ID);
    expect(returnToOriginStop('Plant', 4.6, -74.08).geofenceId).toBeNull();
  });

  test('deriveTripType: one stop is single, several are multi', () => {
    expect(deriveTripType(4.6, -74.08, [])).toBe('single');
    expect(deriveTripType(4.6, -74.08, [stop({ latitude: 4.7 })])).toBe('single');
    expect(
      deriveTripType(4.6, -74.08, [
        stop({ sequence: 1, latitude: 4.7 }),
        stop({ sequence: 2, latitude: 4.8 }),
      ])
    ).toBe('multi');
  });

  test('deriveTripType: a trailing stop back at the origin makes it a round trip', () => {
    const stops = [
      stop({ sequence: 1, latitude: 4.7 }),
      // Return stop within the 6-decimal rounding pickers apply.
      stop({ sequence: 2, latitude: 4.600004, longitude: -74.080004 }),
    ];
    expect(deriveTripType(4.6, -74.08, stops)).toBe('round');
    // Sequence order decides which stop is "last", not array order.
    expect(deriveTripType(4.6, -74.08, [...stops].reverse())).toBe('round');
  });

  test('deriveTripType: a lone stop at the origin is NOT a round trip', () => {
    expect(deriveTripType(4.6, -74.08, [stop({ latitude: 4.6, longitude: -74.08 })])).toBe(
      'single'
    );
  });

  test('destinationsFromStops drops the return stop of a round trip and keeps its geofence', () => {
    const template = destinationsFromStops(
      [
        stop({ sequence: 1, name: 'Client', latitude: 4.7 }),
        stop({ sequence: 2, name: 'Plant', latitude: 4.6, geofenceId: GEOFENCE_ID }),
      ],
      4.6,
      -74.08
    );
    expect(template.tripType).toBe('round');
    expect(template.destinations).toHaveLength(1);
    expect(template.destinations[0].name).toBe('Client');
    expect(template.originGeofenceId).toBe(GEOFENCE_ID);
  });

  test('destinationsFromStops keeps every stop of a multi-destination route in sequence order', () => {
    const template = destinationsFromStops(
      [
        stop({ sequence: 2, name: 'Second', latitude: 4.8 }),
        stop({ sequence: 1, name: 'First', latitude: 4.7 }),
      ],
      4.6,
      -74.08
    );
    expect(template.tripType).toBe('multi');
    expect(template.destinations.map((entry) => entry.name)).toEqual(['First', 'Second']);
    expect(template.originGeofenceId).toBeNull();
  });
});

/**
 * Spec 11a §4.2: what a stop is FOR. The normalizer has to be forgiving in one
 * direction only — an omitted or unrecognised value becomes `Unload`, so a client
 * that predates the field keeps working, while a real value is never rewritten.
 */
describe('stop activity', () => {
  test('recognised values pass through unchanged', () => {
    expect(normalizeStopActivity('Load')).toBe('Load');
    expect(normalizeStopActivity('Unload')).toBe('Unload');
    expect(normalizeStopActivity('Other')).toBe('Other');
  });

  test('anything unrecognised, empty or absent falls back to unloading', () => {
    expect(normalizeStopActivity(null)).toBe('Unload');
    expect(normalizeStopActivity(undefined)).toBe('Unload');
    expect(normalizeStopActivity('')).toBe('Unload');
    expect(normalizeStopActivity('Collect')).toBe('Unload');
  });

  test("a round trip's return leg is Other, not Unload", () => {
    // Parking back at the depot is neither loading nor unloading; calling it either
    // would put a fictional duration into the dwell reports.
    expect(returnToOriginStop('Plant 3', 4.6, -74.08).activity).toBe('Other');
  });

  test('the stop payload carries the activity through to the server', () => {
    const payload = buildStopPayloadFromDestination({
      name: 'Plant 3',
      latitude: 4.6,
      longitude: -74.08,
      geofenceId: null,
      arrivalRadiusMeters: 150,
      activity: 'Load',
      requiresPod: false,
      priority: 0,
    });
    expect(payload.activity).toBe('Load');
  });
});

/**
 * Spec 11a §10: dispatcher attention is exception-driven. Every answer is derived
 * from a fact the row already carries, never a stored flag — which is what lets the
 * filter run over the page the board already fetched.
 */
describe('board exceptions', () => {
  const trip = (overrides: Partial<Parameters<typeof hasException>[0]> = {}) => ({
    phase: 'InTransit',
    status: 'InProgress',
    deviationOpenedAt: null,
    pendingStopCount: 1,
    phaseDelayed: false,
    ...overrides,
  });

  test('overdue reads the derived phase, not the status', () => {
    // The trip is still Created — Overdue is a READING, and the queue stays blocked
    // until a dispatcher decides.
    expect(hasException(trip({ phase: 'Overdue', status: 'Created' }), 'overdue')).toBe(true);
    expect(hasException(trip({ phase: 'Scheduled', status: 'Created' }), 'overdue')).toBe(false);
  });

  test('off corridor is an open deviation episode', () => {
    expect(hasException(trip({ deviationOpenedAt: '2026-08-03T10:00:00Z' }), 'offCorridor')).toBe(true);
    expect(hasException(trip(), 'offCorridor')).toBe(false);
  });

  test('delayed is the backend verdict, so the badge and the alert cannot disagree', () => {
    // The portal used to re-derive this as "next-stop ETA later than the trip's planned
    // END", which is a looser rule than the one raising TripDelayed (that stop's own
    // window plus delayThresholdMinutes) — two answers to one word on one screen.
    expect(hasException(trip({ phaseDelayed: true }), 'delayed')).toBe(true);
    expect(hasException(trip({ phaseDelayed: false }), 'delayed')).toBe(false);
  });

  test('stalled at the final stop is a running trip at a stop with nowhere left to go', () => {
    expect(hasException(trip({ phase: 'AtStop', pendingStopCount: 0 }), 'stalledFinalStop')).toBe(true);
    // Still has destinations ahead, so it is simply working.
    expect(hasException(trip({ phase: 'AtStop', pendingStopCount: 2 }), 'stalledFinalStop')).toBe(false);
    expect(
      hasException(trip({ phase: 'AtStop', pendingStopCount: 0, status: 'Paused' }), 'stalledFinalStop')
    ).toBe(false);
  });

  /**
   * The regression that made this filter useless: the resolver never sets an ETA on the
   * AtStop branch — an estimate to a stop you are already parked at is meaningless — so
   * the old `!phaseEtaAt` test was ALWAYS true and every truck unloading anywhere was
   * reported as stalled at its final stop.
   */
  test('a truck unloading mid-route is not reported as stalled just because it has no ETA', () => {
    expect(
      hasException(trip({ phase: 'AtStop', pendingStopCount: 3 }), 'stalledFinalStop')
    ).toBe(false);
  });
});
