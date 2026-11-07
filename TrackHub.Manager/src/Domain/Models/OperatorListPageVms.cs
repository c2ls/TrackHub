// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

namespace TrackHub.Manager.Domain.Models;

public readonly record struct AlertEventsPageVm(IReadOnlyCollection<AlertEventVm> Items, int TotalCount);

public readonly record struct AlertSeverityCountVm(string Severity, int Count);

public readonly record struct StaffMembersPageVm(IReadOnlyCollection<StaffMemberVm> Items, int TotalCount);

public readonly record struct StaffOptionsPageVm(IReadOnlyCollection<StaffLookupVm> Items, int TotalCount);

public readonly record struct StaffQualificationsPageVm(IReadOnlyCollection<StaffQualificationVm> Items, int TotalCount);

public readonly record struct StaffAssignmentHistoryPageVm(IReadOnlyCollection<StaffTransporterAssignmentVm> Items, int TotalCount);

public readonly record struct DocumentsPageVm(IReadOnlyCollection<DocumentVm> Items, int TotalCount);

public readonly record struct NotificationRulesPageVm(IReadOnlyCollection<NotificationRuleVm> Items, int TotalCount);

public readonly record struct NotificationDeliveriesPageVm(IReadOnlyCollection<NotificationDeliveryVm> Items, int TotalCount);

public readonly record struct AlertSubscriptionsPageVm(IReadOnlyCollection<AlertSubscriptionVm> Items, int TotalCount);

public readonly record struct PublicLinkGrantsPageVm(IReadOnlyCollection<PublicLinkGrantVm> Items, int TotalCount);

public readonly record struct BackgroundJobRunsPageVm(IReadOnlyCollection<BackgroundJobRunVm> Items, int TotalCount);
