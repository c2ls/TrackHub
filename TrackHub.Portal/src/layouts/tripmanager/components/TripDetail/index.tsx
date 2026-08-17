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

import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { usePermissions } from 'context/permissions';
import { PermissionResources, PermissionActions } from 'constants/permissions';
import Grid from '@mui/material/Grid';
import Icon from '@mui/material/Icon';
import ArgonBox from 'components/ArgonBox';
import ArgonBadge from 'components/ArgonBadge';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import Table from 'controls/Tables/Table';
import { Name, Description } from 'controls/Tables/components/tableComponents';
import RouteMap from 'controls/Maps/RouteMap';
import type { MapProvider } from 'controls/Maps/core/MapProviderContext';
import type { RoutePoint, RouteStop, TrailPoint } from 'controls/Maps/core/mapTypes';
import { useTripTimeline, useTripRouteReplay } from 'queries/trips';
import { downloadDocument } from 'api/manager/documents';
import { formatDateTime } from 'utils/dateUtils';
import type { TripDetail as TripDetailVm, TripDelivery } from 'api/tripManagement/trips';

interface TripDetailProps {
  detail: TripDetailVm;
  mapType?: MapProvider;
  mapKey?: string | null;
  darkMode?: boolean;
  onStopAction: (action: 'arrive' | 'depart' | 'skip', tripStopId: string) => void;
  /** Stop progress can only be recorded while the trip is actually running. */
  canRecordProgress: boolean;
  /** Opens the POD capture dialog for a stop. */
  onRecordPod: (tripStopId: string) => void;
  onAddDelivery: (tripStopId: string) => void;
  onEditDelivery: (delivery: TripDelivery) => void;
  onDeliveryOutcome: (delivery: TripDelivery) => void;
  onDeleteDelivery: (delivery: TripDelivery) => void;
  /**
   * Deliveries and POD stay writable until the trip reaches a terminal status —
   * the backend's own rule for both commands is `TripStatuses.IsTerminal`.
   */
  canWriteDeliveries: boolean;
  /**
   * Which panel to render. The trip workspace is tabbed, so exactly one of these
   * is on screen at a time.
   *
   * This matters for more than tidiness: rendering every section at once put two
   * large maps (the planner's 52vh and the replay's 45vh) plus a POD image
   * gallery in the DOM simultaneously, and the replay query fired whether or not
   * anyone had scrolled to it. Selecting a section keeps the heavy work to the
   * tab the dispatcher actually opened.
   */
  section: TripDetailSection;
}

export type TripDetailSection = 'stops' | 'timeline' | 'pod' | 'replay';

/**
 * Trip detail: stop table, delivery table, event timeline, proof-of-delivery
 * gallery, and the recorded-route replay.
 */
