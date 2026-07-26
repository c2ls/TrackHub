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
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;
public sealed class GeocodingProviderConfiguration : IEntityTypeConfiguration<GeocodingProvider>
{
    public void Configure(EntityTypeBuilder<GeocodingProvider> builder)
    {
        //Table name
        builder.ToTable(
            name: TableMetadata.GeocodingProvider,
            schema: SchemaMetadata.Map,
            t => t.HasCheckConstraint("ck_geocoding_providers_type", EnumColumn.Check<GeocodingProviderType>("type")));

        //Column names
        builder.Property(x => x.GeocodingProviderId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Type).HasColumnName("type")
            .HasComment(EnumColumn.Comment<GeocodingProviderType>("Geocoding backend this provider row configures."));
        builder.Property(x => x.EndpointUri).HasColumnName("endpointuri");
        builder.Property(x => x.ApiKey).HasColumnName("apikey");
        builder.Property(x => x.Salt).HasColumnName("salt");
        builder.Property(x => x.RequestsPerSecond).HasColumnName("requestspersecond");
        builder.Property(x => x.TimeoutSeconds).HasColumnName("timeoutseconds");
        builder.Property(x => x.ConfigurationJson).HasColumnName("configurationjson");
        builder.Property(x => x.Active).HasColumnName("active");

        builder.Property(x => x.Name)
            .HasMaxLength(ColumnMetadata.DefaultNameLength)
            .IsRequired();

        builder.Property(x => x.EndpointUri)
            .HasMaxLength(ColumnMetadata.DefaultDescriptionLength)
            .IsRequired();

        builder.Property(x => x.ConfigurationJson)
            .HasColumnType(ColumnMetadata.TextField);

        // Exactly one active provider at a time; activation flips the previous one off,
        // and this partial unique index guarantees it at the database level.
        builder.HasIndex(x => x.Active)
            .HasFilter("active = true")
            .IsUnique()
            .HasDatabaseName("ix_geocoding_providers_single_active");
    }
}
