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

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;

public interface IApplicationDbContext
{
    // Telemetry-owned (schema telemetry) — read/write.
    DbSet<TransporterPosition> TransporterPositions { get; }
    DbSet<TransporterPositionHistory> TransporterPositionHistory { get; }
    DbSet<OperatorSyncRun> OperatorSyncRuns { get; }
    DbSet<OperatorHealthCheck> OperatorHealthChecks { get; }

    // Read-only scoping projections (schema app, spec 01.3 §5.2).
    DbSet<Transporter> Transporters { get; }
    DbSet<Device> Devices { get; }
    DbSet<TransporterDeviceAssignment> TransporterDeviceAssignments { get; }
    DbSet<User> Users { get; }
    DbSet<Group> Groups { get; }
    DbSet<UserGroup> UsersGroup { get; }
    DbSet<Operator> Operators { get; }
    DbSet<AccountFeature> AccountFeatures { get; }
    DbSet<Account> Accounts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
