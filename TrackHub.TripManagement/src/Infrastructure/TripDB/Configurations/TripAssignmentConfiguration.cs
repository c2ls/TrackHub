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

namespace TrackHub.TripManagement.Infrastructure.TripDB.Configurations;

public sealed class TripAssignmentConfiguration : IEntityTypeConfiguration<TripAssignment>
{
    public void Configure(EntityTypeBuilder<TripAssignment> builder)
    {
        builder.ToTable(name: TableMetadata.TripAssignment, schema: SchemaMetadata.Trip);
        builder.HasKey(x => x.TripAssignmentId);

        builder.Property(x => x.TripAssignmentId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.TripId).HasColumnName("tripid");
        builder.Property(x => x.DriverId).HasColumnName("driverid");
        builder.Property(x => x.TransporterId).HasColumnName("transporterid");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(40).IsRequired();
        builder.Property(x => x.AssignedAt).HasColumnName("assignedat");
        builder.Property(x => x.AcknowledgedAt).HasColumnName("acknowledgedat");
        builder.Property(x => x.EndedAt).HasColumnName("endedat");

        builder.HasIndex(x => new { x.AccountId, x.TripId })
            .HasDatabaseName("ix_trip_assignments_accountid_tripid");

        // Exactly ONE Active assignment per trip, enforced by the database rather than by handler
        // discipline (spec 11 section 6.1). A duplicate assign attempt surfaces as CONFLICT.
        builder.HasIndex(x => x.TripId)
            .HasDatabaseName("ux_trip_assignments_active_per_trip")
            .IsUnique()
            .HasFilter("status = 'Active'");

        builder.HasOne(x => x.Trip)
            .WithMany()
            .HasForeignKey(x => x.TripId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
