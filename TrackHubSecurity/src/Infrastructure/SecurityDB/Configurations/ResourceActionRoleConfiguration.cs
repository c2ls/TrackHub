// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Common.Domain.Constants;

namespace TrackHub.Security.Infrastructure.SecurityDB.Configurations;

public class ResourceActionRoleConfiguration : IEntityTypeConfiguration<ResourceActionRole>
{
    public void Configure(EntityTypeBuilder<ResourceActionRole> builder)
    {
        //Table name
        builder.ToTable(name: TableMetadata.ResourceActionRole, schema: SchemaMetadata.Security);

        //Column names
        builder.Property(x => x.ResourceActionRoleId).HasColumnName("id");
        builder.Property(x => x.ResourceId).HasColumnName("resourceid");
        builder.Property(x => x.ActionId).HasColumnName("actionid");
        builder.Property(x => x.RoleId).HasColumnName("roleid");

        builder
            .HasOne(rar => rar.Resource)
            .WithMany()
            .HasForeignKey(rar => rar.ResourceId);

        builder
            .HasOne(rar => rar.ResourceAction)
            .WithMany()
            .HasForeignKey(rap => new { rap.ResourceId, rap.ActionId });

        builder
            .HasOne(rar => rar.Role)
            .WithMany()
            .HasForeignKey(rar => rar.RoleId);

    }
}
