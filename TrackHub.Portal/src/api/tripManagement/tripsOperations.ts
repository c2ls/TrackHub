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
 * Trip management GraphQL documents (TripManagement backend). Codegen validates
 * these against schemas/tripManagement.graphql; values always travel as
 * variables — never string interpolation.
 *
 * The anonymous public tracking snapshot is NOT here: it is an unauthenticated
 * REST endpoint (see api/tripManagement/publicTrips.ts).
 */

import { graphql } from './generated';

export const TripSummaryFieldsFragment = graphql(`
  fragment TripSummaryFields on TripVm {
    tripId
    accountId
    code
    status
    transporterId
    driverId
    routePlanId
    serviceOrderId
    externalReference
    customerName
    originName
    originLatitude
    originLongitude
    originGeofenceId
    originRadiusMeters
    plannedStartAt
    plannedEndAt
    actualStartAt
    actualEndAt
    armedAt
    originArrivedAt
    originDepartedAt
    # The derived phase (spec 11a §10). It is what the board leads with: a column of
    # rows all reading "InProgress" is exactly why a dispatcher used to have to open
    # each one to find out what was actually happening.
    phase
    phaseStopName
    phaseStopActivity
    phaseEtaAt
    # Decided by the backend, from the same stop-level threshold that raises TripDelayed —
    # so the badge, the exception filter and the alert cannot disagree about the word.
    phaseDelayed
    notes
    lastPositionAt
    lastLatitude
    lastLongitude
    actualDistanceMeters
    tollVehicleClass
    deviationOpenedAt
    cancellationReason
    stopCount
    # How much of the route is still ahead. "Stalled at final stop" needs to tell a truck
    # unloading at stop 2 of 5 from one parked at the last stop with nowhere left to go,
    # and the phase alone cannot — both read AtStop.
    pendingStopCount
    lastModified
  }
`);

export const DeliveryFieldsFragment = graphql(`
  fragment DeliveryFields on DeliveryVm {
    deliveryId
    accountId
    tripStopId
    reference
    clientName
    branchName
    productsSummary
    status
    observations
    sequenceIndex
  }
`);

export const ProofOfDeliveryFieldsFragment = graphql(`
  fragment ProofOfDeliveryFields on ProofOfDeliveryVm {
    proofOfDeliveryId
    accountId
    tripStopId
    deliveryId
    receiverName
    receiverDocument
    capturedAt
    latitude
    longitude
    notes
    documents {
      tripDocumentId
      documentId
      tripStopId
      proofOfDeliveryId
      kind
    }
  }
`);

export const TransporterTollClassFieldsFragment = graphql(`
  fragment TransporterTollClassFields on TransporterTollClassVm {
    transporterTollClassId
    accountId
    transporterTypeId
    transporterId
    tollVehicleClassCode
  }
`);

export const TripStopFieldsFragment = graphql(`
  fragment TripStopFields on TripStopVm {
    tripStopId
    accountId
    tripId
    sequence
    name
    address
    # city is not cosmetic duplication of address: it is the ONLY locality the
    # anonymous customer snapshot may disclose (spec 11 §7.8), so it has to
    # survive a round trip through the edit dialog. Leaving it out of the
    # selection made every stop edit write it back as null, silently stripping
    # the one place name the customer is allowed to see.
    city
    latitude
    longitude
    geofenceId
    arrivalRadiusMeters
    # What the vehicle does here. It is what turns the dwell between arrival and
    # departure from an anonymous number of minutes into loading or unloading time.
    activity
    plannedArrivalFrom
    plannedArrivalTo
    status
    actualArrivalAt
    actualDepartureAt
    etaAt
    etaSource
    delayAlertedAt
    requiresPod
    priority
    observations
    deliveries {
      ...DeliveryFields
    }
  }
`);

export const TollStationMatchFieldsFragment = graphql(`
  fragment TollStationMatchFields on TollStationMatchVm {
    tollStationId
    name
    code
    latitude
    longitude
    roadName
    direction
    amount
    currency
    hasTariff
  }
`);

