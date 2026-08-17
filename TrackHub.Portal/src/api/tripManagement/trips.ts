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
 * Trip management API (TripManagement backend): plain typed async functions.
 * Failures THROW ApiError — fallbacks and toasts belong to the caller layer
 * (src/queries/trips.ts handles both for components).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  TripSummaryFieldsFragment,
  TripStopFieldsFragment,
  DeliveryFieldsFragment,
  ProofOfDeliveryFieldsFragment,
  TransporterTollClassFieldsFragment,
  RoutePlanFieldsFragment,
  TripShareFieldsFragment,
  TollStationFieldsFragment,
  TollTariffFieldsFragment,
  TollVehicleClassFieldsFragment,
  TollStationMatchFieldsFragment,
  GetTripsQuery,
  GetTripDetailQuery,
  GetTripTimelineQuery,
  GetTripRouteReplayQuery,
  GetTollStationsQuery,
  GetTollStationDetailQuery,
  EstimateTollsQuery,
  AssignTripMutation,
  ImportTollCatalogMutation,
  ImportTripsCsvMutation,
  DeclareTripInTransitMutation,
  TripDtoInput,
  TripStopDtoInput,
  DeliveryDtoInput,
  ProofOfDeliveryDtoInput,
  TripShareFieldFlagsDtoInput,
  TollStationDtoInput,
  TollTariffDtoInput,
  TollVehicleClassDtoInput,
} from './generated/graphql';
import {
  GetTripsDocument,
  GetTripDetailDocument,
  GetTripTimelineDocument,
  GetTripRouteReplayDocument,
  GetTollVehicleClassesDocument,
  GetTollStationsDocument,
  GetTollStationDetailDocument,
  EstimateTollsDocument,
  CreateTripDocument,
  UpdateTripDocument,
  DeleteTripDocument,
  AssignTripDocument,
  PlanTripRouteDocument,
  StartTripDocument,
  PauseTripDocument,
  ResumeTripDocument,
  CompleteTripDocument,
  CancelTripDocument,
  AbortTripDocument,
  ShareTripDocument,
  RevokeTripShareDocument,
  AddTripStopDocument,
  UpdateTripStopDocument,
  RemoveTripStopDocument,
  ReorderTripStopsDocument,
  RecordStopArrivalDocument,
  RecordStopDepartureDocument,
  SkipStopDocument,
  CreateDeliveryDocument,
  UpdateDeliveryDocument,
  UpdateDeliveryOutcomeDocument,
  DeleteDeliveryDocument,
  RecordProofOfDeliveryDocument,
  SetTransporterTollClassDocument,
  CreateTollVehicleClassDocument,
  UpdateTollVehicleClassDocument,
  DeactivateTollVehicleClassDocument,
  CreateTollStationDocument,
  UpdateTollStationDocument,
  DeactivateTollStationDocument,
  CreateTollTariffDocument,
  UpdateTollTariffDocument,
  DeleteTollTariffDocument,
  ImportTollCatalogDocument,
  ImportTripsCsvDocument,
  DeclareTripInTransitDocument,
} from './tripsOperations';

