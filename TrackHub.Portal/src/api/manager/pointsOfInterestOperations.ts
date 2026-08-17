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
 * Point-of-interest GraphQL documents (Manager backend). Codegen validates these
 * against schemas/manager.graphql and emits typed document nodes — values always
 * travel as variables, never string interpolation.
 */

import { graphql } from './generated';

export const PointOfInterestItemFragment = graphql(`
  fragment PointOfInterestItem on PointOfInterestVm {
    pointOfInterestId
    accountId
    name
    description
    type
    latitude
    longitude
    address
    color
    groupId
    active
  }
`);

export const GetPointsOfInterestByAccountDocument = graphql(`
  query GetPointsOfInterestByAccount {
    pointsOfInterestByAccount {
      ...PointOfInterestItem
    }
  }
`);

export const CreatePointOfInterestDocument = graphql(`
  mutation CreatePointOfInterest($pointOfInterest: PointOfInterestDtoInput!) {
    createPointOfInterest(command: { pointOfInterest: $pointOfInterest }) {
      ...PointOfInterestItem
    }
  }
`);

export const UpdatePointOfInterestDocument = graphql(`
  mutation UpdatePointOfInterest($id: UUID!, $pointOfInterest: UpdatePointOfInterestDtoInput!) {
    updatePointOfInterest(id: $id, command: { id: $id, pointOfInterest: $pointOfInterest })
  }
`);

export const DeletePointOfInterestDocument = graphql(`
  mutation DeletePointOfInterest($id: UUID!) {
    deletePointOfInterest(id: $id)
  }
`);