export const RoutePlanFieldsFragment = graphql(`
  fragment RoutePlanFields on RoutePlanVm {
    routePlanId
    accountId
    tripId
    provider
    geometry {
      coordinates {
        latitude
        longitude
      }
    }
    corridor {
      coordinates {
        latitude
        longitude
      }
    }
    corridorMeters
    plannedDistanceMeters
    plannedDurationSeconds
    computedAt
    status
    errorCode
    errorMessage
    tollVehicleClass
    estimatedTollAmount
    tollCurrency
    tollStatus
    tollStations {
      ...TollStationMatchFields
    }
  }
`);

export const TripShareFieldsFragment = graphql(`
  fragment TripShareFields on TripShareVm {
    tripShareId
    accountId
    tripId
    publicLinkGrantId
    includeDriverName
    includeVehicle
    includeLivePosition
    includeStopDetail
    includePodSummary
    # Selected so an ALREADY-CREATED link can be audited. The dialog sends all
    # six flags on creation, so omitting this one here cost nothing at share
    # time — but it left the dispatcher unable to tell, from the "Existing
    # links" list, which of the live links disclose the planned route. A
    # disclosure the UI cannot show is a disclosure nobody can revoke on
    # purpose (spec 11 §7.8).
    includeRoute
    createdByPrincipalId
    expiresAt
    revokedAt
    token
  }
`);

export const TollStationFieldsFragment = graphql(`
  fragment TollStationFields on TollStationVm {
    tollStationId
    name
    code
    latitude
    longitude
    country
    region
    roadName
    direction
    operator
    notes
    active
  }
`);

export const TollTariffFieldsFragment = graphql(`
  fragment TollTariffFields on TollTariffVm {
    tollTariffId
    tollStationId
    tollVehicleClassCode
    amount
    currency
    effectiveFrom
    effectiveTo
  }
`);

export const TollVehicleClassFieldsFragment = graphql(`
  fragment TollVehicleClassFields on TollVehicleClassVm {
    tollVehicleClassId
    code
    name
    description
    sortOrder
    active
  }
`);

/* ---------------------------------------------------------------- queries */

export const GetTripsDocument = graphql(`
  query GetTrips(
    $statuses: [String!]
    $from: DateTime
    $to: DateTime
    $transporterId: UUID
    $driverId: UUID
    $customer: String
    $search: String
    $skip: Int
    $take: Int
  ) {
    trips(
      query: {
        statuses: $statuses
        from: $from
        to: $to
        transporterId: $transporterId
        driverId: $driverId
        customer: $customer
        search: $search
        skip: $skip
        take: $take
      }
    ) {
      items {
        ...TripSummaryFields
      }
      totalCount
    }
  }
`);

export const GetTripDetailDocument = graphql(`
  query GetTripDetail($tripId: UUID!) {
    tripDetail(query: { tripId: $tripId }) {
      trip {
        ...TripSummaryFields
      }
      stops {
        ...TripStopFields
      }
      assignment {
        tripAssignmentId
        accountId
        tripId
        driverId
        transporterId
        status
        assignedAt
        acknowledgedAt
        endedAt
      }
      routePlan {
        ...RoutePlanFields
      }
      proofsOfDelivery {
        ...ProofOfDeliveryFields
      }
      shares {
        ...TripShareFields
      }
    }
  }
`);

export const GetTripTimelineDocument = graphql(`
  query GetTripTimeline($tripId: UUID!, $skip: Int, $take: Int) {
    tripTimeline(query: { tripId: $tripId, skip: $skip, take: $take }) {
      items {
        tripEventId
        tripId
        tripStopId
        eventType
        occurredAt
        source
        payloadJson
      }
      totalCount
    }
  }
`);

export const GetTripRouteReplayDocument = graphql(`
  query GetTripRouteReplay($tripId: UUID!, $maxPoints: Int) {
    tripRouteReplay(query: { tripId: $tripId, maxPoints: $maxPoints }) {
      points {
        latitude
        longitude
        deviceTimestamp
        speed
      }
      truncated
    }
  }
`);

export const GetTollVehicleClassesDocument = graphql(`
  query GetTollVehicleClasses {
    tollVehicleClasses {
      ...TollVehicleClassFields
    }
  }
`);

export const GetTollStationsDocument = graphql(`
  query GetTollStations(
    $search: String
    $country: String
    $active: Boolean
    $skip: Int
    $take: Int
  ) {
    tollStations(
      query: { search: $search, country: $country, active: $active, skip: $skip, take: $take }
    ) {
      items {
        ...TollStationFields
      }
      totalCount
    }
  }
`);

