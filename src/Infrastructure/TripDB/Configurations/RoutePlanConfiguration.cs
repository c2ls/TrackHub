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

public sealed class RoutePlanConfiguration : IEntityTypeConfiguration<RoutePlan>
{
    public void Configure(EntityTypeBuilder<RoutePlan> builder)
    {
        builder.ToTable(name: TableMetadata.TripRoutePlan, schema: SchemaMetadata.Trip);
        builder.HasKey(x => x.RoutePlanId);

        builder.Property(x => x.RoutePlanId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.TripId).HasColumnName("tripid");
        builder.Property(x => x.Provider).HasColumnName("provider").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Geom).HasColumnName("geom").HasColumnType("geometry (LineString, 4326)");
        builder.Property(x => x.CorridorGeom).HasColumnName("corridorgeom").HasColumnType("geometry (Polygon, 4326)");
        builder.Property(x => x.CorridorMeters).HasColumnName("corridormeters")
            .HasDefaultValue(TripGeometryDefaults.CorridorMeters);
        builder.Property(x => x.PlannedDistanceMeters).HasColumnName("planneddistancemeters");
        builder.Property(x => x.PlannedDurationSeconds).HasColumnName("planneddurationseconds");
        builder.Property(x => x.WaypointsJson).HasColumnName("waypointsjson").HasColumnType(ColumnMetadata.TextField);
        builder.Property(x => x.LegsJson).HasColumnName("legsjson").HasColumnType(ColumnMetadata.TextField);
        builder.Property(x => x.ComputedAt).HasColumnName("computedat");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(40).IsRequired();
        builder.Property(x => x.ErrorCode).HasColumnName("errorcode").HasMaxLength(ColumnMetadata.DefaultFieldLength);
        builder.Property(x => x.ErrorMessage).HasColumnName("errormessage").HasMaxLength(ColumnMetadata.DefaultDescriptionLength);
        builder.Property(x => x.TollVehicleClass).HasColumnName("tollvehicleclass").HasMaxLength(20);
        builder.Property(x => x.EstimatedTollAmount).HasColumnName("estimatedtollamount").HasPrecision(18, 2);
        builder.Property(x => x.TollCurrency).HasColumnName("tollcurrency").HasMaxLength(3);
        builder.Property(x => x.TollStationsJson).HasColumnName("tollstationsjson").HasColumnType(ColumnMetadata.TextField);
        builder.Property(x => x.TollStatus).HasColumnName("tollstatus").HasMaxLength(40).IsRequired();

        builder.HasIndex(x => new { x.AccountId, x.TripId })
            .HasDatabaseName("ix_route_plans_accountid_tripid");

        builder.HasIndex(x => x.Geom)
            .HasDatabaseName("ix_route_plans_geom_gist")
            .HasMethod("gist");

        // Route-deviation detection is a ST_Contains against the corridor on every fix.
        builder.HasIndex(x => x.CorridorGeom)
            .HasDatabaseName("ix_route_plans_corridorgeom_gist")
            .HasMethod("gist");

        builder.HasOne(x => x.Trip)
            .WithMany()
            .HasForeignKey(x => x.TripId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
