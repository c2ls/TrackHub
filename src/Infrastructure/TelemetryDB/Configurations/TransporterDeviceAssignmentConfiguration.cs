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

using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB.Configurations;

// Read-only scoping map (spec 01.3 §5.2): Telemetry reads this app-schema table cross-schema; it never writes it.
public sealed class TransporterDeviceAssignmentConfiguration : IEntityTypeConfiguration<TransporterDeviceAssignment>
{
    public void Configure(EntityTypeBuilder<TransporterDeviceAssignment> builder)
    {
        builder.ToTable(name: TableMetadata.TransporterDeviceAssignment, schema: SchemaMetadata.Application);
        builder.HasKey(x => x.TransporterDeviceAssignmentId);
        builder.Property(x => x.TransporterDeviceAssignmentId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.TransporterId).HasColumnName("transporterid");
        builder.Property(x => x.DeviceId).HasColumnName("deviceid");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.IsPrimary).HasColumnName("isprimary");
        builder.Property(x => x.Priority).HasColumnName("priority");
        builder.HasOne(e => e.Transporter).WithMany(t => t.Assignments).HasForeignKey(e => e.TransporterId);
        builder.HasOne(e => e.Device).WithMany(d => d.Assignments).HasForeignKey(e => e.DeviceId);
    }
}
