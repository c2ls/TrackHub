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

/// <summary>Server-side page of transporters plus the unpaged total for pagination controls.</summary>
public readonly record struct TransportersPageVm(
    IReadOnlyCollection<TransporterVm> Items,
    int TotalCount);

/// <summary>Server-side page of devices plus the unpaged total for pagination controls.</summary>
public readonly record struct DevicesPageVm(
    IReadOnlyCollection<DeviceVm> Items,
    int TotalCount);

/// <summary>Server-side page of groups plus the unpaged total for pagination controls.</summary>
public readonly record struct GroupsPageVm(
    IReadOnlyCollection<GroupVm> Items,
    int TotalCount);

/// <summary>Server-side page of operators plus the unpaged total for pagination controls.</summary>
public readonly record struct OperatorsPageVm(
    IReadOnlyCollection<OperatorVm> Items,
    int TotalCount);

/// <summary>Server-side page of users plus the unpaged total for pagination controls.</summary>
public readonly record struct UsersPageVm(
    IReadOnlyCollection<UserVm> Items,
    int TotalCount);

/// <summary>Server-side page of points of interest plus the unpaged total for pagination controls.</summary>
public readonly record struct PointsOfInterestPageVm(
    IReadOnlyCollection<PointOfInterestVm> Items,
    int TotalCount);

/// <summary>Server-side page of accounts plus the unpaged total for pagination controls.</summary>
public readonly record struct AccountsPageVm(
    IReadOnlyCollection<AccountVm> Items,
    int TotalCount);

/// <summary>Server-side page of transporter/device assignments plus the unpaged total.</summary>
public readonly record struct TransporterDeviceAssignmentsPageVm(
    IReadOnlyCollection<TransporterDeviceAssignmentVm> Items,
    int TotalCount);
