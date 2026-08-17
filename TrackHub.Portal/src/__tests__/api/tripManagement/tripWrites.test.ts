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
 * Spec 11 §7.3 write surfaces the dispatcher drives from the portal: delivery
 * CRUD, delivery outcome, proof of delivery, and the transporter → toll-class
 * mapping. The assertions here are about the CONTRACT — which backend is
 * addressed, that every value travels as a GraphQL variable, and that the
 * idempotency keys are passed straight through.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import {
  createDelivery,
  updateDelivery,
  updateDeliveryOutcome,
  deleteDelivery,
  recordProofOfDelivery,
  setTransporterTollClass,
} from 'api/tripManagement/trips';
import {
  CreateDeliveryDocument,
  UpdateDeliveryDocument,
  UpdateDeliveryOutcomeDocument,
  DeleteDeliveryDocument,
  RecordProofOfDeliveryDocument,
  SetTransporterTollClassDocument,
} from 'api/tripManagement/tripsOperations';

vi.mock('api/core/graphqlClient', () => ({ executeGraphQL: vi.fn() }));

const TRIP_ID = '11111111-1111-1111-1111-111111111111';
const STOP_ID = '22222222-2222-2222-2222-222222222222';
const DELIVERY_ID = '33333333-3333-3333-3333-333333333333';
const DOC_ID = '44444444-4444-4444-4444-444444444444';
const EVENT_ID = '55555555-5555-5555-5555-555555555555';
const TRANSPORTER_ID = '66666666-6666-6666-6666-666666666666';

const mocked = vi.mocked(executeGraphQL);

const resolveWith = (value: unknown) =>
  mocked.mockResolvedValue(value as Awaited<ReturnType<typeof executeGraphQL>>);

beforeEach(() => vi.clearAllMocks());

describe('delivery writes', () => {
  test('createDelivery posts the stop id and dto as variables', async () => {
    resolveWith({ createDelivery: { deliveryId: DELIVERY_ID } });

    await createDelivery(STOP_ID, {
      clientName: 'Acme',
      reference: null,
      branchName: null,
      productsSummary: null,
      observations: null,
      sequenceIndex: 0,
    });

    const [backend, document, variables] = mocked.mock.calls[0];
    expect(backend).toBe('tripManagement');
    expect(document).toBe(CreateDeliveryDocument);
    expect(variables).toEqual({
      tripStopId: STOP_ID,
      delivery: expect.objectContaining({ clientName: 'Acme' }),
    });
  });

  test('updateDelivery addresses the delivery by id', async () => {
    resolveWith({ updateDelivery: true });

    await updateDelivery(DELIVERY_ID, {
      clientName: 'Acme',
      reference: null,
      branchName: null,
      productsSummary: null,
      observations: null,
      sequenceIndex: 1,
    });

    const [, document, variables] = mocked.mock.calls[0];
    expect(document).toBe(UpdateDeliveryDocument);
    expect(variables).toMatchObject({ deliveryId: DELIVERY_ID });
  });

  test('updateDeliveryOutcome forwards the clientEventId unchanged', async () => {
    resolveWith({ updateDeliveryOutcome: true });

    await updateDeliveryOutcome(TRIP_ID, DELIVERY_ID, 'PartiallyDelivered', null, EVENT_ID);

    const [, document, variables] = mocked.mock.calls[0];
    expect(document).toBe(UpdateDeliveryOutcomeDocument);
    expect(variables).toEqual({
      tripId: TRIP_ID,
      deliveryId: DELIVERY_ID,
      status: 'PartiallyDelivered',
      observations: null,
      clientEventId: EVENT_ID,
    });
  });

  test('deleteDelivery returns the deleted id the schema hands back', async () => {
    resolveWith({ deleteDelivery: DELIVERY_ID });

    await expect(deleteDelivery(DELIVERY_ID)).resolves.toBe(DELIVERY_ID);
    const [, document, variables] = mocked.mock.calls[0];
    expect(document).toBe(DeleteDeliveryDocument);
    expect(variables).toEqual({ id: DELIVERY_ID });
  });
});

describe('recordProofOfDelivery', () => {
  test('sends the trip id alongside the POD dto, document ids included', async () => {
    resolveWith({ recordProofOfDelivery: { proofOfDeliveryId: 'pod' } });

    await recordProofOfDelivery(TRIP_ID, {
      tripStopId: STOP_ID,
      deliveryId: null,
      receiverName: 'Ana',
      receiverDocument: null,
      notes: null,
      capturedAt: '2026-07-21T10:00:00.000Z',
      latitude: null,
      longitude: null,
      documentIds: [DOC_ID],
      clientEventId: EVENT_ID,
    });

    const [backend, document, variables] = mocked.mock.calls[0];
    expect(backend).toBe('tripManagement');
    expect(document).toBe(RecordProofOfDeliveryDocument);
    expect(variables).toMatchObject({
      tripId: TRIP_ID,
      proofOfDelivery: {
        tripStopId: STOP_ID,
        documentIds: [DOC_ID],
        clientEventId: EVENT_ID,
      },
    });
  });

  test('a retry of the same capture reuses the clientEventId', async () => {
    resolveWith({ recordProofOfDelivery: { proofOfDeliveryId: 'pod' } });
    const dto = {
      tripStopId: STOP_ID,
      deliveryId: null,
      receiverName: 'Ana',
      receiverDocument: null,
      notes: null,
      capturedAt: '2026-07-21T10:00:00.000Z',
      latitude: null,
      longitude: null,
      documentIds: [],
      clientEventId: EVENT_ID,
    };

    await recordProofOfDelivery(TRIP_ID, dto);
    await recordProofOfDelivery(TRIP_ID, dto);

    const ids = mocked.mock.calls.map(
      (call) => (call[2] as { proofOfDelivery: { clientEventId: string } }).proofOfDelivery.clientEventId
    );
    expect(ids).toEqual([EVENT_ID, EVENT_ID]);
  });
});

describe('setTransporterTollClass', () => {
  test('maps a transporter type', async () => {
    resolveWith({ setTransporterTollClass: { transporterTollClassId: 'ttc' } });

    await setTransporterTollClass(3, null, 'III');

    const [, document, variables] = mocked.mock.calls[0];
    expect(document).toBe(SetTransporterTollClassDocument);
    expect(variables).toEqual({
      transporterTypeId: 3,
      transporterId: null,
      tollVehicleClassCode: 'III',
    });
  });

  test('maps a single transporter as a row-level override', async () => {
    resolveWith({ setTransporterTollClass: { transporterTollClassId: 'ttc' } });

    await setTransporterTollClass(null, TRANSPORTER_ID, 'IV');

    expect(mocked.mock.calls[0][2]).toEqual({
      transporterTypeId: null,
      transporterId: TRANSPORTER_ID,
      tollVehicleClassCode: 'IV',
    });
  });
});
