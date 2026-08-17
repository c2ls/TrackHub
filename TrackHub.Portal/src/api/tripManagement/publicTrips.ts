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
 * Anonymous customer trip tracking (TripManagement backend, REST).
 *
 * SANCTIONED ANONYMOUS FETCH, the `getVisibleAnnouncements` precedent:
 * `restClient.restRequest` always acquires an access token, which is exactly
 * wrong here — the recipient of a shared link is not a platform principal and
 * has nothing to sign in with. Failures are still normalized to `ApiError` so
 * the page can distinguish 404 (missing/revoked/wrong scope/feature disabled)
 * from 410 (expired) and render the right localized state.
 *
 * The response is the server's already-projected snapshot: whatever the trip's
 * share flags withheld simply is not in the payload. The page renders what it
 * receives and never re-filters or substitutes placeholders (acceptance 23).
 */

import axios, { AxiosError } from 'axios';
import { ApiError } from 'api/core/errors';
import { REST_ENDPOINTS } from 'api/core/endpoints';

/** A stop as an end customer sees it — no ids, no POD content, existence flag only. */
export interface PublicTripStop {
  sequence: number;
  name: string;
  city?: string | null;
  status: string;
  plannedArrivalFrom?: string | null;
  plannedArrivalTo?: string | null;
  actualArrivalAt?: string | null;
  etaAt?: string | null;
  hasProofOfDelivery: boolean;
}

/**
 * The anonymous tracking snapshot (`PublicTripVm`). Every optional field is
 * genuinely absent when the share did not grant it — the UI must test for
 * presence rather than assume a value, and must never show a placeholder in
 * place of a field the server deliberately withheld. Toll and cost figures are
 * never present at all, by design.
 */
export interface PublicTrip {
  tripId: string;
  code: string;
  status: string;
  plannedStartAt: string;
  plannedEndAt?: string | null;
  actualStartAt?: string | null;
  actualEndAt?: string | null;
  /**
   * There is deliberately NO `customerName` here. `PublicTripVm` does not expose
   * one: on a multi-drop trip the consignee is not the same party as every link
   * holder, so disclosing it would tell one recipient who another recipient is
   * (spec 11 §7.8). Do not re-add the field to mirror the internal `TripVm`.
   */
  stops: PublicTripStop[];
  /** Present only when the share includes the vehicle. */
  vehicleLabel?: string | null;
  /** Given name only — never a full name, never contact data. */
  driverGivenName?: string | null;
  /** Live position: present only when shared AND the trip is in progress. */
  lastLatitude?: number | null;
  lastLongitude?: number | null;
  lastPositionAt?: string | null;
  /**
   * Planned route geometry — present ONLY when the share ticked `includeRoute`,
   * which defaults to false. It used to be disclosed unconditionally; treat its
   * absence as the normal case, never as a missing field to work around.
   */
  plannedRoute?: { coordinates: Array<{ latitude: number; longitude: number }> } | null;
}

/** The four query values a tracking URL must carry. */
export interface PublicTripLinkParams {
  publicLinkGrantId: string;
  accountId: string;
  resourceId: string;
  token: string;
}

/** Distinguishable outcomes the tracking page renders as localized states. */
export type PublicTripErrorCode = 'NOT_FOUND' | 'EXPIRED' | 'INVALID_LINK' | 'UNAVAILABLE';

/** Reads the four link parameters out of a URL query string. Returns null when any is missing. */
export function parsePublicTripLink(search: string): PublicTripLinkParams | null {
  const params = new URLSearchParams(search);
  const publicLinkGrantId = params.get('grant');
  const accountId = params.get('account');
  const resourceId = params.get('trip');
  const token = params.get('token');
  if (!publicLinkGrantId || !accountId || !resourceId || !token) return null;
  return { publicLinkGrantId, accountId, resourceId, token };
}

/** Builds the shareable customer URL for a created trip share. */
export function publicTripUrl(origin: string, params: PublicTripLinkParams): string {
  const query = new URLSearchParams({
    grant: params.publicLinkGrantId,
    account: params.accountId,
    trip: params.resourceId,
    token: params.token,
  });
  return `${origin.replace(/\/+$/, '')}/track?${query.toString()}`;
}

/**
 * Fetches the tracking snapshot WITHOUT authentication. Throws an `ApiError`
 * whose `code` is one of {@link PublicTripErrorCode} so the page can render a
 * clean localized 404/410 rather than a transport message.
 */
export async function getPublicTrip(params: PublicTripLinkParams): Promise<PublicTrip> {
  const url = `${REST_ENDPOINTS.tripManagementPublicTrips}/${encodeURIComponent(params.publicLinkGrantId)}`;
  try {
    const response = await axios.get<PublicTrip>(url, {
      params: {
        accountId: params.accountId,
        resourceId: params.resourceId,
        token: params.token,
      },
      timeout: 10000,
      withCredentials: false,
    });
    return response.data;
  } catch (error) {
    const status = (error as AxiosError).response?.status;
    const code: PublicTripErrorCode =
      status === 410
        ? 'EXPIRED'
        : status === 404
          ? 'NOT_FOUND'
          : status === 400
            ? 'INVALID_LINK'
            : 'UNAVAILABLE';
    throw new ApiError(`Public trip lookup failed (${status ?? 'no response'})`, {
      code,
      i18nKey: PUBLIC_TRIP_I18N[code],
      status,
      cause: error,
    });
  }
}

/** Localized headline per outcome; the page pairs each with a longer explanation. */
const PUBLIC_TRIP_I18N: Record<PublicTripErrorCode, string> = {
  NOT_FOUND: 'tripTracking.notFoundTitle',
  EXPIRED: 'tripTracking.expiredTitle',
  INVALID_LINK: 'tripTracking.invalidLinkTitle',
  UNAVAILABLE: 'tripTracking.unavailableTitle',
};
