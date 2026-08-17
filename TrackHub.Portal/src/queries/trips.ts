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
 * Trip / toll-catalog query and mutation hooks. Components consume these — not
 * the api layer directly. Loading and error state come from the hooks; failures
 * also surface in the global toast via the query client's error handlers.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/tripManagement/trips';
import * as publicApi from 'api/tripManagement/publicTrips';
import type {
  TripListFilters,
  TripDtoInput,
  TripStopDtoInput,
  DeliveryDtoInput,
  ProofOfDeliveryDtoInput,
  TripShareFieldFlagsDtoInput,
  TollStationFilters,
  TollStationDtoInput,
  TollTariffDtoInput,
  TollVehicleClassDtoInput,
} from 'api/tripManagement/trips';
import type { PublicTripLinkParams } from 'api/tripManagement/publicTrips';

export const tripKeys = {
  all: ['trips'] as const,
  list: (filters: TripListFilters = {}) => [...tripKeys.all, 'list', filters] as const,
  detail: (tripId: string) => [...tripKeys.all, 'detail', tripId] as const,
  timeline: (tripId: string, skip: number, take: number) =>
    [...tripKeys.all, 'timeline', tripId, skip, take] as const,
  replay: (tripId: string, maxPoints: number | null) =>
    [...tripKeys.all, 'replay', tripId, maxPoints ?? ''] as const,
  tolls: ['tolls'] as const,
  vehicleClasses: () => [...tripKeys.tolls, 'vehicleClasses'] as const,
  stations: (filters: TollStationFilters = {}) => [...tripKeys.tolls, 'stations', filters] as const,
  stationDetail: (tollStationId: string) =>
    [...tripKeys.tolls, 'stationDetail', tollStationId] as const,
  estimate: (routePlanId: string, tollVehicleClass: string | null) =>
    [...tripKeys.tolls, 'estimate', routePlanId, tollVehicleClass ?? ''] as const,
  publicTrip: (params: PublicTripLinkParams | null) =>
    ['publicTrip', params?.publicLinkGrantId ?? '', params?.resourceId ?? ''] as const,
};

/* ------------------------------------------------------------------ trips */

/** Server-paged dispatch board. Every filter travels to the server — nothing is filtered client-side. */
/**
 * How often the dispatch board re-reads itself.
 *
 * Polling is not a nicety here, it is a requirement of zero-touch: trips now start,
 * advance and complete without any mutation of this dispatcher's making, so nothing
 * invalidates the cache on their behalf. Without it the board would show a truck as
 * "scheduled" for as long as the tab stayed open (spec 11a §10).
 *
 * 30 s rather than the status page's 60 s — this is an operational screen someone
 * watches while making decisions, not a health dashboard.
 */
export const TRIP_BOARD_POLL_MS = 30_000;

export function useTrips(filters: TripListFilters = {}, options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: tripKeys.list(filters),
    queryFn: () => api.getTrips(filters),
    enabled: options.enabled ?? true,
    refetchInterval: TRIP_BOARD_POLL_MS,
    // A backgrounded tab stops hitting the backend; the focus refetch below is what
    // makes coming back to it show the truth immediately rather than up to 30 s late.
    refetchIntervalInBackground: false,
    refetchOnWindowFocus: true,
  });
}

export function useTripDetail(tripId: string | null | undefined) {
  return useQuery({
    queryKey: tripKeys.detail(tripId ?? ''),
    queryFn: () => api.getTripDetail(tripId as string),
    enabled: !!tripId,
    // The workspace tracks one trip in flight — its stops close without anyone here
    // pressing anything, so it polls on the same cadence as the board.
    refetchInterval: TRIP_BOARD_POLL_MS,
    refetchIntervalInBackground: false,
    refetchOnWindowFocus: true,
  });
}

export function useTripTimeline(tripId: string | null | undefined, skip = 0, take = 100) {
  return useQuery({
    queryKey: tripKeys.timeline(tripId ?? '', skip, take),
    queryFn: () => api.getTripTimeline(tripId as string, skip, take),
    enabled: !!tripId,
  });
}

/** Replay points. The result's `truncated` flag must be rendered, never swallowed. */
export function useTripRouteReplay(
  tripId: string | null | undefined,
  maxPoints: number | null = null,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: tripKeys.replay(tripId ?? '', maxPoints),
    queryFn: () => api.getTripRouteReplay(tripId as string, maxPoints),
    enabled: (options.enabled ?? true) && !!tripId,
  });
}

export function useCreateTrip() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (trip: TripDtoInput) => api.createTrip(trip),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

export function useUpdateTrip() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ tripId, trip }: { tripId: string; trip: TripDtoInput }) =>
      api.updateTrip(tripId, trip),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

