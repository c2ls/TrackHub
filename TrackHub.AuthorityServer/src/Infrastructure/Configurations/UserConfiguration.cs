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
using TrackHub.AuthorityServer.Infrastructure.Entities;

namespace TrackHub.AuthorityServer.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        //Table name
        builder.ToTable(name: TableMetadata.User, schema: SchemaMetadata.Security);

        //Column names
        builder.Property(x => x.UserId).HasColumnName("id");
        builder.Property(x => x.Username).HasColumnName("username");
        builder.Property(x => x.Password).HasColumnName("password");
        builder.Property(x => x.EmailAddress).HasColumnName("emailaddress");
        builder.Property(x => x.Verified).HasColumnName("verified");
        builder.Property(x => x.Active).HasColumnName("active");
        builder.Property(x => x.LoginAttempts).HasColumnName("loginattempts");
        builder.Property(x => x.LockedUntil).HasColumnName("lockeduntil");
        builder.Property(x => x.AccountId).HasColumnName("accountid");

        builder.Property(t => t.Username)
            .HasMaxLength(ColumnMetadata.DefaultUserNameLength)
            .IsRequired();

        builder.Property(t => t.Password)
            .HasMaxLength(ColumnMetadata.DefaultPasswordLength)
            .IsRequired();

        builder.Property(t => t.EmailAddress)
            .HasMaxLength(ColumnMetadata.DefaultEmailLength)
            .IsRequired();
    }
}
