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

public class AccountFeatureConfiguration : IEntityTypeConfiguration<AccountFeature>
{
    public void Configure(EntityTypeBuilder<AccountFeature> builder)
    {
        // Manager-owned table: read-only here, never part of this repo's migrations
        builder.ToTable(name: TableMetadata.AccountFeature, schema: SchemaMetadata.Application, t => t.ExcludeFromMigrations());
        builder.Property(x => x.AccountFeatureId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.FeatureKey).HasColumnName("featurekey");
        builder.Property(x => x.Enabled).HasColumnName("enabled");
        builder.Property(x => x.EffectiveFrom).HasColumnName("effectivefrom");
        builder.Property(x => x.EffectiveTo).HasColumnName("effectiveto");
        builder.HasKey(x => x.AccountFeatureId);
        builder.HasIndex(x => new { x.AccountId, x.FeatureKey });
    }
}
