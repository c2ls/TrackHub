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
 * Alert-event GraphQL documents (Manager backend). Codegen validates these
 * against schemas/manager.graphql and emits typed document nodes — values
 * always travel as variables, never string interpolation.
 */

import { graphql } from './generated';

export const AlertEventItemFragment = graphql(`
  fragment AlertEventItem on AlertEventVm {
    alertEventId
    accountId
    eventType
    severity
    sourceModule
    resourceType
    resourceId
    status
    firstSeenAt
    lastSeenAt
    deduplicationKey
  }
`);

export const GetAlertEventsDocument = graphql(`
  query GetAlertEvents($accountId: UUID!, $skip: Int!, $take: Int!) {
    alertEvents(query: { accountId: $accountId, skip: $skip, take: $take }) {
      ...AlertEventItem
    }
  }
`);

export const AcknowledgeAlertEventDocument = graphql(`
  mutation AcknowledgeAlertEvent($alertEventId: UUID!) {
    acknowledgeAlertEvent(command: { alertEventId: $alertEventId })
  }
`);

export const ResolveAlertEventDocument = graphql(`
  mutation ResolveAlertEvent($alertEventId: UUID!) {
    resolveAlertEvent(command: { alertEventId: $alertEventId })
  }
`);
