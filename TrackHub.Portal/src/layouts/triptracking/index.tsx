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

import { useMemo } from 'react';
import { useLocation } from 'react-router';
import { useTranslation } from 'react-i18next';
import Grid from '@mui/material/Grid';
import Icon from '@mui/material/Icon';
import ArgonBox from 'components/ArgonBox';
import ArgonBadge from 'components/ArgonBadge';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import RouteMap from 'controls/Maps/RouteMap';
import type { RoutePoint } from 'controls/Maps/core/mapTypes';
import { usePublicTrip } from 'queries/trips';
import { parsePublicTripLink } from 'api/tripManagement/publicTrips';
import type { PublicTripErrorCode } from 'api/tripManagement/publicTrips';
import { ApiError } from 'api/core/errors';
import { formatDateTime } from 'utils/dateUtils';

/** A single labelled fact. Rendered ONLY when the server actually sent the value. */
const Fact = ({ label, value }: { label: string; value: string }) => (
  <ArgonBox mr={3} mb={1}>
    <ArgonTypography variant="caption" color="secondary" display="block">
      {label}
    </ArgonTypography>
    <ArgonTypography variant="button" fontWeight="medium">
      {value}
    </ArgonTypography>
  </ArgonBox>
);

/** Full-page localized outcome for a link that cannot be resolved. */
const LinkState = ({ code }: { code: PublicTripErrorCode }) => {
  const { t } = useTranslation();
  const titles: Record<PublicTripErrorCode, string> = {
    NOT_FOUND: t('tripTracking.notFoundTitle'),
    EXPIRED: t('tripTracking.expiredTitle'),
    INVALID_LINK: t('tripTracking.invalidLinkTitle'),
    UNAVAILABLE: t('tripTracking.unavailableTitle'),
  };
  const bodies: Record<PublicTripErrorCode, string> = {
    NOT_FOUND: t('tripTracking.notFoundBody'),
    EXPIRED: t('tripTracking.expiredBody'),
    INVALID_LINK: t('tripTracking.invalidLinkBody'),
    UNAVAILABLE: t('tripTracking.unavailableBody'),
  };
  return (
    <ArgonBox
      display="flex"
      flexDirection="column"
      alignItems="center"
      justifyContent="center"
      minHeight="70vh"
      px={3}
      textAlign="center"
    >
      <Icon fontSize="large" color="disabled">
        {code === 'EXPIRED' ? 'schedule' : 'link_off'}
      </Icon>
      <ArgonTypography variant="h4" fontWeight="medium" mt={2}>
        {titles[code]}
      </ArgonTypography>
      <ArgonTypography variant="body2" color="secondary" mt={1} maxWidth="520px">
        {bodies[code]}
      </ArgonTypography>
    </ArgonBox>
  );
};

/**
 * Anonymous customer trip tracking.
 *
 * DISCLOSURE RULE: this page renders exactly what the server sent and nothing
 * else. The snapshot is already projected from the trip's share flags, so a
 * field the customer was not granted is simply ABSENT from the payload — every
 * optional value is therefore tested for presence and omitted when missing.
 * There is no client-side filtering standing in for the server's projection,
 * and no placeholder is shown for a withheld field: a blank "Driver: —" row
 * would itself leak that a driver exists. Toll and cost figures never appear at
 * all; the backend does not send them.
 */