export type Trip = TripSummaryFieldsFragment;
export type TripStop = TripStopFieldsFragment;
export type TripDelivery = DeliveryFieldsFragment;
export type TransporterTollClass = TransporterTollClassFieldsFragment;
export type RoutePlan = RoutePlanFieldsFragment;
export type TripShare = TripShareFieldsFragment;
export type TripsPage = GetTripsQuery['trips'];
export type TripDetail = GetTripDetailQuery['tripDetail'];
export type TripAssignment = AssignTripMutation['assignTrip'];
export type TripEvent = GetTripTimelineQuery['tripTimeline']['items'][number];
export type TripTimelinePage = GetTripTimelineQuery['tripTimeline'];
export type RouteReplay = GetTripRouteReplayQuery['tripRouteReplay'];
export type ProofOfDelivery = ProofOfDeliveryFieldsFragment;
export type TollStation = TollStationFieldsFragment;
export type TollTariff = TollTariffFieldsFragment;
export type TollVehicleClass = TollVehicleClassFieldsFragment;
export type TollStationMatch = TollStationMatchFieldsFragment;
export type TollStationsPage = GetTollStationsQuery['tollStations'];
export type TollStationDetail = GetTollStationDetailQuery['tollStationDetail'];
export type TollEstimate = EstimateTollsQuery['estimateTolls'];
export type TollCatalogImportResult = ImportTollCatalogMutation['importTollCatalog'];
export type TripCsvImportResult = ImportTripsCsvMutation['importTripsCsv'];
export type TripStartBackfillResult = DeclareTripInTransitMutation['declareTripInTransit'];
export type {
  TripDtoInput,
  TripStopDtoInput,
  DeliveryDtoInput,
  ProofOfDeliveryDtoInput,
  TripShareFieldFlagsDtoInput,
  TollStationDtoInput,
  TollTariffDtoInput,
  TollVehicleClassDtoInput,
};

/** Server-side filters accepted by {@link getTrips} — the dispatch board's board state. */
export interface TripListFilters {
  statuses?: string[] | null;
  from?: string | null;
  to?: string | null;
  transporterId?: string | null;
  driverId?: string | null;
  customer?: string | null;
  search?: string | null;
  skip?: number | null;
  take?: number | null;
}

/** Server-side filters accepted by {@link getTollStations}. */
export interface TollStationFilters {
  search?: string | null;
  country?: string | null;
  active?: boolean | null;
  skip?: number | null;
  take?: number | null;
}

/* ---------------------------------------------------------------- queries */

export async function getTrips(filters: TripListFilters = {}): Promise<TripsPage> {
  const data = await executeGraphQL('tripManagement', GetTripsDocument, {
    statuses: filters.statuses ?? null,
    from: filters.from ?? null,
    to: filters.to ?? null,
    transporterId: filters.transporterId ?? null,
    driverId: filters.driverId ?? null,
    customer: filters.customer ?? null,
    search: filters.search ?? null,
    skip: filters.skip ?? null,
    take: filters.take ?? null,
  });
  return data.trips;
}

export async function getTripDetail(tripId: string): Promise<TripDetail> {
  const data = await executeGraphQL('tripManagement', GetTripDetailDocument, { tripId });
  return data.tripDetail;
}

export async function getTripTimeline(
  tripId: string,
  skip = 0,
  take = 100
): Promise<TripTimelinePage> {
  const data = await executeGraphQL('tripManagement', GetTripTimelineDocument, {
    tripId,
    skip,
    take,
  });
  return data.tripTimeline;
}

/**
 * Recorded positions for the replay control. `truncated` reports Telemetry's
 * 10 000-point cap and MUST be surfaced — a silently shortened route is a lie
 * about where the vehicle went (spec 11 acceptance 22).
 */
export async function getTripRouteReplay(
  tripId: string,
  maxPoints?: number | null
): Promise<RouteReplay> {
  const data = await executeGraphQL('tripManagement', GetTripRouteReplayDocument, {
    tripId,
    maxPoints: maxPoints ?? null,
  });
  return data.tripRouteReplay;
}

export async function getTollVehicleClasses(): Promise<TollVehicleClass[]> {
  const data = await executeGraphQL('tripManagement', GetTollVehicleClassesDocument);
  return data.tollVehicleClasses;
}

export async function getTollStations(
  filters: TollStationFilters = {}
): Promise<TollStationsPage> {
  const data = await executeGraphQL('tripManagement', GetTollStationsDocument, {
    search: filters.search ?? null,
    country: filters.country ?? null,
    active: filters.active ?? null,
    skip: filters.skip ?? null,
    take: filters.take ?? null,
  });
  return data.tollStations;
}

