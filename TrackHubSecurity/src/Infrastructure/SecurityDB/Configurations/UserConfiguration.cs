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

namespace TrackHub.Security.Infrastructure.Configurations;

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
        builder.Property(x => x.FirstName).HasColumnName("firstname");
        builder.Property(x => x.SecondName).HasColumnName("secondname");
        builder.Property(x => x.LastName).HasColumnName("lastname");
        builder.Property(x => x.SecondSurname).HasColumnName("secondsurname");
        builder.Property(x => x.EmailAddress).HasColumnName("emailaddress");
        builder.Property(x => x.DOB).HasColumnName("dob");
        builder.Property(x => x.Verified).HasColumnName("verified");
        builder.Property(x => x.Active).HasColumnName("active");
        builder.Property(x => x.LoginAttempts).HasColumnName("loginattempts");
        builder.Property(x => x.LockedUntil).HasColumnName("lockeduntil");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.IntegrationUser).HasColumnName("integrationuser");

        builder.Property(t => t.Username)
            .HasMaxLength(ColumnMetadata.DefaultUserNameLength)
            .IsRequired();

        builder.Property(t => t.Password)
            .HasMaxLength(ColumnMetadata.DefaultPasswordLength)
            .IsRequired();

        builder.Property(t => t.EmailAddress)
            .HasMaxLength(ColumnMetadata.DefaultEmailLength)
            .IsRequired();

        // Authentication resolves a user by emailaddress on every login (AuthorityServer
        // UserReader.GetUserAsync) with SingleOrDefault, and both username and emailaddress are
        // validated for GLOBAL uniqueness by the create/update command validators — so the unique
        // indexes both serve those lookups and close the gap where CreateUser never checked the
        // username at all. AccountId carries the per-tenant user listings.
        builder.HasIndex(x => x.Username)
            .IsUnique()
            .HasDatabaseName("ix_users_username");

        builder.HasIndex(x => x.EmailAddress)
            .IsUnique()
            .HasDatabaseName("ix_users_emailaddress");

        builder.HasIndex(x => x.AccountId)
            .HasDatabaseName("ix_users_accountid");

        //Constraints
        builder
            .HasMany(e => e.Roles)
            .WithMany(e => e.Users)
            .UsingEntity<UserRole>();

        builder
            .HasMany(e => e.Policies)
            .WithMany(e => e.Users)
            .UsingEntity<UserPolicy>();

    }
}
