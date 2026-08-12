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

using TrackHub.Reporting.Domain.Models.Manager;

namespace TrackHub.Reporting.Domain.Interfaces.Telemetry;

// Telemetry reads served by the TrackHub.Telemetry service. Master-data reads
// (operators, devices, assignments) remain on IGpsManagerReader.
public interface IGpsTelemetryReader
{
    Task<ManagerOperatorHealthSummaryVm> GetOperatorHealthSummaryAsync(Guid operatorId, int lookbackHours, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ManagerOperatorSyncRunVm>> GetOperatorSyncRunsAsync(Guid accountId, Guid? operatorId, int take, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ManagerTransporterPositionVm>> GetLatestPositionsAsync(Guid operatorId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ManagerTransporterPositionHistoryVm>> GetPositionHistoryAsync(Guid accountId, Guid? transporterId, Guid? deviceId, int take, CancellationToken cancellationToken);
}