export async function getTollStationDetail(tollStationId: string): Promise<TollStationDetail> {
  const data = await executeGraphQL('tripManagement', GetTollStationDetailDocument, {
    tollStationId,
  });
  return data.tollStationDetail;
}

/**
 * Re-prices a computed route plan for another vehicle class. An empty catalog
 * answers `NoStations` with a null amount — never zero-as-fact (spec 11 §7.7).
 */
export async function estimateTolls(
  routePlanId: string,
  tollVehicleClass?: string | null
): Promise<TollEstimate> {
  const data = await executeGraphQL('tripManagement', EstimateTollsDocument, {
    routePlanId,
    tollVehicleClass: tollVehicleClass ?? null,
  });
  return data.estimateTolls;
}

/* -------------------------------------------------------------- mutations */

export async function createTrip(trip: TripDtoInput): Promise<Trip> {
  const data = await executeGraphQL('tripManagement', CreateTripDocument, { trip });
  return data.createTrip;
}

export async function updateTrip(tripId: string, trip: TripDtoInput): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', UpdateTripDocument, { tripId, trip });
  return data.updateTrip;
}

/** Returns the id of the deleted trip (schema: `deleteTrip: UUID!`). */
export async function deleteTrip(tripId: string): Promise<string> {
  const data = await executeGraphQL('tripManagement', DeleteTripDocument, { id: tripId });
  return data.deleteTrip;
}

export async function assignTrip(
  tripId: string,
  driverId: string,
  transporterId?: string | null
): Promise<TripAssignment> {
  const data = await executeGraphQL('tripManagement', AssignTripDocument, {
    tripId,
    driverId,
    transporterId: transporterId ?? null,
  });
  return data.assignTrip;
}

/**
 * Requests an ORS route plan. A routing failure comes back as a `Failed` plan
 * with an error code, NOT as a thrown error (spec 11 §17.18) — the caller must
 * read `status` rather than assuming success.
 */
export async function planTripRoute(
  tripId: string,
  corridorMeters?: number | null,
  tollVehicleClass?: string | null
): Promise<RoutePlan> {
  const data = await executeGraphQL('tripManagement', PlanTripRouteDocument, {
    tripId,
    corridorMeters: corridorMeters ?? null,
    tollVehicleClass: tollVehicleClass ?? null,
  });
  return data.planTripRoute;
}

export async function startTrip(tripId: string): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', StartTripDocument, { id: tripId });
  return data.startTrip;
}

export async function pauseTrip(tripId: string): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', PauseTripDocument, { id: tripId });
  return data.pauseTrip;
}

export async function resumeTrip(tripId: string): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', ResumeTripDocument, { id: tripId });
  return data.resumeTrip;
}

export async function completeTrip(tripId: string, force = false): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', CompleteTripDocument, { tripId, force });
  return data.completeTrip;
}

export async function cancelTrip(tripId: string, reason: string): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', CancelTripDocument, { tripId, reason });
  return data.cancelTrip;
}

export async function abortTrip(tripId: string, reason: string): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', AbortTripDocument, { tripId, reason });
  return data.abortTrip;
}

/**
 * Creates a public tracking link. The plaintext `token` is returned exactly
 * once, here — it is never readable again (spec 11 acceptance 23).
 */
export async function shareTrip(
  tripId: string,
  expiresAt: string,
  purpose: string,
  fieldFlags: TripShareFieldFlagsDtoInput
): Promise<TripShare> {
  const data = await executeGraphQL('tripManagement', ShareTripDocument, {
    tripId,
    expiresAt,
    purpose,
    fieldFlags,
  });
  return data.shareTrip;
}

export async function revokeTripShare(tripId: string, tripShareId: string): Promise<string> {
  const data = await executeGraphQL('tripManagement', RevokeTripShareDocument, {
    tripId,
    tripShareId,
  });
  return data.revokeTripShare;
}

export async function addTripStop(tripId: string, stop: TripStopDtoInput): Promise<TripStop> {
  const data = await executeGraphQL('tripManagement', AddTripStopDocument, { tripId, stop });
  return data.addTripStop;
}

