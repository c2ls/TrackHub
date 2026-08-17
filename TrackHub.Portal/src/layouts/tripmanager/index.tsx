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

import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import Grid from '@mui/material/Grid';
import Icon from '@mui/material/Icon';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import DashboardLayout from 'controls/LayoutContainers/DashboardLayout';
import DashboardNavbar from 'controls/Navbars/DashboardNavbar';
import Footer from 'controls/Footer';
import Table from 'controls/Tables/Table';
import CompactSelect from 'controls/Selects/CompactSelect';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import FormDialog from 'controls/Dialogs/FormDialog';
import CustomTextField from 'controls/Dialogs/CustomTextField';
import CustomCheckbox from 'controls/Dialogs/CustomCheckbox';
import useForm from 'controls/Dialogs/useForm';
import { Name, Description } from 'controls/Tables/components/tableComponents';
import ArgonBox from 'components/ArgonBox';
import ArgonBadge from 'components/ArgonBadge';
import ArgonButton from 'components/ArgonButton';
import ArgonPagination from 'components/ArgonPagination';
import ArgonTypography from 'components/ArgonTypography';
import { useArgonController } from 'context';
import { usePermissions } from 'context/permissions';
import { PermissionResources, PermissionActions } from 'constants/permissions';
import { useAccountByUser } from 'queries/accounts';
import { useTransporterLookupByUser } from 'queries/transporters';
import { useDriversByAccount } from 'queries/drivers';
import { usePointOfInterestLookup } from 'queries/pointsOfInterest';
import { useAllGeofences } from 'queries/geofences';
import {
  useTrips,
  useTripDetail,
  useCreateTrip,
  useUpdateTrip,
  useDeleteTrip,
  useAssignTrip,
  usePlanTripRoute,
  useTripLifecycle,
  useAddTripStop,
  useUpdateTripStop,
  useRemoveTripStop,
  useReorderTripStops,
  useStopProgress,
  useTollVehicleClasses,
  useCreateDelivery,
  useUpdateDelivery,
  useUpdateDeliveryOutcome,
  useDeleteDelivery,
  useRecordProofOfDelivery,
  useSetTransporterTollClass,
  useDeclareTripInTransit,
} from 'queries/trips';
import { useUploadDocument, useRefreshDocument } from 'queries/documents';
import { getAccountSettings } from 'api/manager/settings';
import { notifyApiError } from 'api/core/errors';
import { formatDateTime } from 'utils/dateUtils';
import { MAP_PROVIDERS } from 'controls/Maps/core/MapProviderContext';
import type { MapProvider } from 'controls/Maps/core/MapProviderContext';
import type { RoutePoint } from 'controls/Maps/core/mapTypes';
import { getTripDetail } from 'api/tripManagement/trips';
import type {
  TripListFilters,
  TripStopDtoInput,
  TripDtoInput,
  TripDelivery,
  TransporterTollClass,
  Trip,
} from 'api/tripManagement/trips';
import {
  toIso,
  toLocalInput,
  newClientEventId,
  buildDeliveryPayload,
  buildPodPayload,
  buildStopPayloadFromDestination,
  buildTollClassVariables,
  DEFAULT_ARRIVAL_RADIUS_METERS,
  deriveTripType,
  destinationsFromStops,
  podDocumentFields,
  normalizeStopCity,
  isStopCityWithinLimit,
  returnToOriginStop,
  hasException,
  normalizeStopActivity,
  STOP_CITY_MAX_LENGTH,
  TRIP_EXCEPTIONS,
  DELIVERY_REQUIRED_FIELDS,
  POD_REQUIRED_FIELDS,
} from './tripWriteForms';
import type {
  DeliveryFormValues,
  DeliveryStatus,
  PodAttachment,
  PodFormValues,
  TollClassFormValues,
  TripDestinationDraft,
  TripException,
} from './tripWriteForms';
import TripDialog from './components/TripDialog';
import type { TripFormValues } from './components/TripDialog';
import StopDialog from './components/StopDialog';
import type { StopFormValues } from './components/StopDialog';
import RoutePlanner, { DEFAULT_CORRIDOR_METERS } from './components/RoutePlanner';
import TollPanel from './components/TollPanel';
import AssignmentPanel from './components/AssignmentPanel';
import ShareDialog from './components/ShareDialog';
import TripDetail from './components/TripDetail';
import DeliveryDialog from './components/DeliveryDialog';
import DeliveryOutcomeDialog from './components/DeliveryOutcomeDialog';
import PodDialog from './components/PodDialog';
import TollClassDialog from './components/TollClassDialog';
import TripImportDialog from './components/TripImportDialog';

const PAGE_SIZE = 10;
const ALL = 'all';

/**
 * How many trips the board scans when an exception filter is on. Exceptions are derived
 * client-side, so the wider window is what makes the answer trustworthy; it is bounded
 * because "every trip ever" is not a board, and the other filters (status, unit, dates)
 * still narrow it first.
 */
const EXCEPTION_SCAN_SIZE = 200;

/**
 * Feed for the create dialog's "reuse a previous route" picker. A stable
 * reference so the query key never churns; 50 recent trips is plenty for a
 * dispatcher re-running known routes (full templates are spec 11 slice 2).
 */
const COPY_SOURCE_FILTERS: TripListFilters = { take: 50 };
const TRIP_STATUSES = ['Created', 'InProgress', 'Paused', 'Completed', 'Cancelled', 'Aborted'] as const;

/** Statuses whose trips are still being planned — stops and routes stay editable. */
const EDITABLE_STATUSES = new Set(['Created', 'Paused']);

/** Statuses that still accept delivery/POD writes — the backend's own rule is "not terminal". */
const TERMINAL_STATUSES = new Set(['Completed', 'Cancelled', 'Aborted']);

/**
 * The trip workspace is tabbed rather than stacked.
 *
 * Rendering every section at once made this screen roughly five viewports tall
 * and put TWO large maps (the planner's 52vh and the replay's 45vh) plus a POD
 * image gallery on screen together. Tabs keep the workspace to one screen, and
 * because each panel only mounts when it is opened, the replay query and the
 * second map cost nothing until a dispatcher actually asks for them.
 *
 * Ordered by how a trip is worked: plan it, crew it, run the stops, then look
 * at what it cost and what it left behind.
 */
const WORKSPACE_TABS = ['route', 'assignment', 'stops', 'tolls', 'pod', 'timeline', 'replay'] as const;
type WorkspaceTab = (typeof WORKSPACE_TABS)[number];

/**
 * Dispatch board and trip workspace.
 *
 * The board is SERVER-paged and server-filtered: every filter travels in the
 * `trips` query, so the page a dispatcher sees is the page the backend built —
 * group visibility, feature gating and paging all stay on the server side.
 */
