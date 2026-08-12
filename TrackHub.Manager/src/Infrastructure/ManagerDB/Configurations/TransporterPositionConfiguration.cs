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
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public sealed class TransporterPositionConfiguration : IEntityTypeConfiguration<TransporterPosition>
{
    public void Configure(EntityTypeBuilder<TransporterPosition> builder)
    {
        //Table name
        builder.ToTable(name: TableMetadata.TransporterPosition, schema: SchemaMetadata.Telemetry);

        //Column names
        builder.Property(x => x.TransporterPositionId).HasColumnName("id");
        builder.Property(x => x.TransporterId).HasColumnName("transporterid");
        builder.Property(x => x.GeometryId).HasColumnName("geometryid");
        builder.Property(x => x.Latitude).HasColumnName("latitude");
        builder.Property(x => x.Longitude).HasColumnName("longitude");
        builder.Property(x => x.Altitude).HasColumnName("altitude");
        builder.Property(x => x.DeviceDateTime).HasColumnName("datetime");
        builder.Property(x => x.Speed).HasColumnName("speed");
        builder.Property(x => x.Course).HasColumnName("course");
        builder.Property(x => x.EventId).HasColumnName("eventId");
        builder.Property(x => x.Address).HasColumnName("address");
        builder.Property(x => x.City).HasColumnName("city");
        builder.Property(x => x.State).HasColumnName("state");
        builder.Property(x => x.Country).HasColumnName("country");

        builder.Property(e => e.Attributes)
            .HasColumnName("attributes")
            .HasColumnType("json");

    }
}