export async function updateTripStop(
  tripStopId: string,
  stop: TripStopDtoInput
): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', UpdateTripStopDocument, {
    tripStopId,
    stop,
  });
  return data.updateTripStop;
}

export async function removeTripStop(tripStopId: string): Promise<string> {
  const data = await executeGraphQL('tripManagement', RemoveTripStopDocument, { id: tripStopId });
  return data.removeTripStop;
}

export async function reorderTripStops(
  tripId: string,
  orderedStopIds: string[]
): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', ReorderTripStopsDocument, {
    tripId,
    orderedStopIds,
  });
  return data.reorderTripStops;
}

export async function recordStopArrival(
  tripId: string,
  tripStopId: string,
  occurredAt: string,
  clientEventId: string
): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', RecordStopArrivalDocument, {
    tripId,
    tripStopId,
    occurredAt,
    clientEventId,
  });
  return data.recordStopArrival;
}

export async function recordStopDeparture(
  tripId: string,
  tripStopId: string,
  occurredAt: string,
  clientEventId: string
): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', RecordStopDepartureDocument, {
    tripId,
    tripStopId,
    occurredAt,
    clientEventId,
  });
  return data.recordStopDeparture;
}

export async function skipStop(
  tripId: string,
  tripStopId: string,
  occurredAt: string,
  reason: string,
  clientEventId: string
): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', SkipStopDocument, {
    tripId,
    tripStopId,
    occurredAt,
    reason,
    clientEventId,
  });
  return data.skipStop;
}

/* ----------------------------------------------------- delivery mutations */

export async function createDelivery(
  tripStopId: string,
  delivery: DeliveryDtoInput
): Promise<TripDelivery> {
  const data = await executeGraphQL('tripManagement', CreateDeliveryDocument, {
    tripStopId,
    delivery,
  });
  return data.createDelivery;
}

export async function updateDelivery(
  deliveryId: string,
  delivery: DeliveryDtoInput
): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', UpdateDeliveryDocument, {
    deliveryId,
    delivery,
  });
  return data.updateDelivery;
}

/**
 * Records a delivery outcome. `clientEventId` is the server's idempotency key —
 * callers MUST reuse the same id when retrying one attempt, or the retry stops
 * being idempotent (spec 11 §7.3, §9).
 */
export async function updateDeliveryOutcome(
  tripId: string,
  deliveryId: string,
  status: string,
  observations: string | null,
  clientEventId: string
): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', UpdateDeliveryOutcomeDocument, {
    tripId,
    deliveryId,
    status,
    observations,
    clientEventId,
  });
  return data.updateDeliveryOutcome;
}

/** Returns the id of the deleted delivery (schema: `deleteDelivery: UUID!`). */
export async function deleteDelivery(deliveryId: string): Promise<string> {
  const data = await executeGraphQL('tripManagement', DeleteDeliveryDocument, { id: deliveryId });
  return data.deleteDelivery;
}

/* ---------------------------------------------------------- POD mutation  */

/**
 * Captures proof of delivery for a stop. Every `documentIds` entry must already
 * be an uploaded Manager document whose scan finished `Clean` — otherwise the
 * backend answers `POD_DOCUMENT_NOT_CLEAN` (spec 11 §9). Idempotent on
 * `(tripStopId, clientEventId)`, so a retry of the same capture must carry the
 * same `clientEventId`.
 */
export async function recordProofOfDelivery(
  tripId: string,
  proofOfDelivery: ProofOfDeliveryDtoInput
): Promise<ProofOfDelivery> {
  const data = await executeGraphQL('tripManagement', RecordProofOfDeliveryDocument, {
    tripId,
    proofOfDelivery,
  });
  return data.recordProofOfDelivery;
}

/* ------------------------------------------------- toll catalog mutations */