export function useDeleteTrip() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (tripId: string) => api.deleteTrip(tripId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

export function useAssignTrip() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      tripId,
      driverId,
      transporterId,
    }: {
      tripId: string;
      driverId: string;
      transporterId?: string | null;
    }) => api.assignTrip(tripId, driverId, transporterId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

/**
 * Requests a route plan. A routing failure resolves with `status: 'Failed'`
 * rather than rejecting, so `onSuccess` still invalidates and the screen reads
 * the returned plan's status to decide what to show.
 */
export function usePlanTripRoute() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      tripId,
      corridorMeters,
      tollVehicleClass,
    }: {
      tripId: string;
      corridorMeters?: number | null;
      tollVehicleClass?: string | null;
    }) => api.planTripRoute(tripId, corridorMeters, tollVehicleClass),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

/** One hook for every lifecycle transition; the action name selects the mutation. */
export type TripLifecycleAction = 'start' | 'pause' | 'resume' | 'complete' | 'cancel' | 'abort';

export function useTripLifecycle() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      action,
      tripId,
      reason,
      force,
    }: {
      action: TripLifecycleAction;
      tripId: string;
      reason?: string;
      force?: boolean;
    }) => {
      switch (action) {
        case 'start':
          return api.startTrip(tripId);
        case 'pause':
          return api.pauseTrip(tripId);
        case 'resume':
          return api.resumeTrip(tripId);
        case 'complete':
          return api.completeTrip(tripId, force ?? false);
        case 'cancel':
          return api.cancelTrip(tripId, reason ?? '');
        case 'abort':
          return api.abortTrip(tripId, reason ?? '');
      }
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

/* ------------------------------------------------------------------ stops */

export function useAddTripStop() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ tripId, stop }: { tripId: string; stop: TripStopDtoInput }) =>
      api.addTripStop(tripId, stop),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

export function useUpdateTripStop() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ tripStopId, stop }: { tripStopId: string; stop: TripStopDtoInput }) =>
      api.updateTripStop(tripStopId, stop),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

export function useRemoveTripStop() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (tripStopId: string) => api.removeTripStop(tripStopId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

export function useReorderTripStops() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ tripId, orderedStopIds }: { tripId: string; orderedStopIds: string[] }) =>
      api.reorderTripStops(tripId, orderedStopIds),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

/** Stop progress recorded by a dispatcher. `clientEventId` makes each call idempotent. */
export type StopProgressAction = 'arrive' | 'depart' | 'skip';

export function useStopProgress() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      action,
      tripId,
      tripStopId,
      occurredAt,
      clientEventId,
      reason,
    }: {
      action: StopProgressAction;
      tripId: string;
      tripStopId: string;
      occurredAt: string;
      clientEventId: string;
      reason?: string;
    }) => {
      switch (action) {
        case 'arrive':
          return api.recordStopArrival(tripId, tripStopId, occurredAt, clientEventId);
        case 'depart':
          return api.recordStopDeparture(tripId, tripStopId, occurredAt, clientEventId);
        case 'skip':
          return api.skipStop(tripId, tripStopId, occurredAt, reason ?? '', clientEventId);
      }
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

/* ------------------------------------------------------------- deliveries */

export function useCreateDelivery() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ tripStopId, delivery }: { tripStopId: string; delivery: DeliveryDtoInput }) =>
      api.createDelivery(tripStopId, delivery),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

export function useUpdateDelivery() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ deliveryId, delivery }: { deliveryId: string; delivery: DeliveryDtoInput }) =>
      api.updateDelivery(deliveryId, delivery),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

/**
 * Records a delivery outcome. The caller owns `clientEventId` precisely so a
 * retry after a failure can reuse it — minting a fresh id per attempt would
 * defeat the server's idempotency (spec 11 §7.3).
 */
export function useUpdateDeliveryOutcome() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      tripId,
      deliveryId,
      status,
      observations,
      clientEventId,
    }: {
      tripId: string;
      deliveryId: string;
      status: string;
      observations?: string | null;
      clientEventId: string;
    }) => api.updateDeliveryOutcome(tripId, deliveryId, status, observations ?? null, clientEventId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

export function useDeleteDelivery() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (deliveryId: string) => api.deleteDelivery(deliveryId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

/* -------------------------------------------------------------------- POD */

/**
 * Captures proof of delivery. Invalidates the whole trip namespace: a POD also
 * flips the stop's deliveries to `Delivered` and appends a timeline event, so
 * the detail, the timeline and the board all go stale at once.
 */
export function useRecordProofOfDelivery() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      tripId,
      proofOfDelivery,
    }: {
      tripId: string;
      proofOfDelivery: ProofOfDeliveryDtoInput;
    }) => api.recordProofOfDelivery(tripId, proofOfDelivery),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

/* ---------------------------------------------- transporter → toll class  */

/**
 * Account-scoped transporter-type / transporter → toll-class mapping. It feeds
 * `Trip.TollVehicleClass` defaulting at trip creation, so both the trip
 * namespace and the toll namespace go stale.
 */
