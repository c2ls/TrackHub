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

public sealed class TollVehicleClassConfiguration : IEntityTypeConfiguration<TollVehicleClass>
{
    public void Configure(EntityTypeBuilder<TollVehicleClass> builder)
    {
        builder.ToTable(name: TableMetadata.TollVehicleClass, schema: SchemaMetadata.Trip);
        builder.HasKey(x => x.TollVehicleClassId);

        builder.Property(x => x.TollVehicleClassId).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(ColumnMetadata.DefaultFieldLength).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(ColumnMetadata.DefaultDescriptionLength);
        builder.Property(x => x.SortOrder).HasColumnName("sortorder");
        builder.Property(x => x.Active).HasColumnName("active").HasDefaultValue(true);

        // Code is the value toll_tariffs and transporter_toll_classes reference, so it is a real
        // alternate key rather than merely a unique index — that is what makes those two columns
        // FK-able instead of dangling strings validated only for length.
        // Keeps its original name so TollCatalogWriter's 23505 -> "duplicate code" translation
        // still matches; a PostgreSQL unique CONSTRAINT is backed by a unique index, so lookups are
        // unaffected by the promotion.
        builder.HasAlternateKey(x => x.Code)
            .HasName("ux_toll_vehicle_classes_code");
    }
}
