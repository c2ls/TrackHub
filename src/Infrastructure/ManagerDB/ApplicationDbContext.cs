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
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountBranding> AccountBrandings { get; set; }
    public DbSet<AccountFeature> AccountFeatures { get; set; }
    public DbSet<AccountSettings> AccountSettings { get; set; }
    public DbSet<AccountSupportGrant> AccountSupportGrants { get; set; }
    public DbSet<AlertEvent> AlertEvents { get; set; }
    public DbSet<AlertSubscription> AlertSubscriptions { get; set; }
    public DbSet<AuditEvent> AuditEvents { get; set; }
    public DbSet<BackgroundJobRun> BackgroundJobRuns { get; set; }
    public DbSet<PlatformAnnouncement> PlatformAnnouncements { get; set; }
    public DbSet<Credential> Credentials { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentVersion> DocumentVersions { get; set; }
    public DbSet<DocumentSignature> DocumentSignatures { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<DriverQualification> DriverQualifications { get; set; }
    public DbSet<DriverTransporterAssignment> DriverTransporterAssignments { get; set; }
    public DbSet<GeocodingProvider> GeocodingProviders { get; set; }
    public DbSet<PointOfInterest> PointsOfInterest { get; set; }
    public DbSet<Transporter> Transporters { get; set; }
    public DbSet<TransporterGroup> TransportersGroup { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
    public DbSet<NotificationRule> NotificationRules { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<Operator> Operators { get; set; }
    public DbSet<PublicLinkGrant> PublicLinkGrants { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<TransporterPosition> TransporterPositions { get; set; }
    public DbSet<TransporterPositionHistory> TransporterPositionHistory { get; set; }
    public DbSet<TransporterDeviceAssignment> TransporterDeviceAssignments { get; set; }
    public DbSet<OperatorHealthCheck> OperatorHealthChecks { get; set; }
    public DbSet<OperatorSyncRun> OperatorSyncRuns { get; set; }
    public DbSet<TransporterType> TransporterTypes { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserGroup> UsersGroup { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Normalize every DateTimeOffset to UTC on write (workspace UTC-timestamp policy).
        configurationBuilder.UseUtcTimestamps();
        base.ConfigureConventions(configurationBuilder);
    }

}
