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
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        //Table name
        builder.ToTable(
            name: TableMetadata.Account,
            schema: SchemaMetadata.Application,
            t =>
            {
                t.HasCheckConstraint("ck_accounts_type", EnumColumn.Check<AccountType>("type"));
                t.HasCheckConstraint("ck_accounts_status", EnumColumn.Check<AccountStatus>("status"));
            });

        //Column names
        builder.Property(x => x.AccountId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.Type).HasColumnName("type")
            .HasComment(EnumColumn.Comment<AccountType>("Commercial classification of the tenant."));
        builder.Property(x => x.Active).HasColumnName("active")
            .HasComment("Legacy on/off flag derived from status; true exactly when status is Trial or Active.");
        builder.Property(x => x.Status).HasColumnName("status")
            .HasComment(EnumColumn.Comment<AccountStatus>("Authoritative operational state of the tenant."));
        builder.Property(x => x.StatusChangedAt).HasColumnName("statuschangedat")
            .HasComment("Timestamp of the last status transition; null until the first transition.");

        builder.Property(t => t.Name)
            .HasMaxLength(ColumnMetadata.DefaultNameLength)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(ColumnMetadata.DefaultDescriptionLength)
            .IsRequired();

        // Lifecycle reports/filtering and the DM-05 secondary-index gap.
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Name);

        builder
            .HasMany(e => e.Users)
            .WithOne(e => e.Account)
            .HasForeignKey(e => e.AccountId)
            .IsRequired();

        builder
            .HasMany(e => e.Groups)
            .WithOne(e => e.Account)
            .HasForeignKey(e => e.AccountId)
            .IsRequired();

        builder
            .HasMany(e => e.Operators)
            .WithOne(e => e.Account)
            .HasForeignKey(e => e.AccountId)
            .IsRequired();

        builder
            .HasMany(e => e.Drivers)
            .WithOne()
            .HasForeignKey(e => e.AccountId)
            .IsRequired();

        builder
            .HasMany(e => e.AccountFeatures)
            .WithOne()
            .HasForeignKey(e => e.AccountId)
            .IsRequired();

        builder
            .HasOne(d => d.AccountSettings)
            .WithOne(d => d.Account)
            .HasForeignKey<AccountSettings>(d => d.AccountId)
            .IsRequired(false);

        builder
            .HasOne(d => d.AccountBranding)
            .WithOne(d => d.Account)
            .HasForeignKey<AccountBranding>(d => d.AccountId)
            .IsRequired(false);
    }
}
