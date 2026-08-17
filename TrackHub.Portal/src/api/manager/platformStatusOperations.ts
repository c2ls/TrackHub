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
 * Platform status GraphQL documents (Manager backend) — the ADMINISTRATOR tier
 * only. The public tier reads announcements through the anonymous REST endpoint
 * instead. Codegen validates these against schemas/manager.graphql.
 */

import { graphql } from './generated';

export const PlatformAnnouncementItemFragment = graphql(`
  fragment PlatformAnnouncementItem on PlatformAnnouncementVm {
    platformAnnouncementId
    messageEn
    messageEs
    severity
    startsAt
    endsAt
    active
    lastModified
  }
`);

export const GetPlatformAnnouncementsDocument = graphql(`
  query GetPlatformAnnouncements($skip: Int!, $take: Int!) {
    platformAnnouncements(query: { skip: $skip, take: $take }) {
      ...PlatformAnnouncementItem
    }
  }
`);

export const GetBackgroundJobStatusDocument = graphql(`
  query GetBackgroundJobStatus {
    backgroundJobStatus {
      jobKey
      status
      startedAt
      completedAt
      attempts
      errorCode
      recordsEveryCycle
    }
  }
`);

export const CreatePlatformAnnouncementDocument = graphql(`
  mutation CreatePlatformAnnouncement($announcement: PlatformAnnouncementDtoInput!) {
    createPlatformAnnouncement(command: { announcement: $announcement }) {
      ...PlatformAnnouncementItem
    }
  }
`);

export const UpdatePlatformAnnouncementDocument = graphql(`
  mutation UpdatePlatformAnnouncement($platformAnnouncementId: UUID!, $announcement: PlatformAnnouncementDtoInput!) {
    updatePlatformAnnouncement(
      command: { platformAnnouncementId: $platformAnnouncementId, announcement: $announcement }
    )
  }
`);

export const DeletePlatformAnnouncementDocument = graphql(`
  mutation DeletePlatformAnnouncement($platformAnnouncementId: UUID!) {
    deletePlatformAnnouncement(command: { platformAnnouncementId: $platformAnnouncementId })
  }
`);
