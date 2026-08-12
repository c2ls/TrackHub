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

using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.AuthorityServer.Infrastructure.Entities;

namespace TrackHub.AuthorityServer.Infrastructure.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        //Table name
        builder.ToTable(name: TableMetadata.Client, schema: SchemaMetadata.Security, t => t.ExcludeFromMigrations());

        //Column names
        builder.Property(x => x.ClientId).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("userid");
        builder.Property(x => x.Name).HasColumnName("name");

        builder
            .HasOne(d => d.User)
            .WithOne(d => d.Client)
            .HasForeignKey<Client>(d => d.UserId)
            .IsRequired(false);

    }
}
