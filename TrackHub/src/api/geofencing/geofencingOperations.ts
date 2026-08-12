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
 * Geofencing GraphQL documents (Geofencing backend). Merges the former
 * geofence.js + geofencing.js hooks. Codegen validates these against
 * schemas/geofencing.graphql; values always travel as variables.
 */

import { graphql } from './generated';

export const GeofenceFieldsFragment = graphql(`
  fragment GeofenceFields on GeofenceVm {
    geofenceId
    accountId
    name
    description
    type
    color
    active
    circleCenter {
      latitude
      longitude
    }
    circleRadiusMeters
    alertOnEntry
    alertOnExit
    dwellThresholdMinutes
    geom {
      srid
      coordinates {
        latitude
        longitude
      }
    }
  }
`);

export const GetGeofenceDocument = graphql(`
  query GetGeofence($id: UUID!) {
    geofence(query: { id: $id }) {
      ...GeofenceFields
    }
  }
`);

export const GetGeofencesByAccountDocument = graphql(`
  query GetGeofencesByAccount(
    $enableCaching: Boolean!
    $skip: Int
    $take: Int
    $type: Short
    $active: Boolean
    $search: String
  ) {
    geofencesByAccount(
      query: {
        enableCaching: $enableCaching
        skip: $skip
        take: $take
        type: $type
        active: $active
        search: $search
      }
    ) {
      items {
        ...GeofenceFields
      }
      totalCount
    }
  }
`);

export const GetTransportersInGeofenceDocument = graphql(`
  query GetTransportersInGeofence($geofenceId: UUID, $type: Short) {
    transportersInGeofence(geofenceId: $geofenceId, type: $type) {
      transporterId
      transporterName
      geofenceId
      geofenceName
    }
  }
`);

export const CreateGeofenceDocument = graphql(`
  mutation CreateGeofence($geofence: GeofenceDtoInput!) {
    createGeofence(command: { geofence: $geofence }) {
      ...GeofenceFields
    }
  }
`);

export const UpdateGeofenceDocument = graphql(`
  mutation UpdateGeofence($id: UUID!, $geofence: GeofenceDtoInput!) {
    updateGeofence(id: $id, command: { geofence: $geofence })
  }
`);

export const DeleteGeofenceDocument = graphql(`
  mutation DeleteGeofence($id: UUID!) {
    deleteGeofence(id: $id)
  }
`);