function TripManager() {
  const { t } = useTranslation();
  const [controller] = useArgonController();
  const { darkMode } = controller;

  /* ------------------------------------------------------------- settings */

  const [mapSettings, setMapSettings] = useState<{ maps: MapProvider; mapsKey: string | null }>({
    maps: MAP_PROVIDERS.OSM,
    mapsKey: null,
  });
  useEffect(() => {
    getAccountSettings()
      .then((settings) =>
        setMapSettings({
          maps: settings.maps === MAP_PROVIDERS.GOOGLE ? MAP_PROVIDERS.GOOGLE : MAP_PROVIDERS.OSM,
          mapsKey: settings.mapsKey,
        })
      )
      .catch(notifyApiError);
  }, []);

  /* -------------------------------------------------------------- filters */

  const [page, setPage] = useState(0);
  const [status, setStatus] = useState<string>(ALL);
  const [exception, setException] = useState<string>(ALL);
  const [transporterFilter, setTransporterFilter] = useState<string>(ALL);
  const [driverFilter, setDriverFilter] = useState<string>(ALL);
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [search, setSearch] = useState('');

  const filteringExceptions = exception !== ALL;

  const filters = useMemo<TripListFilters>(
    () => ({
      statuses: status === ALL ? null : [status],
      from: toIso(from),
      to: toIso(to),
      transporterId: transporterFilter === ALL ? null : transporterFilter,
      driverId: driverFilter === ALL ? null : driverFilter,
      search: search.trim() || null,
      // Exceptions are DERIVED from the phase and the measured timestamps, so the server
      // cannot filter on them in SQL — the phase does not exist until the rows are read.
      // Filtering ten rows at a time would therefore be a trap: a dispatcher asking "what
      // is overdue?" would be told "nothing" while page two was full of it. So the
      // exception view fetches one wide page and pages through it here instead.
      skip: filteringExceptions ? 0 : page * PAGE_SIZE,
      take: filteringExceptions ? EXCEPTION_SCAN_SIZE : PAGE_SIZE,
    }),
    [status, from, to, transporterFilter, driverFilter, search, page, filteringExceptions]
  );

  /* --------------------------------------------------------------- data */

  const accountQuery = useAccountByUser();
  const accountId = accountQuery.data?.accountId;
  // The transporter lookup carries the type columns TollClassDialog derives its
  // transporter-TYPE list from, so the picker feed is enough — no full drain.
  const transportersQuery = useTransporterLookupByUser();
  const transporters = useMemo(() => transportersQuery.data ?? [], [transportersQuery.data]);
  const driversQuery = useDriversByAccount(accountId, { enabled: !!accountId });
  const drivers = useMemo(() => driversQuery.data ?? [], [driversQuery.data]);
  // The POI lookup carries the pin colour and the popup's type/description/address
  // that RoutePlanner renders, so the picker feed is enough — no full drain.
  const poisQuery = usePointOfInterestLookup();
  const pois = useMemo(() => poisQuery.data ?? [], [poisQuery.data]);
  const geofencesQuery = useAllGeofences(false, { active: true });
  const geofences = useMemo(() => geofencesQuery.data ?? [], [geofencesQuery.data]);
  const vehicleClassesQuery = useTollVehicleClasses();
  const vehicleClasses = useMemo(
    () => vehicleClassesQuery.data ?? [],
    [vehicleClassesQuery.data]
  );

  const tripsQuery = useTrips(filters);
  const trips = useMemo(() => tripsQuery.data?.items ?? [], [tripsQuery.data]);
  const totalCount = tripsQuery.data?.totalCount ?? 0;

  const [selectedTripId, setSelectedTripId] = useState<string | null>(null);

  // Reset to the first tab when the dispatcher picks a different trip: leaving it
  // on, say, POD would open the next trip on a panel that is usually empty and
  // says nothing about why that trip was selected.
  const [workspaceTab, setWorkspaceTab] = useState<WorkspaceTab>('route');
  useEffect(() => setWorkspaceTab('route'), [selectedTripId]);
  const detailQuery = useTripDetail(selectedTripId);
  const detail = detailQuery.data;

  /* ---------------------------------------------------------- permissions */

  // Trips/Delete is NOT in the default User role matrix (the dispatcher creates and
  // runs trips; retiring one is a Manager action), so the three destructive controls
  // on this screen are permission-gated rather than always rendered. A user elevated
  // by a policy that grants Trips/Delete sees them.
  const { can } = usePermissions();
  const canDeleteTrips = can(PermissionResources.Trips, PermissionActions.Delete);

  /* ---------------------------------------------------------- mutations */

  const createTrip = useCreateTrip();
  const updateTrip = useUpdateTrip();
  const deleteTrip = useDeleteTrip();
  const assignTrip = useAssignTrip();
  const planRoute = usePlanTripRoute();
  const lifecycle = useTripLifecycle();
  const addStop = useAddTripStop();
  const updateStop = useUpdateTripStop();
  const removeStop = useRemoveTripStop();
  const reorderStops = useReorderTripStops();
  const stopProgress = useStopProgress();
  const createDelivery = useCreateDelivery();
  const updateDelivery = useUpdateDelivery();
  const updateDeliveryOutcome = useUpdateDeliveryOutcome();
  const deleteDelivery = useDeleteDelivery();
  const recordPod = useRecordProofOfDelivery();
  const setTransporterTollClass = useSetTransporterTollClass();
  const uploadDocument = useUploadDocument();
  const refreshDocument = useRefreshDocument();

  /* -------------------------------------------------------- trip dialog */

  const [tripOpen, setTripOpen] = useState(false);
  const [tripValues, tripChange, setTripValues, setTripErrors, validateTrip, tripErrors] =
    useForm<TripFormValues>({});
  // Destinations queued in the create dialog; they become stops right after the
  // trip header is created (the create command owns only the header, §7.3).
  const [tripDestinations, setTripDestinations] = useState<TripDestinationDraft[]>([]);

  // Reusable-route feed for the create dialog, fetched only while it is open.
  const copySourcesQuery = useTrips(COPY_SOURCE_FILTERS, {
    enabled: tripOpen && !tripValues.tripId,
  });
  const copySources = useMemo(
    () => copySourcesQuery.data?.items ?? [],
    [copySourcesQuery.data]
  );

  /** Prefills origin, route shape and toll class from an existing trip. */
  const applyTripTemplate = async (sourceTripId: string) => {
    try {
      const source = await getTripDetail(sourceTripId);
      const template = destinationsFromStops(
        source.stops,
        source.trip.originLatitude,
        source.trip.originLongitude
      );
      setTripValues((previous) => ({
        ...previous,
        originName: source.trip.originName,
        originLatitude: source.trip.originLatitude,
        originLongitude: source.trip.originLongitude,
        originGeofenceId: template.originGeofenceId,
        originPoiId: null,
        tollVehicleClass: source.trip.tollVehicleClass ?? previous.tollVehicleClass,
        tripType: template.tripType,
      }));
      setTripDestinations(template.destinations);
    } catch (error) {
      notifyApiError(error);
    }
  };

  const openTrip = (edit = false) => {
    if (edit && detail) {
      setTripValues({
        tripId: detail.trip.tripId,
        code: detail.trip.code,
        transporterId: detail.trip.transporterId,
        driverId: detail.trip.driverId ?? '',
        externalReference: detail.trip.externalReference,
        customerName: detail.trip.customerName,
        originName: detail.trip.originName,
        originLatitude: detail.trip.originLatitude,
        originLongitude: detail.trip.originLongitude,
        // Seeded from the STORED column now that the origin zone is real: leaving it
        // blank would save the trip back with its geofence link dropped, and the
        // origin would silently fall from the plant's real shape to a 150 m circle.
        originGeofenceId: detail.trip.originGeofenceId,
        plannedStartAt: toLocalInput(detail.trip.plannedStartAt),
        plannedEndAt: toLocalInput(detail.trip.plannedEndAt),
        notes: detail.trip.notes,
        tollVehicleClass: detail.trip.tollVehicleClass,
      });
    } else {
      setTripValues({
        plannedStartAt: toLocalInput(new Date().toISOString()),
        tripType: 'single',
      });
    }
    setTripDestinations([]);
    setTripErrors({});
    setTripOpen(true);
  };

  const saveTrip = async () => {
    if (!validateTrip(['code', 'transporterId', 'originName', 'plannedStartAt'])) {
      return;
    }
    const creating = !tripValues.tripId;
    const tripType = tripValues.tripType ?? 'single';
    // Places, not fields: the origin must have been PICKED (geofence or POI),
    // and a new trip is a route — it needs at least one destination. Extra stop
    // detail (arrival windows, POD flags, reordering) belongs to the planner.
    const originLatitude = Number(tripValues.originLatitude);
    const originLongitude = Number(tripValues.originLongitude);
    const hasOrigin =
      Number.isFinite(originLatitude) &&
      Number.isFinite(originLongitude) &&
      (originLatitude !== 0 || originLongitude !== 0);
    const placementErrors: Record<string, string> = {};
    if (!hasOrigin) {
      placementErrors.origin = t('trips.origin.required');
    }
    if (creating && tripDestinations.length === 0) {
      placementErrors.destinations = t('trips.destinations.required');
    }
    if (creating && tripType === 'single' && tripDestinations.length > 1) {
      placementErrors.destinations = t('trips.destinations.singleLimit');
    }
    if (Object.keys(placementErrors).length > 0) {
      setTripErrors(placementErrors);
      return;
    }
    const payload: TripDtoInput = {
      code: tripValues.code as string,
      transporterId: tripValues.transporterId as string,
      driverId: tripValues.driverId || null,
      serviceOrderId: tripValues.serviceOrderId || null,
      externalReference: tripValues.externalReference || null,
      customerName: tripValues.customerName || null,
      originName: tripValues.originName as string,
      originLatitude: Number(tripValues.originLatitude),
      originLongitude: Number(tripValues.originLongitude),
      originGeofenceId: tripValues.originGeofenceId || null,
      originRadiusMeters: DEFAULT_ARRIVAL_RADIUS_METERS,
      plannedStartAt: toIso(tripValues.plannedStartAt) as string,
      plannedEndAt: toIso(tripValues.plannedEndAt),
      notes: tripValues.notes || null,
      tollVehicleClass: tripValues.tollVehicleClass || null,
    };
    try {
      if (tripValues.tripId) {
        await updateTrip.mutateAsync({ tripId: tripValues.tripId, trip: payload });
      } else {
        const created = await createTrip.mutateAsync(payload);
        // The queued destinations become stops one by one, in list order; a
        // round trip closes with an auto-appended return stop at the origin.
        // A failed stop surfaces in the global toast but does not abandon the
        // rest — the trip gets selected either way, so the planner shows
        // exactly what landed.
        const queued = [...tripDestinations];
        if (tripType === 'round') {
          queued.push(
            returnToOriginStop(
              payload.originName,
              payload.originLatitude,
              payload.originLongitude,
              tripValues.originGeofenceId
            )
          );
        }
        for (const destination of queued) {
          await addStop
            .mutateAsync({
              tripId: created.tripId,
              stop: buildStopPayloadFromDestination(destination),
            })
            .catch(() => undefined);
        }
        setSelectedTripId(created.tripId);
      }
      setTripOpen(false);
    } catch {
      // A duplicate code (TRIP_DUPLICATE_CODE) or a validation failure is shown
      // in the global toast; keep the dialog open so the entry is not lost.
    }
  };

  /* -------------------------------------------------------- stop dialog */

  const [stopOpen, setStopOpen] = useState(false);
  const [placing, setPlacing] = useState(false);
  const [stopValues, stopChange, setStopValues, setStopErrors, validateStop, stopErrors] =
    useForm<StopFormValues>({});

  const openStop = (tripStopId?: string) => {
    const existing = detail?.stops.find((stop) => stop.tripStopId === tripStopId);
    setStopValues(
      existing
        ? {
            tripStopId: existing.tripStopId,
            name: existing.name,
            address: existing.address,
            // Seeded from the stored value, NOT left blank for StopDialog to
            // re-resolve: the reverse-geocode effect only fires for a
            // coordinate pair it has not seen yet, so on a second edit of the
            // same stop it correctly stays silent — and a blank seed would then
            // save the locality away as null. `city` is the only place name the
            // anonymous customer snapshot may carry (spec 11 §7.8), so losing it
            // costs the customer their delivery's locality.
            city: existing.city ?? '',
            latitude: existing.latitude,
            longitude: existing.longitude,
            geofenceId: existing.geofenceId,
            arrivalRadiusMeters: existing.arrivalRadiusMeters,
            activity: normalizeStopActivity(existing.activity),
            plannedArrivalFrom: toLocalInput(existing.plannedArrivalFrom),
            plannedArrivalTo: toLocalInput(existing.plannedArrivalTo),
            requiresPod: existing.requiresPod,
            priority: existing.priority,
            observations: existing.observations,
          }
        : { arrivalRadiusMeters: 150, activity: 'Unload', priority: 0, requiresPod: false }
    );
    setStopErrors({});
    setPlacing(false);
    setStopOpen(true);
  };

  /** "Place on map" hides the dialog and arms the map's click handler. */
  const startPlacing = () => {
    setStopOpen(false);
    setPlacing(true);
  };

  const handleMapClick = (point: RoutePoint) => {
    setStopValues((previous) => ({
      ...previous,
      latitude: point.lat.toFixed(6),
      longitude: point.lng.toFixed(6),
      geofenceId: null,
      address: '',
    }));
    setPlacing(false);
    setStopOpen(true);
  };

  const saveStop = async () => {
    if (!selectedTripId) return;
    if (!validateStop(['name', 'latitude', 'longitude'])) return;
    // Mirror the backend's 200-char cap so an over-long locality is a field
    // message here rather than a 400 from the server.
    if (!isStopCityWithinLimit(stopValues.city)) {
      setStopErrors({ city: t('tripStops.cityTooLong', { max: STOP_CITY_MAX_LENGTH }) });
      return;
    }
    const payload: TripStopDtoInput = {
      name: stopValues.name as string,
      address: stopValues.address || null,
      city: normalizeStopCity(stopValues.city),
      latitude: Number(stopValues.latitude),
      longitude: Number(stopValues.longitude),
      geofenceId: stopValues.geofenceId || null,
      arrivalRadiusMeters: Number(stopValues.arrivalRadiusMeters) || 150,
      activity: normalizeStopActivity(stopValues.activity),
      plannedArrivalFrom: toIso(stopValues.plannedArrivalFrom),
      plannedArrivalTo: toIso(stopValues.plannedArrivalTo),
      requiresPod: !!stopValues.requiresPod,
      priority: Number(stopValues.priority) || 0,
      observations: stopValues.observations || null,
    };
    try {
      if (stopValues.tripStopId) {
        await updateStop.mutateAsync({ tripStopId: stopValues.tripStopId, stop: payload });
      } else {
        await addStop.mutateAsync({ tripId: selectedTripId, stop: payload });
      }
      setStopOpen(false);
    } catch {
      // Surfaced by the global toast.
    }
  };

  /* ---------------------------------------------------- delivery dialogs */

  const stopLabel = (tripStopId?: string | null): string => {
    const stop = detail?.stops.find((candidate) => candidate.tripStopId === tripStopId);
    return stop ? `${stop.sequence}. ${stop.name}` : '';
  };

  const [deliveryOpen, setDeliveryOpen] = useState(false);
  const [
    deliveryValues,
    deliveryChange,
    setDeliveryValues,
    setDeliveryErrors,
    validateDelivery,
    deliveryErrors,
  ] = useForm<DeliveryFormValues>({});

  const openDelivery = (tripStopId: string, delivery?: TripDelivery) => {
    setDeliveryValues(
      delivery
        ? {
            deliveryId: delivery.deliveryId,
            tripStopId: delivery.tripStopId,
            reference: delivery.reference,
            clientName: delivery.clientName,
            branchName: delivery.branchName,
            productsSummary: delivery.productsSummary,
            observations: delivery.observations,
            sequenceIndex: delivery.sequenceIndex,
          }
        : { tripStopId, sequenceIndex: 0 }
    );
    setDeliveryErrors({});
    setDeliveryOpen(true);
  };

  const saveDelivery = async () => {
    if (!validateDelivery(DELIVERY_REQUIRED_FIELDS)) return;
    const payload = buildDeliveryPayload(deliveryValues);
    try {
      if (deliveryValues.deliveryId) {
        await updateDelivery.mutateAsync({ deliveryId: deliveryValues.deliveryId, delivery: payload });
      } else {
        await createDelivery.mutateAsync({
          tripStopId: deliveryValues.tripStopId as string,
          delivery: payload,
        });
      }
      setDeliveryOpen(false);
    } catch {
      // Surfaced by the global toast; the dialog stays open so nothing is lost.
    }
  };

  const [outcomeDelivery, setOutcomeDelivery] = useState<TripDelivery | null>(null);
  const [outcomeStatus, setOutcomeStatus] = useState<DeliveryStatus>('Delivered');
  const [outcomeObservations, setOutcomeObservations] = useState('');
  // Minted once per outcome attempt and REUSED on retry: the command is
  // idempotent on this id, and a fresh id per attempt would defeat that.
  const [outcomeEventId, setOutcomeEventId] = useState('');

  const openOutcome = (delivery: TripDelivery) => {
    setOutcomeDelivery(delivery);
    setOutcomeStatus((delivery.status as DeliveryStatus) ?? 'Delivered');
    setOutcomeObservations(delivery.observations ?? '');
    setOutcomeEventId(newClientEventId());
  };

  const saveOutcome = async () => {
    if (!selectedTripId || !outcomeDelivery) return;
    try {
      await updateDeliveryOutcome.mutateAsync({
        tripId: selectedTripId,
        deliveryId: outcomeDelivery.deliveryId,
        status: outcomeStatus,
        observations: outcomeObservations.trim() || null,
        clientEventId: outcomeEventId,
      });
      setOutcomeDelivery(null);
    } catch {
      // Keep the dialog — and the SAME clientEventId — open for a retry.
    }
  };

  const [deleteDeliveryTarget, setDeleteDeliveryTarget] = useState<TripDelivery | null>(null);

  /* --------------------------------------------------------- POD capture */

  const [podOpen, setPodOpen] = useState(false);
  const [podValues, podChange, setPodValues, setPodErrors, validatePod, podErrors] =
    useForm<PodFormValues>({});
  const [podAttachments, setPodAttachments] = useState<PodAttachment[]>([]);
  // Same idempotency contract as the outcome command, on (tripStopId, clientEventId).
  const [podEventId, setPodEventId] = useState('');

  const openPod = (tripStopId: string) => {
    setPodValues({
      tripStopId,
      deliveryId: '',
      capturedAt: toLocalInput(new Date().toISOString()),
    });
    setPodErrors({});
    setPodAttachments([]);
    setPodEventId(newClientEventId());
    setPodOpen(true);
  };

  /**
   * POD evidence is uploaded through the EXISTING spec 04 document endpoint and
   * linked by id — no POD-specific upload surface exists or is needed. The
   * document is owned by the trip's transporter (spec 11 §11).
   */
  const uploadPodFiles = async (files: File[]) => {
    if (!accountId || !detail) return;
    for (const file of files) {
      try {
        const uploaded = await uploadDocument.mutateAsync({
          file,
          fields: podDocumentFields(accountId, detail.trip.transporterId, file.name),
        });
        setPodAttachments((previous) => [
          ...previous,
          {
            documentId: uploaded.documentId,
            fileName: uploaded.fileName,
            scanStatus: uploaded.scanStatus,
          },
        ]);
      } catch {
        // Surfaced by the global toast; the remaining files still upload.
      }
    }
  };

  /** Re-reads the scan verdict — an upload starts `Pending` and turns `Clean`. */
  const refreshPodAttachment = async (documentId: string) => {
    try {
      const document = await refreshDocument.mutateAsync(documentId);
      setPodAttachments((previous) =>
        previous.map((attachment) =>
          attachment.documentId === documentId
            ? { ...attachment, scanStatus: document.scanStatus }
            : attachment
        )
      );
    } catch {
      // Surfaced by the global toast.
    }
  };

  const savePod = async () => {
    if (!selectedTripId) return;
    if (!validatePod(POD_REQUIRED_FIELDS)) return;
    try {
      await recordPod.mutateAsync({
        tripId: selectedTripId,
        proofOfDelivery: buildPodPayload(podValues, podAttachments, podEventId),
      });
      setPodOpen(false);
    } catch {
      // POD_DOCUMENT_NOT_CLEAN and the rest surface in the global toast. The
      // dialog — and the clientEventId — survive so a retry stays idempotent.
    }
  };

  /* -------------------------------------------- transporter toll classes */

  const [tollClassOpen, setTollClassOpen] = useState(false);
  const [importOpen, setImportOpen] = useState(false);
  const [
    tollClassValues,
    tollClassChange,
    setTollClassValues,
    setTollClassErrors,
    ,
    tollClassErrors,
  ] = useForm<TollClassFormValues>({ target: 'transporterType' });
  const [savedTollClasses, setSavedTollClasses] = useState<TransporterTollClass[]>([]);

  const openTollClasses = () => {
    setTollClassValues({ target: 'transporterType' });
    setTollClassErrors({});
    setTollClassOpen(true);
  };

  const saveTollClass = async () => {
    const variables = buildTollClassVariables(tollClassValues);
    if (!variables) {
      setTollClassErrors({
        transporterTypeId:
          tollClassValues.target === 'transporterType' ? t('tolls.transporterClass.required') : undefined,
        transporterId:
          tollClassValues.target === 'transporter' ? t('tolls.transporterClass.required') : undefined,
        tollVehicleClassCode: tollClassValues.tollVehicleClassCode
          ? undefined
          : t('tolls.transporterClass.required'),
      });
      return;
    }
    try {
      const mapping = await setTransporterTollClass.mutateAsync(variables);
      setSavedTollClasses((previous) => [
        ...previous.filter(
          (candidate) => candidate.transporterTollClassId !== mapping.transporterTollClassId
        ),
        mapping,
      ]);
      setTollClassErrors({});
    } catch {
      // Surfaced by the global toast.
    }
  };

  /* --------------------------------------------------- lifecycle dialogs */

  const [confirmDelete, setConfirmDelete] = useState(false);
  const [reasonAction, setReasonAction] = useState<'cancel' | 'abort' | null>(null);
  const [reason, setReason] = useState('');
  const [completeOpen, setCompleteOpen] = useState(false);
  const [forceComplete, setForceComplete] = useState(false);
  // The manual verbs live behind one affordance now — see the Override group below.
  const [overrideOpen, setOverrideOpen] = useState(false);
  const [inTransitOpen, setInTransitOpen] = useState(false);
  const [inTransitStartedAt, setInTransitStartedAt] = useState('');
  const declareInTransit = useDeclareTripInTransit();
  const [skipStopId, setSkipStopId] = useState<string | null>(null);
  const [skipReason, setSkipReason] = useState('');
  const [shareOpen, setShareOpen] = useState(false);

  const runLifecycle = (action: 'start' | 'pause' | 'resume') => {
    if (!selectedTripId) return;
    lifecycle.mutate({ action, tripId: selectedTripId });
  };

  const handleStopAction = (action: 'arrive' | 'depart' | 'skip', tripStopId: string) => {
    if (!selectedTripId) return;
    if (action === 'skip') {
      setSkipStopId(tripStopId);
      setSkipReason('');
      return;
    }
    stopProgress.mutate({
      action,
      tripId: selectedTripId,
      tripStopId,
      occurredAt: new Date().toISOString(),
      clientEventId: newClientEventId(),
    });
  };

  /* ---------------------------------------------------------------- board */

  const statusOptions = useMemo(
    () => [
      { value: ALL, label: t('trips.allStatuses') },
      ...TRIP_STATUSES.map((value) => ({
        value,
        label: t(`trips.statuses.${value}` as 'trips.statuses.Created'),
      })),
    ],
    [t]
  );

  const transporterOptions = useMemo(
    () => [
      { value: ALL, label: t('trips.allTransporters') },
      ...transporters.map((transporter) => ({
        value: transporter.transporterId,
        label: transporter.name,
      })),
    ],
    [transporters, t]
  );

  const driverOptions = useMemo(
    () => [
      { value: ALL, label: t('trips.allDrivers') },
      ...drivers
        .filter((driver) => driver.active)
        .map((driver) => ({ value: driver.driverId, label: driver.name })),
    ],
    [drivers, t]
  );

  const exceptionOptions = useMemo(
    () => [
      { value: ALL, label: t('trips.exceptions.all') },
      ...TRIP_EXCEPTIONS.map((value) => ({
        value,
        label: t(`trips.exceptions.${value}` as 'trips.exceptions.overdue'),
      })),
    ],
    [t]
  );

  const statusColor = (value: string) =>
    value === 'InProgress'
      ? 'info'
      : value === 'Completed'
        ? 'success'
        : value === 'Paused'
          ? 'warning'
          : value === 'Created'
            ? 'primary'
            : 'secondary';

  /**
   * The phase, in the dispatcher's words: "Loading at Plant 3", "In transit → Client X
   * (ETA 11:05)", "Unloading at Client Y", "Overdue". The backend hands over the phase
   * plus the stop it concerns; the sentence is built here because it is a UI reading of
   * those facts, not a stored string (spec 11a §10).
   */
  const phaseLabel = (trip: Trip): string => {
    const phase = t(`trips.phases.${trip.phase}` as 'trips.phases.Scheduled');
    if (!trip.phaseStopName) return phase;
    const eta = trip.phaseEtaAt ? ` (${t('trips.eta')} ${formatDateTime(trip.phaseEtaAt)})` : '';
    return `${phase} · ${trip.phaseStopName}${eta}`;
  };

  const phaseColor = (phase: string) =>
    phase === 'Overdue'
      ? 'error'
      : phase === 'InTransit'
        ? 'info'
        : phase === 'AtOrigin' || phase === 'AtStop'
          ? 'warning'
          : phase === 'Completed'
            ? 'success'
            : 'secondary';

  /**
   * The exception filter runs here, over the wide window {@link EXCEPTION_SCAN_SIZE}
   * fetched for it, and then this component pages the survivors ten at a time.
   */
  const matchingTrips = useMemo(
    () => (filteringExceptions ? trips.filter((trip) => hasException(trip, exception as TripException)) : trips),
    [trips, exception, filteringExceptions]
  );

  const visibleTrips = useMemo(
    () => (filteringExceptions ? matchingTrips.slice(page * PAGE_SIZE, (page + 1) * PAGE_SIZE) : matchingTrips),
    [matchingTrips, page, filteringExceptions]
  );

  // The board spans the FULL width, so it can afford the columns a dispatcher
  // actually scans by — who is carrying it and who is driving — without the
  // crowding that made a one-third-width board collide with itself.
  //
  // Phase leads the status badge: a column of rows all reading "InProgress" is
  // precisely why a dispatcher used to have to open each one.
  const columns = [
    { name: 'code', title: t('trips.code'), align: 'left' as const },
    { name: 'customer', title: t('trips.customerName'), align: 'left' as const },
    { name: 'transporter', title: t('trips.transporter'), align: 'left' as const },
    { name: 'driver', title: t('trips.driver'), align: 'left' as const },
    { name: 'plannedStart', title: t('trips.plannedStart'), align: 'left' as const },
    { name: 'stops', title: t('trips.stops'), align: 'center' as const },
    { name: 'phase', title: t('trips.phase'), align: 'left' as const },
    { name: 'status', title: t('trips.status'), align: 'center' as const },
    { name: 'id' },
  ];

  const rows = visibleTrips.map((trip) => ({
    code: <Name name={trip.code} />,
    customer: <Description description={trip.customerName ?? '-'} />,
    transporter: (
      <Description
        description={
          transporters.find((transporter) => transporter.transporterId === trip.transporterId)?.name ??
          '-'
        }
      />
    ),
    driver: (
      <Description
        description={drivers.find((driver) => driver.driverId === trip.driverId)?.name ?? '-'}
      />
    ),
    plannedStart: <Description description={formatDateTime(trip.plannedStartAt)} />,
    stops: <Name name={trip.stopCount} />,
    phase: (
      <ArgonBadge
        variant="gradient"
        color={phaseColor(trip.phase)}
        size="xs"
        container
        badgeContent={phaseLabel(trip)}
      />
    ),
    status: (
      <ArgonBox display="flex" alignItems="center" justifyContent="center" gap={0.5}>
        <ArgonBadge
          variant="gradient"
          color={statusColor(trip.status)}
          size="xs"
          container
          badgeContent={t(`trips.statuses.${trip.status}` as 'trips.statuses.Created')}
        />
        {trip.deviationOpenedAt && (
          <ArgonBadge
            variant="gradient"
            color="error"
            size="xs"
            container
            badgeContent={t('trips.deviation')}
          />
        )}
      </ArgonBox>
    ),
    id: trip.tripId,
  }));

  // With an exception filter on, the counter has to describe what SURVIVED the filter,
  // not what the server returned — otherwise it reports "1–10 of 200" over four rows.
  const rowTotal = filteringExceptions ? matchingTrips.length : totalCount;

  // Deleting the last row of a page shrinks the total below the page start.
  useEffect(() => {
    if (page > 0 && page * PAGE_SIZE >= rowTotal) {
      setPage(Math.max(0, Math.ceil(rowTotal / PAGE_SIZE) - 1));
    }
  }, [rowTotal, page]);

  const pageStart = rowTotal === 0 ? 0 : page * PAGE_SIZE + 1;
  const pageEnd = Math.min((page + 1) * PAGE_SIZE, rowTotal);
  const hasNext = pageEnd < rowTotal;

  const editable = !!detail && EDITABLE_STATUSES.has(detail.trip.status);
  const running = detail?.trip.status === 'InProgress';
  // Deliveries and POD follow the backend rule ("not terminal"), not the
  // planning-only EDITABLE_STATUSES set: evidence is captured while running.
  const canWriteDeliveries = !!detail && !TERMINAL_STATUSES.has(detail.trip.status);

  return (
    <DashboardLayout>
      <DashboardNavbar
        searchVisibility
        searchQuery={search}
        handleSearch={(event) => {
          setPage(0);
          setSearch(event.target.value);
        }}
      />
      <ArgonBox py={2}>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12 }}>
            <ArgonBox display="flex" alignItems="center" gap={1} flexWrap="wrap">
              <CompactSelect
                name="status"
                value={status}
                options={statusOptions}
                label={t('trips.filterStatus')}
                onChange={(_, value) => {
                  setPage(0);
                  setStatus(String(value));
                }}
              />
              {/* Exception-driven attention (spec 11a §10): the system runs the
                  ordinary lifecycle, so what a dispatcher needs on screen is the
                  short list of trips that are NOT going to plan. */}
              <CompactSelect
                name="exception"
                value={exception}
                options={exceptionOptions}
                label={t('trips.exceptions.label')}
                onChange={(_, value) => {
                  setPage(0);
                  setException(String(value));
                }}
              />
              <CompactSelect
                name="transporter"
                value={transporterFilter}
                options={transporterOptions}
                label={t('trips.filterTransporter')}
                onChange={(_, value) => {
                  setPage(0);
                  setTransporterFilter(String(value));
                }}
              />
              <CompactSelect
                name="driver"
                value={driverFilter}
                options={driverOptions}
                label={t('trips.filterDriver')}
                onChange={(_, value) => {
                  setPage(0);
                  setDriverFilter(String(value));
                }}
              />
              <ArgonBox width="190px">
                <CustomTextField
                  margin="none"
                  name="from"
                  id="from"
                  label={t('trips.filterFrom')}
                  type="date"
                  value={from}
                  onChange={(event) => {
                    setPage(0);
                    setFrom(event.target.value);
                  }}
                />
              </ArgonBox>
              <ArgonBox width="190px">
                <CustomTextField
                  margin="none"
                  name="to"
                  id="to"
                  label={t('trips.filterTo')}
                  type="date"
                  value={to}
                  onChange={(event) => {
                    setPage(0);
                    setTo(event.target.value);
                  }}
                />
              </ArgonBox>
              <ArgonButton variant="gradient" color="info" size="small" onClick={() => openTrip(false)}>
                <Icon>add</Icon>&nbsp;{t('trips.newTrip')}
              </ArgonButton>
              {/* Bulk planning is the input side of zero-touch: a dispatcher who no
                  longer starts trips one by one must not have to create them one by
                  one either (spec 11a §9.1). */}
              <ArgonButton
                variant="outlined"
                color="info"
                size="small"
                onClick={() => setImportOpen(true)}
              >
                <Icon>upload_file</Icon>&nbsp;{t('trips.import.action')}
              </ArgonButton>
              {/* Account-scoped transporter → toll-class mapping. It sits here,
                  not in the SuperAdministrator toll catalog: it is tenant data
                  under Resources.Trips/Edit, and this is the only route already
                  gated by the trip-management feature key (spec 11 §4, §7.6). */}
              <ArgonButton
                variant="outlined"
                color="dark"
                size="small"
                onClick={openTollClasses}
              >
                <Icon>local_shipping</Icon>&nbsp;{t('tolls.transporterClass.action')}
              </ArgonButton>
            </ArgonBox>
          </Grid>

          {/* MASTER over DETAIL: the board takes the full width on top, the trip
              workspace the full width underneath.

              Side by side, neither half had room — six columns collided inside a
              one-third-width board, while the map, stop tables and POD gallery
              were squeezed into the remaining two thirds. Stacking them gives
              both the whole window, and the board is short enough (10 rows) that
              the workspace below still starts above the fold. */}
          <Grid size={{ xs: 12 }}>
            <ArgonTypography variant="button" fontWeight="medium">
              {t('trips.board')}
            </ArgonTypography>
            {rows.length === 0 ? (
              <ArgonTypography variant="caption" color="secondary" display="block">
                {t('trips.noTrips')}
              </ArgonTypography>
            ) : (
              <Table
                columns={columns}
                rows={rows}
                compact
                scrollable
                /* Kept even at full width: the default `tableLayout: fixed; width: 100%` divides
                   space evenly regardless of content, and `scrollable` alone sets
                   `overflowX: hidden`, which clips the container without stopping cells from
                   colliding. `horizontalScroll` sizes columns to their content, so a narrow
                   viewport scrolls sideways instead of overlapping. */
                horizontalScroll
                /* Fits the 10-row page without scrolling; a wrapped cell scrolls slightly rather
                   than pushing the workspace below off screen. */
                maxHeight="440px"
                selectedField="code"
                selected={trips.find((trip) => trip.tripId === selectedTripId)?.code ?? null}
                handleSelected={(code) =>
                  setSelectedTripId(trips.find((trip) => trip.code === code)?.tripId ?? null)
                }
              />
            )}
            <ArgonBox display="flex" justifyContent="space-between" alignItems="center" mt={1}>
              <ArgonTypography variant="caption" color="secondary">
                {t('trips.showing', { from: pageStart, to: pageEnd, total: totalCount })}
              </ArgonTypography>
              <ArgonPagination>
                <ArgonPagination item onClick={() => setPage((p) => Math.max(0, p - 1))} disabled={page === 0}>
                  <Icon>keyboard_arrow_left</Icon>
                </ArgonPagination>
                <ArgonPagination item onClick={() => hasNext && setPage((p) => p + 1)} disabled={!hasNext}>
                  <Icon>keyboard_arrow_right</Icon>
                </ArgonPagination>
              </ArgonPagination>
            </ArgonBox>

          </Grid>

          {/* The workspace gets the full width too — the planner map, the stop and
              delivery tables and the POD gallery are the widest content on the
              screen and were the things most starved by the old split. */}
          <Grid size={{ xs: 12 }}>
            {!detail ? (
              <ArgonTypography variant="caption" color="secondary">
                {t('trips.selectTripHint')}
              </ArgonTypography>
            ) : (
              <ArgonBox>
                <ArgonBox display="flex" alignItems="center" gap={1} flexWrap="wrap" mb={1}>
                  <ArgonTypography variant="h5" fontWeight="medium">
                    {detail.trip.code}
                  </ArgonTypography>
                  <ArgonBadge
                    variant="gradient"
                    color={statusColor(detail.trip.status)}
                    size="sm"
                    container
                    badgeContent={t(
                      `trips.statuses.${detail.trip.status}` as 'trips.statuses.Created'
                    )}
                  />
                  {/* Derived from the stops, not stored: the shape of a trip IS its route. */}
                  {detail.stops.length > 0 && (
                    <ArgonBadge
                      variant="gradient"
                      color="dark"
                      size="sm"
                      container
                      badgeContent={t(
                        `trips.type.${deriveTripType(
                          detail.trip.originLatitude,
                          detail.trip.originLongitude,
                          detail.stops
                        )}` as 'trips.type.single'
                      )}
                    />
                  )}
                  {/* The phase chip: what this trip is actually doing right now,
                      derived from the measured origin and stop timestamps. */}
                  <ArgonBadge
                    variant="gradient"
                    color={phaseColor(detail.trip.phase)}
                    size="sm"
                    container
                    badgeContent={phaseLabel(detail.trip)}
                  />
                  {detail.trip.deviationOpenedAt && (
                    <ArgonTypography variant="caption" color="error">
                      {t('trips.deviationSince', {
                        since: formatDateTime(detail.trip.deviationOpenedAt),
                      })}
                    </ArgonTypography>
                  )}
                  <ArgonBox flexGrow={1} />
                  <ArgonButton variant="outlined" color="dark" size="small" onClick={() => openTrip(true)}>
                    <Icon>edit</Icon>&nbsp;{t('generic.edit')}
                  </ArgonButton>
                  <ArgonButton variant="outlined" color="info" size="small" onClick={() => setShareOpen(true)}>
                    <Icon>share</Icon>&nbsp;{t('trips.actions.share')}
                  </ArgonButton>
                  {/* The manual lifecycle verbs are OVERRIDES now, and they are grouped
                      to say so (spec 11a §10). The system starts, advances and closes a
                      trip from the zones; these exist for the exceptions — a dead
                      tracker, a dispatcher correction — and putting them behind one
                      affordance is what stops the screen from inviting the manual flow
                      the module no longer runs on. Permissions are unchanged. */}
                  <ArgonButton
                    variant={overrideOpen ? 'gradient' : 'outlined'}
                    color="secondary"
                    size="small"
                    onClick={() => setOverrideOpen((open) => !open)}
                  >
                    <Icon>build</Icon>&nbsp;{t('trips.override.action')}
                  </ArgonButton>
                  {detail.trip.status === 'Created' && canDeleteTrips && (
                    <ArgonButton variant="text" color="error" size="small" onClick={() => setConfirmDelete(true)}>
                      <Icon>delete</Icon>&nbsp;{t('trips.actions.delete')}
                    </ArgonButton>
                  )}
                </ArgonBox>

                {overrideOpen && (
                  <ArgonBox
                    display="flex"
                    alignItems="center"
                    gap={1}
                    flexWrap="wrap"
                    mb={2}
                    p={1.5}
                    borderRadius="md"
                    sx={{ border: '1px dashed', borderColor: 'secondary.main' }}
                  >
                    <ArgonTypography variant="caption" color="secondary">
                      {t('trips.override.hint')}
                    </ArgonTypography>
                    {detail.trip.status === 'Created' && (
                      <>
                        <ArgonButton variant="outlined" color="success" size="small" onClick={() => runLifecycle('start')}>
                          <Icon>play_arrow</Icon>&nbsp;{t('trips.actions.start')}
                        </ArgonButton>
                        {/* The trip whose truck left before anyone wrote it down.
                            Backfill from Geofencing's record wins over the declared
                            time whenever there is one (spec 11a §5.4). */}
                        <ArgonButton
                          variant="outlined"
                          color="info"
                          size="small"
                          onClick={() => {
                            setInTransitStartedAt('');
                            setInTransitOpen(true);
                          }}
                        >
                          <Icon>history</Icon>&nbsp;{t('trips.inTransit.action')}
                        </ArgonButton>
                      </>
                    )}
                    {running && (
                      <ArgonButton variant="outlined" color="warning" size="small" onClick={() => runLifecycle('pause')}>
                        <Icon>pause</Icon>&nbsp;{t('trips.actions.pause')}
                      </ArgonButton>
                    )}
                    {detail.trip.status === 'Paused' && (
                      <ArgonButton variant="outlined" color="info" size="small" onClick={() => runLifecycle('resume')}>
                        <Icon>play_arrow</Icon>&nbsp;{t('trips.actions.resume')}
                      </ArgonButton>
                    )}
                    {/* Complete works from Paused too: a dispatcher who took control of
                        a finished trip should not have to hand it back to automation
                        just to close it (spec 11a §5.1). */}
                    {(running || detail.trip.status === 'Paused') && (
                      <ArgonButton
                        variant="outlined"
                        color="success"
                        size="small"
                        onClick={() => {
                          setForceComplete(false);
                          setCompleteOpen(true);
                        }}
                      >
                        <Icon>flag</Icon>&nbsp;{t('trips.actions.complete')}
                      </ArgonButton>
                    )}
                    {['Created', 'InProgress', 'Paused'].includes(detail.trip.status) && (
                      <>
                        <ArgonButton
                          variant="outlined"
                          color="secondary"
                          size="small"
                          onClick={() => {
                            setReason('');
                            setReasonAction('cancel');
                          }}
                        >
                          {t('trips.actions.cancel')}
                        </ArgonButton>
                        {running && (
                          <ArgonButton
                            variant="outlined"
                            color="error"
                            size="small"
                            onClick={() => {
                              setReason('');
                              setReasonAction('abort');
                            }}
                          >
                            {t('trips.actions.abort')}
                          </ArgonButton>
                        )}
                      </>
                    )}
                  </ArgonBox>
                )}

                <Tabs
                  value={workspaceTab}
                  onChange={(_, value: WorkspaceTab) => setWorkspaceTab(value)}
                  variant="scrollable"
                  scrollButtons="auto"
                  sx={{ mb: 2 }}
                >
                  {WORKSPACE_TABS.map((tab) => (
                    <Tab
                      key={tab}
                      value={tab}
                      label={t(`trips.workspace.${tab}` as 'trips.workspace.route')}
                    />
                  ))}
                </Tabs>

                {/* Every panel below is keyed by trip id so switching trips
                    REMOUNTS it. Each holds trip-specific `useState` seeded once,
                    at mount. The surrounding `{!detail ? … }` ternary looks like
                    it unmounts them between trips, but it does not: TanStack
                    serves a previously visited trip's detail from cache
                    synchronously, so `detail` never goes undefined on the way
                    back and the panel keeps the PREVIOUS trip's values while
                    displaying the new one. That is not cosmetic — Assign then
                    writes trip A's driver onto trip B, and Replan applies A's
                    corridor width. */}
                {workspaceTab === 'route' && (
                  <RoutePlanner
                    key={detail.trip.tripId}
                    stops={detail.stops}
                    routePlan={detail.routePlan}
                    pois={pois}
                    mapType={mapSettings.maps}
                    mapKey={mapSettings.mapsKey}
                    darkMode={darkMode}
                    placing={placing}
                    onMapClick={handleMapClick}
                    onAddStop={() => openStop()}
                    onEditStop={openStop}
                    onRemoveStop={(tripStopId) => removeStop.mutate(tripStopId)}
                    onReorder={(orderedStopIds) =>
                      reorderStops.mutate({ tripId: detail.trip.tripId, orderedStopIds })
                    }
                    onPlanRoute={(corridorMeters) =>
                      planRoute.mutate({
                        tripId: detail.trip.tripId,
                        corridorMeters: corridorMeters || DEFAULT_CORRIDOR_METERS,
                        tollVehicleClass: detail.trip.tollVehicleClass,
                      })
                    }
                    planning={planRoute.isPending}
                    editable={editable}
                  />
                )}

                {workspaceTab === 'assignment' && (
                  <AssignmentPanel
                    key={detail.trip.tripId}
                    detail={detail}
                    drivers={drivers}
                    transporters={transporters}
                    onAssign={(driverId, transporterId) =>
                      assignTrip.mutate({ tripId: detail.trip.tripId, driverId, transporterId })
                    }
                    assigning={assignTrip.isPending}
                    editable={detail.trip.status !== 'Completed'}
                  />
                )}

                {workspaceTab === 'tolls' && (
                  <TollPanel
                    key={detail.trip.tripId}
                    routePlan={detail.routePlan}
                    vehicleClasses={vehicleClasses}
                    tripVehicleClass={detail.trip.tollVehicleClass}
                  />
                )}

                {(workspaceTab === 'stops' ||
                  workspaceTab === 'pod' ||
                  workspaceTab === 'timeline' ||
                  workspaceTab === 'replay') && (
                  <TripDetail
                    section={workspaceTab}
                    detail={detail}
                    mapType={mapSettings.maps}
                    mapKey={mapSettings.mapsKey}
                    darkMode={darkMode}
                    onStopAction={handleStopAction}
                    canRecordProgress={!!running}
                    onRecordPod={openPod}
                    onAddDelivery={(tripStopId) => openDelivery(tripStopId)}
                    onEditDelivery={(delivery) => openDelivery(delivery.tripStopId, delivery)}
                    onDeliveryOutcome={openOutcome}
                    onDeleteDelivery={setDeleteDeliveryTarget}
                    canWriteDeliveries={canWriteDeliveries}
                  />
                )}
              </ArgonBox>
            )}
          </Grid>

        </Grid>
      </ArgonBox>
      <Footer />

      <TripDialog
        open={tripOpen}
        setOpen={setTripOpen}
        handleSubmit={saveTrip}
        values={tripValues}
        handleChange={tripChange}
        errors={tripErrors}
        transporters={transporters}
        drivers={drivers}
        vehicleClasses={vehicleClasses}
        pois={pois}
        geofences={geofences}
        destinations={tripDestinations}
        setDestinations={setTripDestinations}
        copySources={copySources}
        onCopyFrom={applyTripTemplate}
      />

      <StopDialog
        open={stopOpen}
        setOpen={setStopOpen}
        handleSubmit={saveStop}
        handleCancel={() => setPlacing(false)}
        values={stopValues}
        handleChange={stopChange}
        errors={stopErrors}
        pois={pois}
        geofences={geofences}
        onPlaceOnMap={startPlacing}
      />

      {detail && <ShareDialog open={shareOpen} setOpen={setShareOpen} detail={detail} />}

      <DeliveryDialog
        open={deliveryOpen}
        setOpen={setDeliveryOpen}
        handleSubmit={saveDelivery}
        values={deliveryValues}
        handleChange={deliveryChange}
        errors={deliveryErrors}
        stopLabel={stopLabel(deliveryValues.tripStopId)}
      />

      <DeliveryOutcomeDialog
        open={!!outcomeDelivery}
        setOpen={() => setOutcomeDelivery(null)}
        handleSubmit={saveOutcome}
        deliveryLabel={
          outcomeDelivery
            ? [outcomeDelivery.clientName, outcomeDelivery.reference].filter(Boolean).join(' · ')
            : ''
        }
        status={outcomeStatus}
        onStatusChange={setOutcomeStatus}
        observations={outcomeObservations}
        onObservationsChange={setOutcomeObservations}
      />

      <ConfirmDialog
        title={t('trips.deliveries.deleteTitle')}
        message={t('trips.deliveries.deleteMessage')}
        open={!!deleteDeliveryTarget}
        setOpen={() => setDeleteDeliveryTarget(null)}
        onConfirm={async () => {
          if (deleteDeliveryTarget) {
            await deleteDelivery
              .mutateAsync(deleteDeliveryTarget.deliveryId)
              .catch(() => undefined);
          }
          setDeleteDeliveryTarget(null);
        }}
      />

      <PodDialog
        open={podOpen}
        setOpen={setPodOpen}
        handleSubmit={savePod}
        values={podValues}
        handleChange={podChange}
        errors={podErrors}
        stopLabel={stopLabel(podValues.tripStopId)}
        deliveries={
          detail?.stops.find((stop) => stop.tripStopId === podValues.tripStopId)?.deliveries ?? []
        }
        attachments={podAttachments}
        onUploadFiles={uploadPodFiles}
        onRemoveAttachment={(documentId) =>
          setPodAttachments((previous) =>
            previous.filter((attachment) => attachment.documentId !== documentId)
          )
        }
        onRefreshAttachment={refreshPodAttachment}
        uploading={uploadDocument.isPending}
      />

      <TripImportDialog open={importOpen} setOpen={setImportOpen} />

      <TollClassDialog
        open={tollClassOpen}
        setOpen={setTollClassOpen}
        handleSubmit={saveTollClass}
        values={tollClassValues}
        handleChange={tollClassChange}
        errors={tollClassErrors}
        transporters={transporters}
        vehicleClasses={vehicleClasses}
        savedMappings={savedTollClasses}
        saving={setTransporterTollClass.isPending}
      />

      <ConfirmDialog
        title={t('trips.deleteTitle')}
        message={t('trips.deleteMessage')}
        open={confirmDelete}
        setOpen={setConfirmDelete}
        onConfirm={async () => {
          if (selectedTripId) {
            await deleteTrip.mutateAsync(selectedTripId).catch(() => undefined);
            setSelectedTripId(null);
          }
          setConfirmDelete(false);
        }}
      />

      <FormDialog
        title={reasonAction === 'abort' ? t('trips.abortTitle') : t('trips.cancelTitle')}
        open={!!reasonAction}
        setOpen={() => setReasonAction(null)}
        handleSave={async () => {
          if (!selectedTripId || !reasonAction || !reason.trim()) return;
          await lifecycle
            .mutateAsync({ action: reasonAction, tripId: selectedTripId, reason })
            .catch(() => undefined);
          setReasonAction(null);
        }}
      >
        <ArgonTypography variant="caption" color="secondary">
          {reasonAction === 'abort' ? t('trips.abortMessage') : t('trips.cancelMessage')}
        </ArgonTypography>
        <CustomTextField
          autoFocus
          margin="normal"
          name="reason"
          id="reason"
          label={t('trips.reasonTitle')}
          type="text"
          multiline
          rows={2}
          value={reason}
          onChange={(event) => setReason(event.target.value)}
          required
        />
      </FormDialog>

      <FormDialog
        title={t('trips.completeTitle')}
        open={completeOpen}
        setOpen={setCompleteOpen}
        handleSave={async () => {
          if (!selectedTripId) return;
          await lifecycle
            .mutateAsync({ action: 'complete', tripId: selectedTripId, force: forceComplete })
            .catch(() => undefined);
          setCompleteOpen(false);
        }}
      >
        <ArgonTypography variant="caption" color="secondary">
          {t('trips.completeMessage')}
        </ArgonTypography>
        <CustomCheckbox
          name="forceComplete"
          id="forceComplete"
          value={forceComplete}
          label={t('trips.forceComplete')}
          handleChange={(event) => setForceComplete(!!event.target.checked)}
        />
      </FormDialog>

      {/* "This trip is already under way" (spec 11a §5.4).

          The date-time is the FALLBACK, not the input: when Geofencing recorded the
          vehicle leaving the origin zone, those measurements win and the field is
          ignored. It is left optional here for exactly that reason — requiring it
          would make a dispatcher invent a time the system can measure better. */}
      <FormDialog
        title={t('trips.inTransit.title')}
        open={inTransitOpen}
        setOpen={setInTransitOpen}
        handleSave={async () => {
          if (!selectedTripId) return;
          await declareInTransit
            .mutateAsync({ tripId: selectedTripId, startedAt: toIso(inTransitStartedAt) })
            .catch(() => undefined);
          setInTransitOpen(false);
        }}
      >
        <ArgonTypography variant="caption" color="secondary">
          {t('trips.inTransit.message')}
        </ArgonTypography>
        <CustomTextField
          margin="dense"
          name="startedAt"
          id="startedAt"
          label={t('trips.inTransit.startedAt')}
          type="datetime-local"
          slotProps={{ inputLabel: { shrink: true } }}
          value={inTransitStartedAt}
          onChange={(event) => setInTransitStartedAt(event.target.value)}
        />
      </FormDialog>

      <FormDialog
        title={t('tripStops.skipTitle')}
        open={!!skipStopId}
        setOpen={() => setSkipStopId(null)}
        handleSave={async () => {
          if (!selectedTripId || !skipStopId || !skipReason.trim()) return;
          await stopProgress
            .mutateAsync({
              action: 'skip',
              tripId: selectedTripId,
              tripStopId: skipStopId,
              occurredAt: new Date().toISOString(),
              clientEventId: newClientEventId(),
              reason: skipReason,
            })
            .catch(() => undefined);
          setSkipStopId(null);
        }}
      >
        <ArgonTypography variant="caption" color="secondary">
          {t('tripStops.skipMessage')}
        </ArgonTypography>
        <CustomTextField
          autoFocus
          margin="normal"
          name="skipReason"
          id="skipReason"
          label={t('tripStops.skipReason')}
          type="text"
          value={skipReason}
          onChange={(event) => setSkipReason(event.target.value)}
          required
        />
      </FormDialog>
    </DashboardLayout>
  );
}

export default TripManager;
