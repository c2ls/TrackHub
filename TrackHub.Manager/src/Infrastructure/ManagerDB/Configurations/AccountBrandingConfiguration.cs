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
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class AccountBrandingConfiguration : IEntityTypeConfiguration<AccountBranding>
{
    // Manager-owned; not in the shared TableMetadata to avoid a Common repack for a single name.
    private const string TableName = "account_branding";
    private const int ColorLength = 7; // #RRGGBB

    public void Configure(EntityTypeBuilder<AccountBranding> builder)
    {
        builder.ToTable(name: TableName, schema: SchemaMetadata.Application);

        builder.HasKey(x => x.AccountId);
        builder.Property(x => x.AccountId).HasColumnName("accountid").ValueGeneratedNever();
        builder.Property(x => x.DisplayName)
            .HasColumnName("displayname")
            .HasMaxLength(ColumnMetadata.DefaultNameLength)
            .IsRequired();
        builder.Property(x => x.LogoDocumentId).HasColumnName("logodocumentid");
        builder.Property(x => x.PrimaryColor)
            .HasColumnName("primarycolor")
            .HasMaxLength(ColorLength)
            .IsRequired();
        builder.Property(x => x.ReportHeader)
            .HasColumnName("reportheader")
            .HasColumnType(ColumnMetadata.TextField);
    }
}