function TripDetail({
  detail,
  mapType,
  mapKey,
  darkMode = false,
  onStopAction,
  canRecordProgress,
  onRecordPod,
  onAddDelivery,
  onEditDelivery,
  onDeliveryOutcome,
  onDeleteDelivery,
  canWriteDeliveries,
  section,
}: TripDetailProps) {
  const { t } = useTranslation();
  // Deleting a delivery is Trips/Delete, which the default User role does not hold.
  const { can } = usePermissions();
  const canDeleteDeliveries = can(PermissionResources.Trips, PermissionActions.Delete);
  const [replayEnabled, setReplayEnabled] = useState(false);

  const timelineQuery = useTripTimeline(detail.trip.tripId);
  const replayQuery = useTripRouteReplay(detail.trip.tripId, null, { enabled: replayEnabled });

  const stops = useMemo(
    () => [...detail.stops].sort((a, b) => a.sequence - b.sequence),
    [detail.stops]
  );

  const stopColumns = [
    { name: 'sequence', title: t('tripStops.sequence'), align: 'center' as const, width: '48px' },
    { name: 'name', title: t('tripStops.name'), align: 'left' as const },
    // Next to the arrival/departure pair it explains: the gap between them is loading time at a
    // Load stop and unloading time at an Unload one (spec 11a §4.2).
    { name: 'activity', title: t('tripStops.activity.label'), align: 'left' as const },
    { name: 'window', title: t('tripStops.plannedFrom'), align: 'left' as const },
    { name: 'eta', title: t('tripStops.eta'), align: 'left' as const },
    { name: 'arrival', title: t('tripStops.actualArrival'), align: 'left' as const },
    { name: 'departure', title: t('tripStops.actualDeparture'), align: 'left' as const },
    { name: 'status', title: t('tripStops.status'), align: 'center' as const },
    { name: 'action', title: t('generic.action'), align: 'center' as const },
    { name: 'id' },
  ];

  const stopRows = stops.map((stop) => ({
    sequence: <Name name={stop.sequence} />,
    name: <Description description={stop.name} />,
    activity: (
      <Description
        description={t(`tripStops.activity.${stop.activity}` as 'tripStops.activity.Unload', {
          defaultValue: stop.activity,
        })}
      />
    ),
    window: (
      <Description
        description={
          stop.plannedArrivalFrom
            ? `${formatDateTime(stop.plannedArrivalFrom)}${
                stop.plannedArrivalTo ? ` – ${formatDateTime(stop.plannedArrivalTo)}` : ''
              }`
            : '-'
        }
      />
    ),
    // The ETA is only meaningful alongside its source: a planned-schedule
    // fallback must not be presented as a live estimate.
    eta: (
      <Description
        description={
          stop.etaAt
            ? `${formatDateTime(stop.etaAt)} · ${t(
                `tripStops.etaSources.${stop.etaSource}` as 'tripStops.etaSources.Planned'
              )}`
            : t('tripStops.etaSources.Unavailable')
        }
      />
    ),
    arrival: <Description description={stop.actualArrivalAt ? formatDateTime(stop.actualArrivalAt) : '-'} />,
    departure: (
      <Description description={stop.actualDepartureAt ? formatDateTime(stop.actualDepartureAt) : '-'} />
    ),
    status: (
      <ArgonBadge
        variant="gradient"
        color={
          stop.status === 'Departed'
            ? 'success'
            : stop.status === 'Arrived'
              ? 'info'
              : stop.status === 'Skipped'
                ? 'secondary'
                : 'warning'
        }
        size="xs"
        container
        badgeContent={t(`tripStops.statuses.${stop.status}` as 'tripStops.statuses.Pending')}
      />
    ),
    action: (
      <>
        {canRecordProgress && stop.status === 'Pending' && (
          <>
            <ArgonButton
              variant="text"
              color="info"
              onClick={() => onStopAction('arrive', stop.tripStopId)}
            >
              <Icon>login</Icon>&nbsp;{t('tripStops.actions.arrive')}
            </ArgonButton>
            <ArgonButton
              variant="text"
              color="secondary"
              onClick={() => onStopAction('skip', stop.tripStopId)}
            >
              <Icon>skip_next</Icon>&nbsp;{t('tripStops.actions.skip')}
            </ArgonButton>
          </>
        )}
        {canRecordProgress && stop.status === 'Arrived' && (
          <ArgonButton
            variant="text"
            color="success"
            onClick={() => onStopAction('depart', stop.tripStopId)}
          >
            <Icon>logout</Icon>&nbsp;{t('tripStops.actions.depart')}
          </ArgonButton>
        )}
        {/* Spec 11 §9: until the driver app exists, POD and delivery capture
            are the dispatcher's job — so both live on the stop row. */}
        {canWriteDeliveries && (
          <>
            <ArgonButton
              variant="text"
              color="dark"
              onClick={() => onAddDelivery(stop.tripStopId)}
            >
              <Icon>add_shopping_cart</Icon>&nbsp;{t('trips.deliveries.add')}
            </ArgonButton>
            <ArgonButton
              variant="text"
              color="primary"
              onClick={() => onRecordPod(stop.tripStopId)}
            >
              <Icon>assignment_turned_in</Icon>&nbsp;{t('pod.record')}
            </ArgonButton>
          </>
        )}
      </>
    ),
    id: stop.tripStopId,
  }));

  const deliveries = stops.flatMap((stop) =>
    stop.deliveries.map((delivery) => ({ stop, delivery }))
  );

  const deliveryColumns = [
    { name: 'stop', title: t('trips.deliveries.stop'), align: 'left' as const },
    { name: 'reference', title: t('trips.deliveries.reference'), align: 'left' as const },
    { name: 'client', title: t('trips.deliveries.client'), align: 'left' as const },
    { name: 'products', title: t('trips.deliveries.products'), align: 'left' as const },
    { name: 'status', title: t('trips.deliveries.status'), align: 'center' as const },
    { name: 'action', title: t('generic.action'), align: 'center' as const },
    { name: 'id' },
  ];

  const deliveryStatusColor = (status: string) =>
    status === 'Delivered'
      ? 'success'
      : status === 'PartiallyDelivered'
        ? 'warning'
        : status === 'Rejected'
          ? 'error'
          : 'secondary';

  const deliveryRows = deliveries.map(({ stop, delivery }) => ({
    stop: <Name name={`${stop.sequence}. ${stop.name}`} />,
    reference: <Description description={delivery.reference ?? '-'} />,
    client: (
      <Description
        description={[delivery.clientName, delivery.branchName].filter(Boolean).join(' · ')}
      />
    ),
    products: <Description description={delivery.productsSummary ?? '-'} />,
    status: (
      <ArgonBadge
        variant="gradient"
        color={deliveryStatusColor(delivery.status)}
        size="xs"
        container
        badgeContent={t(
          `trips.deliveries.statuses.${delivery.status}` as 'trips.deliveries.statuses.Pending'
        )}
      />
    ),
    action: canWriteDeliveries ? (
      <>
        <ArgonButton variant="text" color="dark" onClick={() => onEditDelivery(delivery)}>
          <Icon>edit</Icon>&nbsp;{t('generic.edit')}
        </ArgonButton>
        <ArgonButton variant="text" color="success" onClick={() => onDeliveryOutcome(delivery)}>
          <Icon>fact_check</Icon>&nbsp;{t('trips.deliveries.outcome')}
        </ArgonButton>
        {canDeleteDeliveries && (
          <ArgonButton variant="text" color="error" onClick={() => onDeleteDelivery(delivery)}>
            <Icon>delete</Icon>&nbsp;{t('generic.delete')}
          </ArgonButton>
        )}
      </>
    ) : null,
    id: delivery.deliveryId,
  }));

  const events = timelineQuery.data?.items ?? [];
  const eventColumns = [
    { name: 'occurredAt', title: t('trips.timeline.occurredAt'), align: 'left' as const },
    { name: 'event', title: t('trips.timeline.event'), align: 'left' as const },
    { name: 'source', title: t('trips.timeline.source'), align: 'left' as const },
    { name: 'id' },
  ];
  const eventRows = events.map((event) => ({
    occurredAt: <Name name={formatDateTime(event.occurredAt)} />,
    // Falls back to the raw literal rather than rendering a missing-key placeholder: the
    // timeline is a record, and a type this build has no label for must still be readable.
    event: (
      <Description
        description={t(`trips.timeline.events.${event.eventType}` as 'trips.timeline.events.TripStarted', {
          defaultValue: event.eventType,
        })}
      />
    ),
    // The source is what tells a MEASURED start from a dispatcher's override (spec 11a §5.3),
    // so it is worth a word rather than an enum name.
    source: (
      <Description
        description={t(`trips.timeline.sources.${event.source}` as 'trips.timeline.sources.Detection', {
          defaultValue: event.source,
        })}
      />
    ),
    id: event.tripEventId,
  }));

  const replay = replayQuery.data;
  const trail = useMemo<TrailPoint[]>(
    () => (replay?.points ?? []).map((point) => ({ lat: point.latitude, lng: point.longitude })),
    [replay]
  );
  const plannedLine = useMemo<RoutePoint[]>(
    () =>
      (detail.routePlan?.geometry?.coordinates ?? []).map((point) => ({
        lat: point.latitude,
        lng: point.longitude,
      })),
    [detail.routePlan]
  );
  const replayStops = useMemo<RouteStop[]>(
    () =>
      stops.map((stop) => ({
        id: stop.tripStopId,
        sequence: stop.sequence,
        name: stop.name,
        lat: stop.latitude,
        lng: stop.longitude,
        status: stop.status,
      })),
    [stops]
  );

  return (
    <Grid container spacing={2}>
      {/* Stops and deliveries share a tab: a delivery belongs to a stop, and a
          dispatcher reconciling one is always looking at the other. */}
      {section === 'stops' && (
        <Grid size={{ xs: 12 }}>
          <ArgonTypography variant="button" fontWeight="medium">
            {t('tripStops.title')}
          </ArgonTypography>
          {stopRows.length === 0 ? (
            <ArgonTypography variant="caption" color="secondary" display="block">
              {t('tripStops.empty')}
            </ArgonTypography>
          ) : (
            <Table columns={stopColumns} rows={stopRows} compact selectedField="name" horizontalScroll />
          )}

          <ArgonBox mt={2}>
            <ArgonTypography variant="button" fontWeight="medium">
              {t('trips.deliveries.title')}
            </ArgonTypography>
            {deliveryRows.length === 0 ? (
              <ArgonTypography variant="caption" color="secondary" display="block">
                {t('trips.deliveries.empty')}
              </ArgonTypography>
            ) : (
              <Table
                columns={deliveryColumns}
                rows={deliveryRows}
                compact
                selectedField="stop"
                horizontalScroll
              />
            )}
          </ArgonBox>
        </Grid>
      )}

      {section === 'timeline' && (
        <Grid size={{ xs: 12 }}>
          <ArgonTypography variant="button" fontWeight="medium">
            {t('trips.timeline.title')}
          </ArgonTypography>
          {eventRows.length === 0 ? (
            <ArgonTypography variant="caption" color="secondary" display="block">
              {t('trips.timeline.empty')}
            </ArgonTypography>
          ) : (
            <Table
              columns={eventColumns}
              rows={eventRows}
              compact
              scrollable
              maxHeight="52vh"
              selectedField="occurredAt"
            />
          )}
        </Grid>
      )}

      {section === 'pod' && (
      <Grid size={{ xs: 12 }}>
        <ArgonTypography variant="button" fontWeight="medium">
          {t('pod.title')}
        </ArgonTypography>
        {detail.proofsOfDelivery.length === 0 ? (
          <ArgonTypography variant="caption" color="secondary" display="block">
            {t('pod.empty')}
          </ArgonTypography>
        ) : (
          <Grid container spacing={1} sx={{ mt: 0.5 }}>
            {detail.proofsOfDelivery.map((pod) => {
              const stop = stops.find((candidate) => candidate.tripStopId === pod.tripStopId);
              return (
                <Grid size={{ xs: 12, sm: 6, md: 4 }} key={pod.proofOfDeliveryId}>
                  <ArgonBox
                    p={1.5}
                    sx={{ border: '1px solid', borderColor: 'grey.300', borderRadius: 1, height: '100%' }}
                  >
                    <ArgonTypography variant="button" fontWeight="medium">
                      {stop ? `${stop.sequence}. ${stop.name}` : t('pod.stop')}
                    </ArgonTypography>
                    <ArgonTypography variant="caption" color="text" display="block">
                      {t('pod.receiver')}: {pod.receiverName}
                      {pod.receiverDocument ? ` (${pod.receiverDocument})` : ''}
                    </ArgonTypography>
                    <ArgonTypography variant="caption" color="secondary" display="block">
                      {t('pod.capturedAt')}: {formatDateTime(pod.capturedAt)}
                    </ArgonTypography>
                    {pod.latitude != null && pod.longitude != null && (
                      <ArgonTypography variant="caption" color="secondary" display="block">
                        {t('pod.location')}: {pod.latitude.toFixed(5)}, {pod.longitude.toFixed(5)}
                      </ArgonTypography>
                    )}
                    {pod.notes && (
                      <ArgonTypography variant="caption" color="secondary" display="block">
                        {t('pod.notes')}: {pod.notes}
                      </ArgonTypography>
                    )}
                    <ArgonBox mt={1} display="flex" flexWrap="wrap" gap={0.5}>
                      {pod.documents.map((document) => (
                        <ArgonButton
                          key={document.tripDocumentId}
                          variant="outlined"
                          color="secondary"
                          size="small"
                          onClick={() =>
                            downloadDocument(document.documentId, document.kind).catch(() => undefined)
                          }
                        >
                          <Icon>download</Icon>&nbsp;
                          {t(`pod.kinds.${document.kind}` as 'pod.kinds.Photo')}
                        </ArgonButton>
                      ))}
                    </ArgonBox>
                  </ArgonBox>
                </Grid>
              );
            })}
          </Grid>
        )}
      </Grid>
      )}

      {section === 'replay' && (
      <Grid size={{ xs: 12 }}>
        <ArgonBox display="flex" alignItems="center" justifyContent="space-between" gap={1} flexWrap="wrap">
          <ArgonTypography variant="button" fontWeight="medium">
            {t('trips.replay.title')}
          </ArgonTypography>
          {!replayEnabled && (
            <ArgonButton
              variant="outlined"
              color="info"
              size="small"
              onClick={() => setReplayEnabled(true)}
            >
              <Icon>play_circle</Icon>&nbsp;{t('trips.replay.load')}
            </ArgonButton>
          )}
        </ArgonBox>
        {replayEnabled && replay && (
          <>
            {/* Telemetry caps a replay at 10 000 points. A truncated track is
                surfaced loudly — never silently shortened (acceptance 22). */}
            {replay.truncated && (
              <ArgonTypography variant="caption" color="warning" fontWeight="medium" display="block">
                {t('trips.replay.truncated', { count: replay.points.length })}
              </ArgonTypography>
            )}
            {replay.points.length === 0 ? (
              <ArgonTypography variant="caption" color="secondary" display="block">
                {t('trips.replay.empty')}
              </ArgonTypography>
            ) : (
              <>
                <ArgonTypography variant="caption" color="secondary" display="block">
                  {t('trips.replay.points', { count: replay.points.length })}
                </ArgonTypography>
                <ArgonBox mt={1}>
                  <RouteMap
                    mapType={mapType}
                    mapKey={mapKey}
                    darkMode={darkMode}
                    height="45vh"
                    route={plannedLine}
                    stops={replayStops}
                    trail={trail}
                  />
                </ArgonBox>
              </>
            )}
          </>
        )}
      </Grid>
      )}
    </Grid>
  );
}

export default TripDetail;
