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
using Common.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TrackHub.Geofencing.Infrastructure.Configurations;
public class GeofenceConfiguration : IEntityTypeConfiguration<Geofence>
{
    public void Configure(EntityTypeBuilder<Geofence> builder)
    {
        //Table name
        builder.ToTable(
            name: TableMetadata.Geofence,
            schema: SchemaMetadata.Geofencing,
            t => t.HasCheckConstraint(
                "ck_geofences_type",
                EnumColumn.Check<GeofenceType>("type")));

        //Column names
        builder.Property(x => x.GeofenceId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(e => e.Geom)
            .HasColumnName("geom")
            .HasColumnType("geometry (Polygon, 4326)");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.Color).HasColumnName("color");
        builder.Property(x => x.Type).HasColumnName("type")
            .HasComment(EnumColumn.Comment<GeofenceType>("Category of the geofence."));
        builder.Property(x => x.Active).HasColumnName("active");
        builder.Property(e => e.CircleCenter)
            .HasColumnName("circlecenter")
            .HasColumnType("geometry (Point, 4326)");
        builder.Property(x => x.CircleRadiusMeters).HasColumnName("circleradiusmeters");
        builder.Property(x => x.AlertOnEntry).HasColumnName("alertonentry").HasDefaultValue(false);
        builder.Property(x => x.AlertOnExit).HasColumnName("alertonexit").HasDefaultValue(false);
        builder.Property(x => x.DwellThresholdMinutes).HasColumnName("dwellthresholdminutes");

        builder.Property(t => t.Name)
            .HasMaxLength(ColumnMetadata.DefaultNameLength)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(ColumnMetadata.DefaultDescriptionLength);

        builder
            .HasIndex(e => e.Geom)
            .HasDatabaseName("geofence_idx")
            .HasAnnotation("Npgsql:IndexMethod", "gist");

        builder
            .HasIndex(e => e.AccountId)
            .HasDatabaseName("ix_geofences_accountid");
    }
}