function TripTracking() {
  const { t } = useTranslation();
  const { search } = useLocation();

  const params = useMemo(() => parsePublicTripLink(search), [search]);
  const tripQuery = usePublicTrip(params);

  if (!params) return <LinkState code="INVALID_LINK" />;

  if (tripQuery.isPending) {
    return (
      <ArgonBox display="flex" alignItems="center" justifyContent="center" minHeight="70vh">
        <ArgonTypography variant="body2" color="secondary">
          {t('tripTracking.loading')}
        </ArgonTypography>
      </ArgonBox>
    );
  }

  if (tripQuery.isError) {
    const code =
      tripQuery.error instanceof ApiError
        ? ((tripQuery.error.code as PublicTripErrorCode | undefined) ?? 'UNAVAILABLE')
        : 'UNAVAILABLE';
    return <LinkState code={code} />;
  }

  const trip = tripQuery.data;
  const stops = [...(trip.stops ?? [])].sort((a, b) => a.sequence - b.sequence);

  // Live position: present only when the share granted it AND the trip is
  // running. The server enforces both; the page simply renders what arrived.
  const hasLivePosition = trip.lastLatitude != null && trip.lastLongitude != null;

  const plannedRoute: RoutePoint[] = (trip.plannedRoute?.coordinates ?? []).map((point) => ({
    lat: point.latitude,
    lng: point.longitude,
  }));

  // NO STOP PINS ON THIS MAP — deliberately, do not add them back.
  //
  // The snapshot carries no stop coordinates, and it must not: §7.8 lists what
  // an anonymous link may disclose about a stop (sequence, name, city, status,
  // window, arrival, ETA) and coordinates are not on it. Adding them would be a
  // disclosure change needing a spec amendment, not a UI tweak.
  //
  // This page previously drew numbered pins at evenly spaced indices along the
  // planned-route polyline. Those positions were arithmetic, not data: a stop
  // pin sat wherever its ordinal fell on the line, which is nowhere near the
  // real address unless the stops happen to be equidistant. A customer reads a
  // numbered pin as "this is where my delivery is", so a fabricated pin is
  // worse than no pin — it is a confident wrong answer about someone's parcel.
  //
  // The stop timeline beside the map already conveys the sequence, the ETA and
  // the status honestly, using only values the server actually sent.
  const showMap = plannedRoute.length > 1 || hasLivePosition;

  return (
    <ArgonBox px={{ xs: 2, md: 4 }} py={4} maxWidth="1100px" mx="auto">
      <ArgonBox display="flex" alignItems="center" gap={1} flexWrap="wrap">
        <ArgonTypography variant="h4" fontWeight="medium">
          {t('tripTracking.trip')} {trip.code}
        </ArgonTypography>
        <ArgonBadge
          variant="gradient"
          color={
            trip.status === 'InProgress'
              ? 'info'
              : trip.status === 'Completed'
                ? 'success'
                : 'secondary'
          }
          size="sm"
          container
          // A customer reads "Created" as a system noise word; what they are being told is
          // that their delivery is SCHEDULED (spec 11a §10). The status itself is unchanged
          // — only the word this page shows for it.
          badgeContent={
            trip.status === 'Created'
              ? t('tripTracking.scheduled')
              : t(`trips.statuses.${trip.status}` as 'trips.statuses.Created')
          }
        />
        <ArgonBox flexGrow={1} />
        <ArgonButton
          variant="outlined"
          color="secondary"
          size="small"
          onClick={() => tripQuery.refetch()}
        >
          <Icon>refresh</Icon>&nbsp;{t('tripTracking.refresh')}
        </ArgonButton>
      </ArgonBox>

      <ArgonBox display="flex" flexWrap="wrap" mt={2}>
        {/* No customer name: `PublicTripVm` deliberately does not carry one. On a
            multi-drop trip the consignee is not the same party as every link
            holder, so naming a customer would tell one recipient who another
            recipient is (spec 11 §7.8). */}
        <Fact label={t('tripTracking.plannedStart')} value={formatDateTime(trip.plannedStartAt)} />
        {trip.plannedEndAt && (
          <Fact label={t('tripTracking.plannedEnd')} value={formatDateTime(trip.plannedEndAt)} />
        )}
        {trip.actualStartAt && (
          <Fact label={t('tripTracking.actualStart')} value={formatDateTime(trip.actualStartAt)} />
        )}
        {trip.actualEndAt && (
          <Fact label={t('tripTracking.actualEnd')} value={formatDateTime(trip.actualEndAt)} />
        )}
        {/* Shared only when the sender ticked the box — absent otherwise. */}
        {trip.vehicleLabel && <Fact label={t('tripTracking.vehicle')} value={trip.vehicleLabel} />}
        {trip.driverGivenName && (
          <Fact label={t('tripTracking.driver')} value={trip.driverGivenName} />
        )}
      </ArgonBox>

      <Grid container spacing={3} sx={{ mt: 0 }}>
        {showMap && (
          <Grid size={{ xs: 12, md: 7 }}>
            <RouteMap
              height="55vh"
              route={plannedRoute}
              playbackPosition={
                hasLivePosition
                  ? { lat: trip.lastLatitude as number, lng: trip.lastLongitude as number }
                  : null
              }
            />
            {hasLivePosition && trip.lastPositionAt && (
              <ArgonTypography variant="caption" color="secondary" display="block" mt={0.5}>
                {t('tripTracking.livePosition')} ·{' '}
                {t('tripTracking.lastUpdate', { when: formatDateTime(trip.lastPositionAt) })}
              </ArgonTypography>
            )}
          </Grid>
        )}

        <Grid size={{ xs: 12, md: showMap ? 5 : 12 }}>
          <ArgonTypography variant="h6" fontWeight="medium" mb={1}>
            {t('tripTracking.stops')}
          </ArgonTypography>
          {stops.map((stop) => (
            <ArgonBox
              key={stop.sequence}
              display="flex"
              gap={1.5}
              py={1}
              sx={{ borderBottom: '1px solid', borderColor: 'grey.300' }}
            >
              <ArgonTypography variant="h6" fontWeight="bold" color="secondary" sx={{ minWidth: 26 }}>
                {stop.sequence}
              </ArgonTypography>
              <ArgonBox flexGrow={1} minWidth={0}>
                <ArgonTypography variant="button" fontWeight="medium">
                  {stop.name}
                </ArgonTypography>
                {stop.city && (
                  <ArgonTypography variant="caption" color="secondary" display="block">
                    {stop.city}
                  </ArgonTypography>
                )}
                {(stop.plannedArrivalFrom || stop.plannedArrivalTo) && (
                  <ArgonTypography variant="caption" color="secondary" display="block">
                    {t('tripTracking.plannedWindow')}:{' '}
                    {stop.plannedArrivalFrom ? formatDateTime(stop.plannedArrivalFrom) : ''}
                    {stop.plannedArrivalTo ? ` – ${formatDateTime(stop.plannedArrivalTo)}` : ''}
                  </ArgonTypography>
                )}
                {stop.actualArrivalAt ? (
                  <ArgonTypography variant="caption" color="success" display="block">
                    {t('tripTracking.arrivedAt')}: {formatDateTime(stop.actualArrivalAt)}
                  </ArgonTypography>
                ) : (
                  stop.etaAt && (
                    <ArgonTypography variant="caption" color="info" display="block">
                      {t('tripTracking.eta')}: {formatDateTime(stop.etaAt)}
                    </ArgonTypography>
                  )
                )}
                {stop.hasProofOfDelivery && (
                  <ArgonTypography variant="caption" color="success" display="block">
                    <Icon fontSize="inherit">check_circle</Icon> {t('tripTracking.podRecorded')}
                  </ArgonTypography>
                )}
              </ArgonBox>
              <ArgonBadge
                variant="gradient"
                color={stop.status === 'Departed' ? 'success' : stop.status === 'Arrived' ? 'info' : 'secondary'}
                size="xs"
                container
                badgeContent={t(`tripStops.statuses.${stop.status}` as 'tripStops.statuses.Pending')}
              />
            </ArgonBox>
          ))}
        </Grid>
      </Grid>
    </ArgonBox>
  );
}

export default TripTracking;