export const GetTollStationDetailDocument = graphql(`
  query GetTollStationDetail($tollStationId: UUID!) {
    tollStationDetail(query: { tollStationId: $tollStationId }) {
      station {
        ...TollStationFields
      }
      tariffs {
        ...TollTariffFields
      }
    }
  }
`);

export const EstimateTollsDocument = graphql(`
  query EstimateTolls($routePlanId: UUID!, $tollVehicleClass: String) {
    estimateTolls(query: { routePlanId: $routePlanId, tollVehicleClass: $tollVehicleClass }) {
      tollVehicleClass
      estimatedTollAmount
      currency
      tollStatus
      stations {
        ...TollStationMatchFields
      }
    }
  }
`);

/* -------------------------------------------------------------- mutations */

export const CreateTripDocument = graphql(`
  mutation CreateTrip($trip: TripDtoInput!) {
    createTrip(command: { trip: $trip }) {
      ...TripSummaryFields
    }
  }
`);

export const UpdateTripDocument = graphql(`
  mutation UpdateTrip($tripId: UUID!, $trip: TripDtoInput!) {
    updateTrip(command: { tripId: $tripId, trip: $trip })
  }
`);

export const DeleteTripDocument = graphql(`
  mutation DeleteTrip($id: UUID!) {
    deleteTrip(id: $id)
  }
`);

export const AssignTripDocument = graphql(`
  mutation AssignTrip($tripId: UUID!, $driverId: UUID!, $transporterId: UUID) {
    assignTrip(command: { tripId: $tripId, driverId: $driverId, transporterId: $transporterId }) {
      tripAssignmentId
      tripId
      driverId
      transporterId
      status
      assignedAt
      acknowledgedAt
      endedAt
    }
  }
`);

export const PlanTripRouteDocument = graphql(`
  mutation PlanTripRoute($tripId: UUID!, $corridorMeters: Int, $tollVehicleClass: String) {
    planTripRoute(
      command: {
        tripId: $tripId
        corridorMeters: $corridorMeters
        tollVehicleClass: $tollVehicleClass
      }
    ) {
      ...RoutePlanFields
    }
  }
`);

export const StartTripDocument = graphql(`
  mutation StartTrip($id: UUID!) {
    startTrip(id: $id)
  }
`);

export const PauseTripDocument = graphql(`
  mutation PauseTrip($id: UUID!) {
    pauseTrip(id: $id)
  }
`);

export const ResumeTripDocument = graphql(`
  mutation ResumeTrip($id: UUID!) {
    resumeTrip(id: $id)
  }
`);

export const CompleteTripDocument = graphql(`
  mutation CompleteTrip($tripId: UUID!, $force: Boolean!) {
    completeTrip(command: { tripId: $tripId, force: $force })
  }
`);

export const CancelTripDocument = graphql(`
  mutation CancelTrip($tripId: UUID!, $reason: String!) {
    cancelTrip(command: { tripId: $tripId, reason: $reason })
  }
`);

export const AbortTripDocument = graphql(`
  mutation AbortTrip($tripId: UUID!, $reason: String!) {
    abortTrip(command: { tripId: $tripId, reason: $reason })
  }
`);

export const ShareTripDocument = graphql(`
  mutation ShareTrip(
    $tripId: UUID!
    $expiresAt: DateTime!
    $purpose: String!
    $fieldFlags: TripShareFieldFlagsDtoInput!
  ) {
    shareTrip(
      command: {
        tripId: $tripId
        expiresAt: $expiresAt
        purpose: $purpose
        fieldFlags: $fieldFlags
      }
    ) {
      ...TripShareFields
    }
  }
`);

export const RevokeTripShareDocument = graphql(`
  mutation RevokeTripShare($tripId: UUID!, $tripShareId: UUID!) {
    revokeTripShare(command: { tripId: $tripId, tripShareId: $tripShareId })
  }
`);

export const AddTripStopDocument = graphql(`
  mutation AddTripStop($tripId: UUID!, $stop: TripStopDtoInput!) {
    addTripStop(command: { tripId: $tripId, stop: $stop }) {
      ...TripStopFields
    }
  }
`);

