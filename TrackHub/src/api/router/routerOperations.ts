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
 * Router GraphQL documents (Router backend). Codegen validates these against
 * schemas/router.graphql and emits typed document nodes — values always
 * travel as variables, never string interpolation.
 */

import { graphql } from './generated';

export const PositionFieldsFragment = graphql(`
  fragment PositionFields on PositionVm {
    transporterId
    deviceName
    transporterType
    latitude
    longitude
    altitude
    speed
    course
    deviceDateTime
    serverDateTime
    eventId
    address
    city
    state
    country
    attributes {
      ignition
      satellites
      mileage
      hourmeter
      temperature
    }
  }
`);

export const TripFieldsFragment = graphql(`
  fragment TripFields on TripVm {
    tripId
    totalDistance
    duration
    averageSpeed
    type
    from
    to
    points {
      latitude
      longitude
      deviceDateTime
      speed
      course
      eventId
    }
  }
`);

export const AddressFieldsFragment = graphql(`
  fragment AddressFields on AddressVm {
    address
    city
    state
    country
  }
`);

export const PingOperatorDocument = graphql(`
  query PingOperator($operatorId: UUID!) {
    pingOperator(query: { operatorId: $operatorId })
  }
`);

export const GetProviderDevicesByOperatorDocument = graphql(`
  query GetProviderDevicesByOperator($operatorId: UUID!) {
    providerDevicesByOperator(query: { operatorId: $operatorId }) {
      identifier
      name
      serial
      deviceTypeId
      transporterTypeId
    }
  }
`);

export const GetDevicePositionsByUserDocument = graphql(`
  query GetDevicePositionsByUser {
    devicePositionsByUser {
      ...PositionFields
    }
  }
`);

export const GetTripsByTransporterDocument = graphql(`
  query GetTripsByTransporter(
    $transporterId: UUID!
    $from: DateTime!
    $to: DateTime!
    $source: PositionSourceType!
  ) {
    tripsByTransporter(
      query: { transporterId: $transporterId, from: $from, to: $to, source: $source }
    ) {
      ...TripFields
    }
  }
`);

export const GetPositionsByTransporterDocument = graphql(`
  query GetPositionsByTransporter(
    $transporterId: UUID!
    $from: DateTime!
    $to: DateTime!
    $source: PositionSourceType!
  ) {
    positionsByTransporter(
      query: { transporterId: $transporterId, from: $from, to: $to, source: $source }
    ) {
      ...PositionFields
    }
  }
`);

export const ReverseGeocodeDocument = graphql(`
  query ReverseGeocode($latitude: Float!, $longitude: Float!, $transporterId: UUID) {
    reverseGeocode(
      query: { latitude: $latitude, longitude: $longitude, transporterId: $transporterId }
    ) {
      ...AddressFields
    }
  }
`);
