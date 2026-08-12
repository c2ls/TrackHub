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

namespace TrackHub.Geofencing.Infrastructure.Configurations;

public class GeofenceEventConfiguration : IEntityTypeConfiguration<GeofenceEvent>
{
    public void Configure(EntityTypeBuilder<GeofenceEvent> builder)
    {
        // Table name - using geofencing schema
        builder.ToTable(name: TableMetadata.GeofenceEvent, schema: SchemaMetadata.Geofencing);

        // Primary key
        builder.HasKey(x => x.GeofenceEventId);

        // Column names
        builder.Property(x => x.GeofenceEventId).HasColumnName("id");
        builder.Property(x => x.TransporterId).HasColumnName("transporterid");
        builder.Property(x => x.GeofenceId).HasColumnName("geofenceid");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.EventDateTime).HasColumnName("datetime");
        builder.Property(x => x.DepartureTimestamp).HasColumnName("departuretimestamp");
        builder.Property(x => x.Latitude).HasColumnName("latitude");
        builder.Property(x => x.Longitude).HasColumnName("longitude");
        builder.Property(x => x.DwellAlertedAt).HasColumnName("dwellalertedat");
        // Indexes
        builder.HasIndex(x => x.TransporterId).HasDatabaseName("ix_geofenceevent_transporterid");
        builder.HasIndex(x => x.GeofenceId).HasDatabaseName("ix_geofenceevent_geofenceid");
        builder.HasIndex(x => new { x.AccountId, x.EventDateTime })
            .HasDatabaseName("ix_geofenceevent_accountid_datetime");
        builder.HasIndex(x => new { x.TransporterId, x.GeofenceId, x.DepartureTimestamp })
            .HasDatabaseName("ix_geofenceevent_open_events")
            .HasFilter("departuretimestamp IS NULL");

        // Relationships
        builder.HasOne(x => x.Geofence)
            .WithMany()
            .HasForeignKey(x => x.GeofenceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