export const UpdateTripStopDocument = graphql(`
  mutation UpdateTripStop($tripStopId: UUID!, $stop: TripStopDtoInput!) {
    updateTripStop(command: { tripStopId: $tripStopId, stop: $stop })
  }
`);

export const RemoveTripStopDocument = graphql(`
  mutation RemoveTripStop($id: UUID!) {
    removeTripStop(id: $id)
  }
`);

export const ReorderTripStopsDocument = graphql(`
  mutation ReorderTripStops($tripId: UUID!, $orderedStopIds: [UUID!]!) {
    reorderTripStops(command: { tripId: $tripId, orderedStopIds: $orderedStopIds })
  }
`);

export const RecordStopArrivalDocument = graphql(`
  mutation RecordStopArrival(
    $tripId: UUID!
    $tripStopId: UUID!
    $occurredAt: DateTime!
    $clientEventId: UUID!
  ) {
    recordStopArrival(
      command: {
        tripId: $tripId
        tripStopId: $tripStopId
        occurredAt: $occurredAt
        clientEventId: $clientEventId
      }
    )
  }
`);

export const RecordStopDepartureDocument = graphql(`
  mutation RecordStopDeparture(
    $tripId: UUID!
    $tripStopId: UUID!
    $occurredAt: DateTime!
    $clientEventId: UUID!
  ) {
    recordStopDeparture(
      command: {
        tripId: $tripId
        tripStopId: $tripStopId
        occurredAt: $occurredAt
        clientEventId: $clientEventId
      }
    )
  }
`);

export const SkipStopDocument = graphql(`
  mutation SkipStop(
    $tripId: UUID!
    $tripStopId: UUID!
    $occurredAt: DateTime!
    $reason: String!
    $clientEventId: UUID!
  ) {
    skipStop(
      command: {
        tripId: $tripId
        tripStopId: $tripStopId
        occurredAt: $occurredAt
        reason: $reason
        clientEventId: $clientEventId
      }
    )
  }
`);

/* ------------------------------------------------------ delivery mutations */

export const CreateDeliveryDocument = graphql(`
  mutation CreateDelivery($tripStopId: UUID!, $delivery: DeliveryDtoInput!) {
    createDelivery(command: { tripStopId: $tripStopId, delivery: $delivery }) {
      ...DeliveryFields
    }
  }
`);

export const UpdateDeliveryDocument = graphql(`
  mutation UpdateDelivery($deliveryId: UUID!, $delivery: DeliveryDtoInput!) {
    updateDelivery(command: { deliveryId: $deliveryId, delivery: $delivery })
  }
`);

/**
 * Records the outcome of one delivery. `clientEventId` is the server-side
 * idempotency key — a retry of the SAME attempt must reuse it (spec 11 §7.3).
 */
export const UpdateDeliveryOutcomeDocument = graphql(`
  mutation UpdateDeliveryOutcome(
    $tripId: UUID!
    $deliveryId: UUID!
    $status: String!
    $observations: String
    $clientEventId: UUID!
  ) {
    updateDeliveryOutcome(
      command: {
        tripId: $tripId
        deliveryId: $deliveryId
        status: $status
        observations: $observations
        clientEventId: $clientEventId
      }
    )
  }
`);

export const DeleteDeliveryDocument = graphql(`
  mutation DeleteDelivery($id: UUID!) {
    deleteDelivery(id: $id)
  }
`);

/* ----------------------------------------------------------- POD mutation */

/**
 * Captures proof of delivery. Documents are ordinary spec 04 Manager documents
 * uploaded through the existing REST surface and referenced here by id — the
 * backend rejects any that is not `ScanStatus = Clean` with
 * `POD_DOCUMENT_NOT_CLEAN`. Idempotent on `(tripStopId, clientEventId)`.
 */
export const RecordProofOfDeliveryDocument = graphql(`
  mutation RecordProofOfDelivery($tripId: UUID!, $proofOfDelivery: ProofOfDeliveryDtoInput!) {
    recordProofOfDelivery(command: { tripId: $tripId, proofOfDelivery: $proofOfDelivery }) {
      ...ProofOfDeliveryFields
    }
  }
`);

/* ------------------------------------------------- toll catalog mutations */

