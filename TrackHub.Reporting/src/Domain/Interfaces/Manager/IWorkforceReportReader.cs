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

using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Domain.Interfaces.Manager;

// Reads the Manager workforce data the spec 09 §13 reports project. Each call runs under the caller's
// propagated token; Manager enforces account scoping and the `workforce` feature gate. All list reads
// page transparently up to the 100k report row limit.
public interface IWorkforceReportReader
{
    // Pre-checks the `workforce` billing feature so a disabled account fails with FEATURE_DISABLED (403)
    // rather than a raw GraphQL error.
    Task EnsureWorkforceFeatureAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReportDriverVm>> GetDriversAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReportDriverQualificationVm>> GetDriverQualificationsAsync(
        Guid? driverId, int? expiringWithinDays, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReportDriverAssignmentVm>> GetDriverAssignmentHistoryAsync(
        Guid? driverId, Guid? transporterId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken);
}
