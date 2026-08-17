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
 * Driver GraphQL documents (Manager backend). Codegen validates these against
 * schemas/manager.graphql and emits typed document nodes — values always travel
 * as variables, never string interpolation.
 */

import { graphql } from './generated';

export const DriverItemFragment = graphql(`
  fragment DriverItem on DriverVm {
    driverId
    accountId
    name
    phone
    documentType
    documentNumber
    active
    employeeCode
    licenseNumber
    licenseExpiresAt
    defaultTransporterId
    lastModified
  }
`);

export const GetDriversByAccountDocument = graphql(`
  query GetDriversByAccount($accountId: UUID!, $skip: Int!, $take: Int!) {
    driversByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) {
      ...DriverItem
    }
  }
`);

export const CreateDriverDocument = graphql(`
  mutation CreateDriver($driver: DriverDtoInput!) {
    createDriver(command: { driver: $driver }) {
      ...DriverItem
    }
  }
`);

export const UpdateDriverDocument = graphql(`
  mutation UpdateDriver($driverId: UUID!, $driver: DriverDtoInput!) {
    updateDriver(command: { driverId: $driverId, driver: $driver })
  }
`);

export const DeactivateDriverDocument = graphql(`
  mutation DeactivateDriver($driverId: UUID!) {
    deactivateDriver(command: { driverId: $driverId })
  }
`);