export function useSetTransporterTollClass() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      transporterTypeId,
      transporterId,
      tollVehicleClassCode,
    }: {
      transporterTypeId?: number | null;
      transporterId?: string | null;
      tollVehicleClassCode: string;
    }) =>
      api.setTransporterTollClass(
        transporterTypeId ?? null,
        transporterId ?? null,
        tollVehicleClassCode
      ),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: tripKeys.all });
      await queryClient.invalidateQueries({ queryKey: tripKeys.tolls });
    },
  });
}

/* ------------------------------------------------------------------ share */

export function useShareTrip() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      tripId,
      expiresAt,
      purpose,
      fieldFlags,
    }: {
      tripId: string;
      expiresAt: string;
      purpose: string;
      fieldFlags: TripShareFieldFlagsDtoInput;
    }) => api.shareTrip(tripId, expiresAt, purpose, fieldFlags),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

export function useRevokeTripShare() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ tripId, tripShareId }: { tripId: string; tripShareId: string }) =>
      api.revokeTripShare(tripId, tripShareId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

/* ---------------------------------------------------------- toll catalog  */

export function useTollVehicleClasses(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: tripKeys.vehicleClasses(),
    queryFn: api.getTollVehicleClasses,
    enabled: options.enabled ?? true,
  });
}

export function useTollStations(
  filters: TollStationFilters = {},
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: tripKeys.stations(filters),
    queryFn: () => api.getTollStations(filters),
    enabled: options.enabled ?? true,
  });
}

/** Station plus its full tariff history — superseded rows stay visible (acceptance 21). */
export function useTollStationDetail(tollStationId: string | null | undefined) {
  return useQuery({
    queryKey: tripKeys.stationDetail(tollStationId ?? ''),
    queryFn: () => api.getTollStationDetail(tollStationId as string),
    enabled: !!tollStationId,
  });
}

export function useTollEstimate(
  routePlanId: string | null | undefined,
  tollVehicleClass: string | null = null,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: tripKeys.estimate(routePlanId ?? '', tollVehicleClass),
    queryFn: () => api.estimateTolls(routePlanId as string, tollVehicleClass),
    enabled: (options.enabled ?? true) && !!routePlanId,
  });
}

export function useCreateTollVehicleClass() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (vehicleClass: TollVehicleClassDtoInput) =>
      api.createTollVehicleClass(vehicleClass),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.tolls }),
  });
}

export function useUpdateTollVehicleClass() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      tollVehicleClassId,
      vehicleClass,
    }: {
      tollVehicleClassId: string;
      vehicleClass: TollVehicleClassDtoInput;
    }) => api.updateTollVehicleClass(tollVehicleClassId, vehicleClass),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.tolls }),
  });
}

export function useDeactivateTollVehicleClass() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (tollVehicleClassId: string) => api.deactivateTollVehicleClass(tollVehicleClassId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.tolls }),
  });
}

export function useCreateTollStation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (station: TollStationDtoInput) => api.createTollStation(station),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.tolls }),
  });
}

export function useUpdateTollStation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      tollStationId,
      station,
    }: {
      tollStationId: string;
      station: TollStationDtoInput;
    }) => api.updateTollStation(tollStationId, station),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.tolls }),
  });
}

export function useDeactivateTollStation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (tollStationId: string) => api.deactivateTollStation(tollStationId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.tolls }),
  });
}

export function useCreateTollTariff() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (tariff: TollTariffDtoInput) => api.createTollTariff(tariff),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.tolls }),
  });
}

export function useUpdateTollTariff() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ tollTariffId, tariff }: { tollTariffId: string; tariff: TollTariffDtoInput }) =>
      api.updateTollTariff(tollTariffId, tariff),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.tolls }),
  });
}

export function useDeleteTollTariff() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (tollTariffId: string) => api.deleteTollTariff(tollTariffId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.tolls }),
  });
}

export function useImportTollCatalog() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (csv: string) => api.importTollCatalog(csv),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.tolls }),
  });
}

/** Bulk trip planning (spec 11a §9.1). Row-level failures come back in the result, never as a throw. */
export function useImportTripsCsv() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (csv: string) => api.importTripsCsv(csv),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

/** "This trip is already under way" (spec 11a §5.4). */
export function useDeclareTripInTransit() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ tripId, startedAt }: { tripId: string; startedAt: string | null }) =>
      api.declareTripInTransit(tripId, startedAt),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tripKeys.all }),
  });
}

/* --------------------------------------------------------- public tracking */

/**
 * Anonymous tracking snapshot. `retry: false` because 404/410 are final answers,
 * not transient failures worth hammering an anonymous rate-limited endpoint over.
 */
export function usePublicTrip(params: PublicTripLinkParams | null) {
  return useQuery({
    queryKey: tripKeys.publicTrip(params),
    queryFn: () => publicApi.getPublicTrip(params as PublicTripLinkParams),
    enabled: !!params,
    retry: false,
  });
}
