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

namespace TrackHub.Geofencing.Infrastructure.ManagerDB.Configurations;
public sealed class VmTransporterPositionConfiguration : IEntityTypeConfiguration<VwTransporterPosition>
{
    public void Configure(EntityTypeBuilder<VwTransporterPosition> builder)
    {
        //Table name
        builder.ToView(name: ViewMetadata.VwTransporterPosition, schema: SchemaMetadata.Geofencing);

        //Column names
        builder.Property(x => x.TransporterId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(e => e.Geom)
            .HasColumnName("geom")
            .HasColumnType("geometry(Point, 4326)");
        builder.Property(x => x.UserId).HasColumnName("userid");

        //Indexes
        builder.HasKey(e => e.TransporterId);
    }
}

