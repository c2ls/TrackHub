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

public sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable(name: TableMetadata.Trip, schema: SchemaMetadata.Trip);
        builder.HasKey(x => x.TripId);

        builder.Property(x => x.TripId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(40).IsRequired();
        builder.Property(x => x.TransporterId).HasColumnName("transporterid");
        builder.Property(x => x.DriverId).HasColumnName("driverid");
        builder.Property(x => x.RoutePlanId).HasColumnName("routeplanid");
        builder.Property(x => x.ServiceOrderId).HasColumnName("serviceorderid");
        builder.Property(x => x.ExternalReference).HasColumnName("externalreference").HasMaxLength(80);
        builder.Property(x => x.CustomerName).HasColumnName("customername").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.OriginName).HasColumnName("originname").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.OriginPoint).HasColumnName("originpoint").HasColumnType("geometry (Point, 4326)").IsRequired();
        builder.Property(x => x.PlannedStartAt).HasColumnName("plannedstartat");
        builder.Property(x => x.PlannedEndAt).HasColumnName("plannedendat");
        builder.Property(x => x.ActualStartAt).HasColumnName("actualstartat");
        builder.Property(x => x.ActualEndAt).HasColumnName("actualendat");
        builder.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(2000);
        builder.Property(x => x.LastPositionAt).HasColumnName("lastpositionat");
        builder.Property(x => x.LastPoint).HasColumnName("lastpoint").HasColumnType("geometry (Point, 4326)");
        builder.Property(x => x.ActualDistanceMeters).HasColumnName("actualdistancemeters").HasDefaultValue(0d);
        builder.Property(x => x.TollVehicleClass).HasColumnName("tollvehicleclass").HasMaxLength(20);
        builder.Property(x => x.DeviationOpenedAt).HasColumnName("deviationopenedat");
        builder.Property(x => x.ConsecutiveOutsideFixes).HasColumnName("consecutiveoutsidefixes").HasDefaultValue(0);
        builder.Property(x => x.CancellationReason).HasColumnName("cancellationreason").HasMaxLength(ColumnMetadata.DefaultDescriptionLength);

        // Dispatch board, transporter board and driver board (spec 11 section 6 "Indexes (trips)").
        builder.HasIndex(x => new { x.AccountId, x.Status, x.PlannedStartAt })
            .HasDatabaseName("ix_trips_accountid_status_plannedstartat");
        builder.HasIndex(x => new { x.AccountId, x.TransporterId, x.PlannedStartAt })
            .HasDatabaseName("ix_trips_accountid_transporterid_plannedstartat");
        builder.HasIndex(x => new { x.AccountId, x.DriverId, x.Status })
            .HasDatabaseName("ix_trips_accountid_driverid_status");

        builder.HasIndex(x => new { x.AccountId, x.Code })
            .HasDatabaseName("ux_trips_accountid_code")
            .IsUnique();

        // Partner/TMS import is idempotent on ExternalReference, which is unique per account
        // only where it is supplied (spec 11 section 7.9).
        builder.HasIndex(x => new { x.AccountId, x.ExternalReference })
            .HasDatabaseName("ux_trips_accountid_externalreference")
            .IsUnique()
            .HasFilter("externalreference IS NOT NULL");
    }
}
