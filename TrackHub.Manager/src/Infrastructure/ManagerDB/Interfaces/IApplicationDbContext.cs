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

using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Account> Accounts { get; set; }
    DbSet<AccountBranding> AccountBrandings { get; set; }
    DbSet<AccountFeature> AccountFeatures { get; set; }
    DbSet<AccountSettings> AccountSettings { get; set; }
    DbSet<AccountSupportGrant> AccountSupportGrants { get; set; }
    DbSet<AlertEvent> AlertEvents { get; set; }
    DbSet<AlertSubscription> AlertSubscriptions { get; set; }
    DbSet<AuditEvent> AuditEvents { get; set; }
    DbSet<BackgroundJobRun> BackgroundJobRuns { get; set; }
    DbSet<PlatformAnnouncement> PlatformAnnouncements { get; set; }
    DbSet<Credential> Credentials { get; set; }
    DbSet<Document> Documents { get; set; }
    DbSet<DocumentVersion> DocumentVersions { get; set; }
    DbSet<DocumentSignature> DocumentSignatures { get; set; }
    DbSet<DocumentType> DocumentTypes { get; set; }
    DbSet<Driver> Drivers { get; set; }
    DbSet<DriverQualification> DriverQualifications { get; set; }
    DbSet<DriverTransporterAssignment> DriverTransporterAssignments { get; set; }
    DbSet<GeocodingProvider> GeocodingProviders { get; set; }
    DbSet<PointOfInterest> PointsOfInterest { get; set; }
    DbSet<Transporter> Transporters { get; set; }
    DbSet<TransporterGroup> TransportersGroup { get; set; }
    DbSet<Device> Devices { get; set; }
    DbSet<Group> Groups { get; set; }
    DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
    DbSet<NotificationRule> NotificationRules { get; set; }
    DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    DbSet<Operator> Operators { get; set; }
    DbSet<PublicLinkGrant> PublicLinkGrants { get; set; }
    DbSet<Report> Reports { get; set; }
    DbSet<TransporterPosition> TransporterPositions { get; set; }
    DbSet<TransporterPositionHistory> TransporterPositionHistory { get; set; }
    DbSet<TransporterDeviceAssignment> TransporterDeviceAssignments { get; set; }
    DbSet<OperatorHealthCheck> OperatorHealthChecks { get; set; }
    DbSet<OperatorSyncRun> OperatorSyncRuns { get; set; }
    DbSet<TransporterType> TransporterTypes { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<UserGroup> UsersGroup { get; set; }
    DbSet<UserSettings> UserSettings { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