/**
 * Maps a transporter type — or one transporter as a row-level override — to a
 * toll vehicle class for THIS account. Without a mapping a new trip's
 * `TollVehicleClass` can never be defaulted, so toll estimation silently never
 * picks up a class (spec 11 §7.3 `CreateTripCommand`, §7.6).
 */
export async function setTransporterTollClass(
  transporterTypeId: number | null,
  transporterId: string | null,
  tollVehicleClassCode: string
): Promise<TransporterTollClass> {
  const data = await executeGraphQL('tripManagement', SetTransporterTollClassDocument, {
    transporterTypeId,
    transporterId,
    tollVehicleClassCode,
  });
  return data.setTransporterTollClass;
}

export async function createTollVehicleClass(
  vehicleClass: TollVehicleClassDtoInput
): Promise<TollVehicleClass> {
  const data = await executeGraphQL('tripManagement', CreateTollVehicleClassDocument, {
    vehicleClass,
  });
  return data.createTollVehicleClass;
}

export async function updateTollVehicleClass(
  tollVehicleClassId: string,
  vehicleClass: TollVehicleClassDtoInput
): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', UpdateTollVehicleClassDocument, {
    tollVehicleClassId,
    vehicleClass,
  });
  return data.updateTollVehicleClass;
}

export async function deactivateTollVehicleClass(tollVehicleClassId: string): Promise<string> {
  const data = await executeGraphQL('tripManagement', DeactivateTollVehicleClassDocument, {
    id: tollVehicleClassId,
  });
  return data.deactivateTollVehicleClass;
}

export async function createTollStation(station: TollStationDtoInput): Promise<TollStation> {
  const data = await executeGraphQL('tripManagement', CreateTollStationDocument, { station });
  return data.createTollStation;
}

export async function updateTollStation(
  tollStationId: string,
  station: TollStationDtoInput
): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', UpdateTollStationDocument, {
    tollStationId,
    station,
  });
  return data.updateTollStation;
}

export async function deactivateTollStation(tollStationId: string): Promise<string> {
  const data = await executeGraphQL('tripManagement', DeactivateTollStationDocument, {
    id: tollStationId,
  });
  return data.deactivateTollStation;
}

export async function createTollTariff(tariff: TollTariffDtoInput): Promise<TollTariff> {
  const data = await executeGraphQL('tripManagement', CreateTollTariffDocument, { tariff });
  return data.createTollTariff;
}

export async function updateTollTariff(
  tollTariffId: string,
  tariff: TollTariffDtoInput
): Promise<boolean> {
  const data = await executeGraphQL('tripManagement', UpdateTollTariffDocument, {
    tollTariffId,
    tariff,
  });
  return data.updateTollTariff;
}

export async function deleteTollTariff(tollTariffId: string): Promise<string> {
  const data = await executeGraphQL('tripManagement', DeleteTollTariffDocument, {
    id: tollTariffId,
  });
  return data.deleteTollTariff;
}

/** Bulk station/tariff upload. Row-level failures come back in `errors`, never as a throw. */
export async function importTollCatalog(csv: string): Promise<TollCatalogImportResult> {
  const data = await executeGraphQL('tripManagement', ImportTollCatalogDocument, { csv });
  return data.importTollCatalog;
}

/**
 * Bulk trip planning (spec 11a §9.1). Same contract as the toll upload: valid rows
 * land, rejected ones come back with their line number and reason.
 */
export async function importTripsCsv(csv: string): Promise<TripCsvImportResult> {
  const data = await executeGraphQL('tripManagement', ImportTripsCsvDocument, { csv });
  return data.importTripsCsv;
}

/**
 * Declares a freshly planned trip already under way (spec 11a §5.4). `startedAt` is
 * only used when Geofencing has no record of the vehicle leaving the origin zone.
 */
export async function declareTripInTransit(
  tripId: string,
  startedAt: string | null
): Promise<TripStartBackfillResult> {
  const data = await executeGraphQL('tripManagement', DeclareTripInTransitDocument, { tripId, startedAt });
  return data.declareTripInTransit;
}
