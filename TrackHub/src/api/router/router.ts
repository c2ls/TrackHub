/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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
 * Router API (Router backend): plain typed async functions. Failures THROW
 * ApiError — fallbacks and toasts belong to the caller layer. `pingOperator`
 * and `reverseGeocode` are the silent operations: their callers swallow the
 * error (no toast) rather than routing it through the query-cache handler.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import { formatDateTimeOffSet } from 'utils/dataUtils';
import type {
  PositionFieldsFragment as PositionFieldsType,
  TripFieldsFragment as TripFieldsType,
  AddressFieldsFragment as AddressFieldsType,
  PositionSourceType,
  GetProviderDevicesByOperatorQuery,
} from './generated/graphql';
import {
  PingOperatorDocument,
  GetProviderDevicesByOperatorDocument,
  GetDevicePositionsByUserDocument,
  GetTripsByTransporterDocument,
  GetPositionsByTransporterDocument,
  ReverseGeocodeDocument,
} from './routerOperations';

export type Position = PositionFieldsType;
export type Trip = TripFieldsType;
export type Address = AddressFieldsType;
export type ProviderDevice = GetProviderDevicesByOperatorQuery['providerDevicesByOperator'][number];
export type { PositionSourceType };

/** Silent op: tests connectivity with an operator. Throws on transport failure. */
export async function pingOperator(operatorId: string): Promise<boolean> {
  const data = await executeGraphQL('router', PingOperatorDocument, { operatorId });
  return data.pingOperator;
}

export async function getProviderDevicesByOperator(operatorId: string): Promise<ProviderDevice[]> {
  const data = await executeGraphQL('router', GetProviderDevicesByOperatorDocument, { operatorId });
  return data.providerDevicesByOperator;
}

export async function getDevicePositions(): Promise<Position[]> {
  const data = await executeGraphQL('router', GetDevicePositionsByUserDocument);
  return data.devicePositionsByUser;
}

export async function getTripsByTransporter(
  transporterId: string,
  from: string | Date,
  to: string | Date,
  source: PositionSourceType = 'PROVIDER'
): Promise<Trip[]> {
  const data = await executeGraphQL('router', GetTripsByTransporterDocument, {
    transporterId,
    from: formatDateTimeOffSet(from) as string,
    to: formatDateTimeOffSet(to) as string,
    source,
  });
  return data.tripsByTransporter;
}

export async function getPositionsByTransporter(
  transporterId: string,
  from: string | Date,
  to: string | Date,
  source: PositionSourceType = 'PROVIDER'
): Promise<Position[]> {
  const data = await executeGraphQL('router', GetPositionsByTransporterDocument, {
    transporterId,
    from: formatDateTimeOffSet(from) as string,
    to: formatDateTimeOffSet(to) as string,
    source,
  });
  return data.positionsByTransporter;
}

/**
 * Silent op: resolves a human-readable address for a coordinate on demand.
 * When a transporterId is provided the backend also persists the resolved
 * address onto the transporter's latest-position row. Throws on failure — the
 * map callers swallow it and keep their local reverse-geocode cache untouched.
 */
export async function reverseGeocode(
  latitude: number,
  longitude: number,
  transporterId: string | null = null
): Promise<Address> {
  const data = await executeGraphQL('router', ReverseGeocodeDocument, {
    latitude,
    longitude,
    transporterId,
  });
  return data.reverseGeocode;
}
