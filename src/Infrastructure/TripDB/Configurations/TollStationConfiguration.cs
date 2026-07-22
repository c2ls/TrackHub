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

public sealed class TollStationConfiguration : IEntityTypeConfiguration<TollStation>
{
    public void Configure(EntityTypeBuilder<TollStation> builder)
    {
        builder.ToTable(name: TableMetadata.TollStation, schema: SchemaMetadata.Trip);
        builder.HasKey(x => x.TollStationId);

        builder.Property(x => x.TollStationId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(40);
        builder.Property(x => x.Point).HasColumnName("point").HasColumnType("geometry (Point, 4326)").IsRequired();
        builder.Property(x => x.Country).HasColumnName("country").HasMaxLength(2);
        builder.Property(x => x.Region).HasColumnName("region").HasMaxLength(ColumnMetadata.DefaultFieldLength);
        builder.Property(x => x.RoadName).HasColumnName("roadname").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.Direction).HasColumnName("direction").HasMaxLength(50);
        builder.Property(x => x.Operator).HasColumnName("operator").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(x => x.Active).HasColumnName("active").HasDefaultValue(true);

        // TWO partial unique indexes, not one over (name, code). `Code` is nullable and PostgreSQL
        // treats NULLs as distinct, so the single index accepted unlimited (name, NULL) rows — and
        // an operator entering the same code-less station twice got it matched twice by
        // ST_DWithin and its toll charged twice in the estimate, because dedup is by TollStationId.
        // Same defect and same remedy as TransporterTollClassConfiguration.
        builder.HasIndex(x => new { x.Name, x.Code }, "toll_stations_name_code")
            .HasDatabaseName("ux_toll_stations_name_code")
            .IsUnique()
            .HasFilter("code is not null");

        builder.HasIndex(x => x.Name, "toll_stations_name_only")
            .HasDatabaseName("ux_toll_stations_name_nocode")
            .IsUnique()
            .HasFilter("code is null");

        // Route matching is ST_DWithin(point, planned line, tolerance) against this index.
        builder.HasIndex(x => x.Point)
            .HasDatabaseName("ix_toll_stations_point_gist")
            .HasMethod("gist");
    }
}