/**
 * Account-scoped transporter-type (or single-transporter) → toll-class mapping.
 * NOT part of the platform toll catalog: it runs under `Resources.Trips`/`Edit`
 * and is what lets `Trip.TollVehicleClass` be defaulted at trip creation
 * (spec 11 §4, §7.6).
 */
export const SetTransporterTollClassDocument = graphql(`
  mutation SetTransporterTollClass(
    $transporterTypeId: Short
    $transporterId: UUID
    $tollVehicleClassCode: String!
  ) {
    setTransporterTollClass(
      command: {
        transporterTypeId: $transporterTypeId
        transporterId: $transporterId
        tollVehicleClassCode: $tollVehicleClassCode
      }
    ) {
      ...TransporterTollClassFields
    }
  }
`);

export const CreateTollVehicleClassDocument = graphql(`
  mutation CreateTollVehicleClass($vehicleClass: TollVehicleClassDtoInput!) {
    createTollVehicleClass(command: { vehicleClass: $vehicleClass }) {
      ...TollVehicleClassFields
    }
  }
`);

export const UpdateTollVehicleClassDocument = graphql(`
  mutation UpdateTollVehicleClass(
    $tollVehicleClassId: UUID!
    $vehicleClass: TollVehicleClassDtoInput!
  ) {
    updateTollVehicleClass(
      command: { tollVehicleClassId: $tollVehicleClassId, vehicleClass: $vehicleClass }
    )
  }
`);

export const DeactivateTollVehicleClassDocument = graphql(`
  mutation DeactivateTollVehicleClass($id: UUID!) {
    deactivateTollVehicleClass(id: $id)
  }
`);

export const CreateTollStationDocument = graphql(`
  mutation CreateTollStation($station: TollStationDtoInput!) {
    createTollStation(command: { station: $station }) {
      ...TollStationFields
    }
  }
`);

export const UpdateTollStationDocument = graphql(`
  mutation UpdateTollStation($tollStationId: UUID!, $station: TollStationDtoInput!) {
    updateTollStation(command: { tollStationId: $tollStationId, station: $station })
  }
`);

export const DeactivateTollStationDocument = graphql(`
  mutation DeactivateTollStation($id: UUID!) {
    deactivateTollStation(id: $id)
  }
`);

export const CreateTollTariffDocument = graphql(`
  mutation CreateTollTariff($tariff: TollTariffDtoInput!) {
    createTollTariff(command: { tariff: $tariff }) {
      ...TollTariffFields
    }
  }
`);

export const UpdateTollTariffDocument = graphql(`
  mutation UpdateTollTariff($tollTariffId: UUID!, $tariff: TollTariffDtoInput!) {
    updateTollTariff(command: { tollTariffId: $tollTariffId, tariff: $tariff })
  }
`);

export const DeleteTollTariffDocument = graphql(`
  mutation DeleteTollTariff($id: UUID!) {
    deleteTollTariff(id: $id)
  }
`);

/**
 * Bulk trip planning from a spreadsheet (spec 11a §9.1). Per-row results: one bad
 * row never fails the batch, and the dialog renders the rejected rows so the
 * operator fixes those lines rather than guessing which of three hundred was wrong.
 */
export const ImportTripsCsvDocument = graphql(`
  mutation ImportTripsCsv($csv: String!) {
    importTripsCsv(command: { csv: $csv }) {
      rowsRead
      tripsCreated
      errors {
        rowNumber
        errorCode
        message
      }
    }
  }
`);

/**
 * "This trip is already under way" (spec 11a §5.4). `startedAt` is the FALLBACK,
 * not the input: when Geofencing recorded the vehicle leaving the origin zone,
 * those measurements win and `backfilled` comes back true.
 */
export const DeclareTripInTransitDocument = graphql(`
  mutation DeclareTripInTransit($tripId: UUID!, $startedAt: DateTime) {
    declareTripInTransit(command: { tripId: $tripId, startedAt: $startedAt }) {
      started
      backfilled
      startedAt
      stopsReplayed
    }
  }
`);

export const ImportTollCatalogDocument = graphql(`
  mutation ImportTollCatalog($csv: String!) {
    importTollCatalog(command: { csv: $csv }) {
      rowsRead
      stationsCreated
      stationsUpdated
      tariffsCreated
      errors {
        rowNumber
        errorCode
        message
      }
    }
  }
`);
