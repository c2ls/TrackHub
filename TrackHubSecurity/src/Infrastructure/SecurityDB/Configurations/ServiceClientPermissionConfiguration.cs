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

namespace TrackHub.Security.Infrastructure.SecurityDB.Configurations;

public class ServiceClientPermissionConfiguration : IEntityTypeConfiguration<ServiceClientPermission>
{
    public void Configure(EntityTypeBuilder<ServiceClientPermission> builder)
    {
        builder.ToTable(name: TableMetadata.ServiceClientPermission, schema: SchemaMetadata.Security);

        builder.Property(x => x.ServiceClientPermissionId).HasColumnName("id");
        builder.Property(x => x.ClientId).HasColumnName("clientid");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.Resource).HasColumnName("resource");
        builder.Property(x => x.Action).HasColumnName("action");
        builder.Property(x => x.Scope).HasColumnName("scope");
        builder.Property(x => x.Audience).HasColumnName("audience");
        builder.Property(x => x.Active).HasColumnName("active");
        builder.Property(x => x.EffectiveFrom).HasColumnName("effectivefrom");
        builder.Property(x => x.EffectiveTo).HasColumnName("effectiveto");

        builder.Property(x => x.ClientId).HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Resource).HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Scope).HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Audience).HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();

        // Unique across the full grant key so duplicate active grants fail with 409.
        builder.HasIndex(x => new { x.ClientId, x.AccountId, x.Resource, x.Action, x.Scope, x.Audience }).IsUnique();
    }
}
