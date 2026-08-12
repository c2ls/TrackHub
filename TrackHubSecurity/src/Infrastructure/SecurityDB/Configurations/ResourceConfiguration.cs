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

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        //Table name
        builder.ToTable(name: TableMetadata.Resource, schema: SchemaMetadata.Security);

        //Column names
        builder.Property(x => x.ResourceId).HasColumnName("id");
        builder.Property(x => x.ResourceName).HasColumnName("name");

        builder.Property(t => t.ResourceName)
            .HasMaxLength(ColumnMetadata.DefaultNameLength)
            .IsRequired();

        //Constraints
        builder
            .HasMany(e => e.ResourceActions)
            .WithOne(e => e.Resource)
            .HasForeignKey(e => e.ResourceId);

    }
}
