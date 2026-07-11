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

using System.Reflection;
using Common.Infrastructure;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<TransporterPosition> TransporterPositions { get; set; }
    public DbSet<TransporterPositionHistory> TransporterPositionHistory { get; set; }
    public DbSet<OperatorSyncRun> OperatorSyncRuns { get; set; }
    public DbSet<OperatorHealthCheck> OperatorHealthChecks { get; set; }

    public DbSet<Transporter> Transporters { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<TransporterDeviceAssignment> TransporterDeviceAssignments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<UserGroup> UsersGroup { get; set; }
    public DbSet<Operator> Operators { get; set; }
    public DbSet<AccountFeature> AccountFeatures { get; set; }
    public DbSet<Account> Accounts { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.UseUtcTimestamps();
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}
